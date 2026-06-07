# US-MUT-05 : Broadcast temps réel des mutations (SSE, multi-pod)

| Statut |
|:---:|
| `DONE` |
* **En tant que** application cliente (front Next.js, backoffice)
* **Je veux** être notifiée en temps réel des mutations d'entités via un flux SSE au contrat stable
* **Afin d'**invalider mon cache (pages HTML, cache mémoire) de manière réactive.

> **Note technique :** Dépend de [US-MUT-04](./us-mut-04-mutation-interceptor.md). L'ancienne infra SSE des events nommés (`EventSignalService` / `SseStreamService` + endpoint `cms/events/stream`) a été **supprimée** par [US-MUT-08](./us-mut-08-authevent-extraction.md) ; ce ticket reconstruit un pipeline SSE **dédié aux mutations**.

> **Note utilisateur (tranchée le 7 juin 2026) :**
> 1. **Un seul** endpoint `GET /events/stream/mutation` dans **Core.Http** (plus rien côté Cms.Http), piloté uniquement par `EntityMutation`. Le `State`/status n'est pas pertinent (audit ≠ SSE).
> 2. **Multi-pod dès le départ via Postgres `LISTEN`/`NOTIFY`** (pas de bus in-process : il ne diffuserait qu'aux clients du pod qui a traité la mutation). Postgres le permet et on écrit déjà la mutation en DB transactionnellement → zéro nouvelle infra.

### Contexte multi-pod (le « pourquoi »)
Un bus **in-process** est par pod : un client connecté au pod A ne reçoit pas les mutations traitées par le pod B → invalidation de cache non fiable derrière un load balancer. Les sticky sessions n'y changent rien (les mutations surviennent sur tous les pods). Il faut un **backplane partagé**. Rappel : la **persistance** (`EntityMutation`) et le **rate limiting** (`AuthEvent`) sont déjà pod-safe (DB partagée) ; **seul le fan-out temps réel** doit être traité ici.

### Critères d'acceptation :

**1. Signal & contrat SSE**
* `MutationSignal` porte `ChangeType`, `EntityType`, `EntityId` (+ timestamp).
* Endpoint unique `GET /events/stream/mutation` (**Core.Http**), authentifié (type `Application` pour le front, cf. US-AUTH-05).
* Type d'event SSE fixe : `mutation`. Payload `data` JSON stable/versionnable : `{ "type": "...", "entity": "...", "entityId": "..." }`.

**2. Filtrage par consommateur**
* Le flux est filtrable par `EntityType` (front = entités publiées `Page`/`Article`… ; backoffice = entités d'admin).

**3. Backplane multi-pod — Postgres `LISTEN`/`NOTIFY`**
* L'interceptor (`MutationTrackingInterceptor`) émet un `NOTIFY` sur le canal `mutation` **dans la transaction** de la mutation → Postgres ne le délivre qu'au **commit** (rien si rollback). Payload `{type, entity, entityId}` (≤ 8 Ko, OK).
* Chaque pod exécute un `BackgroundService` avec une **connexion dédiée** `LISTEN mutation` (Npgsql) ; à réception, il relaie au `SseStreamService` local qui fan-out vers **ses** clients connectés.
* **Tout** passe par `NOTIFY`, y compris le pod d'origine (qui s'écoute lui-même) → un seul chemin, pas de double-livraison.
* Abstraction `IMutationBroadcaster` (impl. Postgres NOTIFY) pour pouvoir basculer vers Redis Pub/Sub ou un hub SSE dédié plus tard **sans impact client** (contrat figé).

**4. Reconnexion & catch-up (sans perte d'invalidation)**
* `Last-Event-Id` mappé au curseur **`(CreatedAt, Id)`** — ordre total **cross-schéma** (le flux fusionne `digital_net` + `digital_net_cms`). Le `bigint` séquentiel est **écarté** : une séquence par table ne peut pas ordonner deux schémas. Index `(CreatedAt, Id)` sur `EntityMutation` ajouté (cf. [US-IDX-01](../06-database-indexes/us-idx-01-index-strategy.md)).
* À la (re)connexion : rejeu depuis `EntityMutation WHERE curseur > lastSeen` (**DB partagée → pod-agnostique et durable**) → aucune invalidation perdue même si un `NOTIFY` a été raté.
* `NOTIFY` = réveil basse latence ; `EntityMutation` = source de vérité.

**5. Résilience**
* Best-effort : aucune mutation bloquée si aucun client connecté, si une écriture SSE échoue, ou si un `NOTIFY` n'est pas délivré (rattrapé au rejeu).

**6. Infra / déploiement**
* L'ingress / LB doit supporter le **streaming SSE** : pas de buffering de réponse, timeouts longs, pas de compression bufferisée.
* Sticky sessions = optionnelles (le catch-up DB les rend non-critiques).
* *Note hors-scope :* le `GlobalLimiter` ASP.NET est en mémoire **par pod** (limite globale réelle = N × `PermitLimit`) — à reconsidérer si une limite globale stricte est requise.

> **Plafond :** `LISTEN`/`NOTIFY` convient largement à l'échelle visée (backoffice + invalidation cache front). Au-delà (dizaines de milliers de connexions SSE), l'étape suivante serait un **hub SSE dédié** (les clients s'y connectent, pas aux pods applicatifs) alimenté par Redis Streams — d'où l'abstraction `IMutationBroadcaster`.

---

## Réalisation (DONE — 8 juin 2026)

Pipeline complet livré, build vert, suite **502 verte** (9 tests ajoutés).

### Composants
- **Core** (`Services/Mutations/`) : `MutationSignal` (record `{ChangeType, EntityType, EntityId, CreatedAt, Id}`), `MutationCursor` (`(CreatedAt, Id)` ↔ `Last-Event-Id`), `IMutationBroadcaster` + `PostgresMutationBroadcaster` (`SELECT pg_notify('mutation', …)` paramétré).
- **Interceptor** (`MutationTrackingInterceptor`) : stash des signaux en `SavingChanges`, émission **post-commit** en `SavedChangesAsync` (best-effort try/catch). Dans une transaction explicite (ex. `CrudService.Patch`) le `pg_notify` est enrôlé dans la tx → Postgres ne le délivre qu'au commit (rien si rollback) ⇒ comportement voulu du critère 3.
- **Core.Http** (`Services/Mutations/`) : `SseStreamService` (registre clients `ConcurrentDictionary`, framing `id:`/`event:`/`data:`, filtre `EntityType`, queue bornée par client, **dédup catch-up↔live** par `Id`), `MutationStreamListener : BackgroundService` (connexion Npgsql dédiée, `LISTEN mutation`, reconnexion + backoff), `IMutationCatchupReader` + impl (SQL brut **UNION ALL** sur les 2 schémas via la connexion partagée — Core.Http ne référence pas `CmsContext`).
- **Endpoint** `GET /events/stream/mutation` (`MutationStreamEndpoints`) : `?entity=Page,Article`, `Last-Event-Id`, ordre **anti-race** (register live → catch-up → live dédupliqué), auth `Application | Jwt | ApiKey` + rate limiting.
- **Index** `(CreatedAt, Id)` sur `EntityMutation` (migration `AddEntityMutationCursorIndex`, 2 contextes).
- **Parent-touch** : `CrudService.Patch` marque toujours la racine `Modified` (`Update(entity)`) ⇒ un patch ne touchant que du sous-contenu/pivot émet quand même une mutation `Page`/`Article` Updated.

### Décisions d'implémentation
- **Curseur `(CreatedAt, Id)`** (et non `bigint`) — comparable cross-schéma.
- **Catch-up SQL brut** (UNION 2 schémas) — garde l'endpoint en Core.Http sans coupler `CmsContext` ; `LISTEN`/`NOTIFY` étant scopé **base**, un seul listener couvre les 2 schémas.
- **NOTIFY post-commit best-effort** ; non-perte garantie par le **catch-up** (curseur durable en DB).

### Tests (9)
`MutationStreamListenerTest` (wire `LISTEN`/`NOTIFY` réel : `pg_notify` → listener → fan-out), `MutationCatchupReaderTest` (cross-schéma + filtre + curseur), `SseStreamServiceTest` (framing + filtre), `MutationParentTouchTest` (patch openGraph → mutation `Page`), broadcaster appelé / non-appelé au commit (`MutationTrackingInterceptorTest`).

### Suivi / limites connues
- **Catch-up plafonné à 1000** : un client hors-ligne très longtemps pourrait manquer un fragment au-delà (best-effort ; full-flush client conseillé sur grosse coupure). À affiner si besoin.
- **Diff sur-rapporté pour les patches** : `Update(entity)` marque tous les scalaires `IsModified` → le `Payload` d'une mutation issue d'un patch liste tous les champs. Sans impact SSE (cache) ; à affiner pour l'audit (US-MUT-07) via le change-tracking.
- **Infra/déploiement** (critère 6) : config ingress/LB SSE (no buffering, timeouts longs) = côté ops.
