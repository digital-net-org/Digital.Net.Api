# US-MUT-04 : Interceptor de capture des mutations

| Statut |
|:---:|
| `TODO` |

> **État (impl. 7 juin 2026) :** critères **1-3, 5, 6 livrés** — `MutationTrackingInterceptor` (capture + persistance en **une passe transactionnelle**, grâce aux `Id` générés côté client) **+ diff champ-à-champ secret-safe** dans `EntityMutation.Payload` ([US-MUT-08](./us-mut-08-authevent-extraction.md), exclut `[Secret]`), accessors best-effort `Try*` (`IUserAccessor.TryGetUserId` / `IOriginAccessor.TryGetOrigin` → `null` / `(null,null)` hors HTTP), wiring DI sur les deux contextes via `AddDatabaseContext`, tests verts + suite complète sans régression. **Critère 4 (émission du signal temps réel) reporté à [US-MUT-05](./us-mut-05-realtime-broadcast.md)** (bus repensé : `MutationSignal` dédié). L'interceptor construit les mutations en `SavingChanges` ; l'émission post-commit (`SavedChanges`) sera ajoutée par US-MUT-05.

* **En tant que** développeur backend
* **Je veux** que toute mutation d'une entité tracée (toute `Entity` non marquée `IUntrackedEntity`) soit capturée automatiquement et persistée en `EntityMutation`
* **Afin de** garantir un tracé exhaustif, sans appel manuel, sur n'importe quelle entité suivie.

> **Note technique :** Dépend de [US-MUT-03](./us-mut-03-entity-mutation-model.md) et du lot [04-http-decoupling](../04-http-decoupling/README.md) (`IOriginAccessor`, `IUserAccessor`). Même patron que `TimestampInterceptor`, mais dépendant de services → enregistrement via DI. Après la fusion du lot 04, l'interceptor vit dans `Digital.Net.Core` (namespace `...Entities.Interceptors`).

### Critères d'acceptation :

**1. Capture**
* Un `MutationTrackingInterceptor : SaveChangesInterceptor` (dans `Digital.Net.Core`, namespace `Entities.Interceptors`).
* En `SavingChangesAsync` : collecte des entrées `Entity` **non** `IUntrackedEntity` au state `Added` / `Modified` / `Deleted` (snapshot du type CLR, du `ChangeType` et des `Id` déjà connus).
* En `SavedChangesAsync` : résolution des `EntityId` générés (cas `Created`), puis persistance des lignes `EntityMutation`.

**2. Renseignement de l'origine (via DI, sans dépendance HTTP)**
* `IpAddress` / `UserAgent` proviennent de `IOriginAccessor.GetOrigin()` (contrat `Lib`).
* `UserId` provient de `IUserAccessor` (contrat `Core`).
* Les implémentations HTTP sont fournies par les couches `.Http` (lot 04) ; hors HTTP, ces champs sont `null`.

**3. Multi-contexte**
* L'interceptor est branché sur `DigitalContext` **et** `CmsContext`.
* Les lignes `EntityMutation` sont écrites **dans le contexte courant** (cohérence transactionnelle avec la mutation source).

**4. Émission du signal temps réel**
* Après commit réussi (`SavedChangesAsync`), l'interceptor émet un signal de mutation (voir [US-MUT-05](./us-mut-05-realtime-broadcast.md)) pour chaque ligne `EntityMutation`.
* Aucune émission si le `SaveChanges` échoue ou est annulé.

**5. Enregistrement DI**
* L'interceptor est résolu depuis le `IServiceProvider` (via `AddDbContext(...)` + `AddInterceptors`), avec ses dépendances (`IOriginAccessor`, `IUserAccessor`, `IEventSignalService`).
* `TimestampInterceptor` reste fonctionnel.

**6. Pas de boucle**
* `EntityMutation` et `Event` implémentant `IUntrackedEntity`, leur écriture ne redéclenche aucune capture.
