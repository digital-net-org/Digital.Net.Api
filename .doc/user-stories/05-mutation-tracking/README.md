# Lot — Mutation Tracking (Audit & Temps Réel)

> Statut global : `EN COURS` — faits : 03 / 04 / 05 / 06 / 08 ; reste **US-MUT-07** (API mutations) et **US-MUT-09** (API auth-events).
> **Prérequis : lot [04-http-decoupling](../04-http-decoupling/README.md)** (couches `.Http`, abstractions `IOriginAccessor`/`IUserAccessor`, fusion `Core.Entities` → `Core`).
> Ce lot fait évoluer le système d'événements existant (`Event` / `AuditService` / SSE, livrés via US-CMS-08 et US-ADMIN-06) vers un **tracé générique et automatique des mutations d'entités**, alimentant deux usages distincts.

## 1. Contexte

Le système actuel journalise des **actions métier nommées** (`AUTH_LOGIN`, `ADMIN_CREATE_USER`…) via des appels **manuels** à `AuditService.RegisterEventAsync`, et diffuse les events `CMS_*` en SSE. Deux limites :
- la capture est **manuelle** (un endpoint qui oublie l'appel ne trace rien) ;
- le signal SSE ne porte qu'un **nom d'event**, pas l'identité de l'entité mutée.

## 2. Objectifs

1. **Audit consultable** — tracer dans le temps *quoi / par qui / quand* toute mutation d'entité, automatiquement et génériquement, en complément des actions métier déjà journalisées via `Event`.
2. **Invalidation de cache temps réel** — notifier les mutations aux apps clientes (front Next.js : cache de pages HTML ; backoffice : cache mémoire) pour qu'elles invalident leur cache. Best-effort, payload minimal `{ type, entity, entityId }`.

## 3. Architecture cible

```
SaveChangesInterceptor           ┌─► Persistance EntityMutation (DB) ──► consultation (US-MUT-07)
(capture auto, opt-out IUntrackedEntity) ──┤
                                 └─► Signal temps réel ──► SSE ──► front + backoffice (US-MUT-05)

RegisterEventAsync (manuel, inchangé) ──► Event (actions métier : auth…)
```

- **Deux entités séparées** : `Event` (actions métier, inchangé) et **`EntityMutation`** (nouveau, mutations d'entités).
- **Opt-out** via l'interface marqueur **`IUntrackedEntity`** : toute entité est tracée par défaut, sauf celles qui l'implémentent (dont l'infrastructure d'audit `Event`/`EntityMutation`, marquée pour éviter les boucles infinies).
- **Capture ET émission du signal depuis le même interceptor** (`MutationTrackingInterceptor`), après commit réussi.
- **Origine & identité via DI** : `IpAddress`/`UserAgent` via **`IOriginAccessor`** (contrat `Lib`), `UserId` via **`IUserAccessor`** (contrat `Core`). Les implémentations HTTP sont fournies par les couches `.Http` (lot 04). L'interceptor n'a aucune dépendance HTTP directe → réutilisable par de futures apps non-HTTP.

## 4. Décisions actées

- **Périmètre : Core + CMS.** Les entités à cacher côté front (`Page`, `Article`, `Media`) vivent dans `CmsContext`, séparé de `DigitalContext`. L'interceptor est branché sur **les deux contextes**.
- **Stockage : une table `EntityMutation` par contexte/schéma** (`digital_net` et `cms`), écrite **dans le contexte courant** (même transaction que la mutation → cohérence garantie). Préserve l'autonomie modulaire de `Digital.Net.Cms`. La consultation (US-MUT-07) agrège les deux schémas.
  - *Alternative écartée pour l'instant* : table unique centralisée via un service à contexte dédié (plus simple à requêter, mais transaction séparée → audit best-effort).
- **Prérequis architecture (lot 04)** : le découplage HTTP est livré avant — `IOriginAccessor`/`IUserAccessor` existent et sont injectés par les couches `.Http`, et `Core.Entities` est fusionné dans `Core`.
- **Contrat SSE figé** dès maintenant (URL stable + payload `{ type, entity, entityId }` versionnable) pour qu'un futur passage en multi-instance (hub SSE dédié / Redis) reste **100 % serveur**, sans réécriture côté client.

## 5. Tickets

| # | Ticket | Dépend de |
|---|--------|-----------|
| 03 | [Modèle de tracé (`EntityMutation` + `IUntrackedEntity`)](./us-mut-03-entity-mutation-model.md) | lot 04 |
| 04 | [Interceptor de capture des mutations](./us-mut-04-mutation-interceptor.md) | 03, lot 04 (`IOriginAccessor`, `IUserAccessor`) |
| 05 | [Broadcast temps réel des mutations (SSE)](./us-mut-05-realtime-broadcast.md) ✅ `DONE` | 04 |
| 06 | [Périmètre du tracé — exclusions (`IUntrackedEntity`)](./us-mut-06-tracked-entities.md) ✅ `DONE` | 03 |
| 07 | [API de consultation de l'audit des mutations](./us-mut-07-mutations-audit-api.md) | 03 |
| 08 | [Extraction `AuthEvent` + suppression du système `Event` générique](./us-mut-08-authevent-extraction.md) ✅ `DONE` | 03, 04 |
| 09 | [API de consultation des `AuthEvent`](./us-mut-09-authevent-audit-api.md) | 08 |

> Les anciens US-MUT-01 (découplage `Lib`) et US-MUT-02 (`IOriginAccessor`) ont été **absorbés** par le lot 04 (REF-01 / REF-02).
>
> **US-MUT-08 (livré le 7 juin 2026)** a remplacé le fourre-tout `Event`/`AuditService`/`EventSignal`/SSE par : (a) `AuthEvent` dédié à l'auth (login/logout/mot de passe + rate limiting), (b) `EntityMutation` enrichi d'un **diff** secret-safe pour le reste. Le SSE (US-MUT-05) sera donc **un seul** endpoint `GET /events/stream/mutation` en Core.Http, **multi-pod via Postgres `LISTEN`/`NOTIFY`** (catch-up de reconnexion depuis `EntityMutation`).
>
> **US-MUT-05 (livré le 8 juin 2026)** : ce SSE unique est en place — `LISTEN`/`NOTIFY` (canal `mutation`, scopé base → couvre les 2 schémas), curseur `(CreatedAt, Id)`, catch-up cross-schéma en SQL brut, et **parent-touch** (un patch de sous-contenu émet une mutation du parent).

## 6. Points techniques à trancher à l'implémentation

- **Génération des `Id`** : `Entity.Id` est annoté `DatabaseGenerated(Identity)`. Si l'`Id` est généré côté DB, l'`EntityId` d'un `Created` n'est connu qu'**après** le `SaveChanges` → capture en `SavingChanges`, résolution + persistance en `SavedChanges`. Envisager une génération côté client (Guid séquentiel) pour tout capturer en une seule passe transactionnelle.
- **Enregistrement de l'interceptor via DI** : il dépend de services (`IOriginAccessor`, `IUserAccessor`, `IEventSignalService`) → ne peut pas être instancié en `new` dans `OnConfiguring` comme `TimestampInterceptor`. Passer par `AddDbContext` avec résolution depuis le `IServiceProvider`.
