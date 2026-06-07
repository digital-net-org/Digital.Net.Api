# US-MUT-08 : Extraction `AuthEvent` + suppression du système `Event` générique

| Statut |
|:---:|
| `DONE` |
* **En tant que** architecte backend
* **Je veux** remplacer le fourre-tout `Event` / `AuditService.RegisterEventAsync` par une entité d'auth dédiée et m'appuyer sur `EntityMutation` pour le reste
* **Afin de** clarifier la sémantique d'audit et préparer un SSE purement orienté mutations.

> **Note technique :** Prérequis tranché avant [US-MUT-05](./us-mut-05-realtime-broadcast.md). Depuis [US-MUT-03](./us-mut-03-entity-mutation-model.md)/[US-MUT-04](./us-mut-04-mutation-interceptor.md), les mutations d'entités (CRUD) sont déjà tracées automatiquement par `EntityMutation` ; le système `Event` générique faisait double emploi (CMS/admin/user-CRUD) sauf pour l'auth (login/logout — utilisé par le rate limiting) et le changement de mot de passe.

### Critères d'acceptation :

**1. Entité `AuthEvent`** (`Digital.Net.Core/Entities/Models/Auth/`)
* Hérite `Entity`, implémente `IUntrackedEntity`. Champs : `Type` (`AuthEventType` : Login/Logout/LogoutAll/PasswordChange), `Success` (bool), `Login` (string?), `UserId` (Guid?), `IpAddress` (string? ≤45), `UserAgent` (string? ≤1024).
* Index composite `(IpAddress, Type, Success, CreatedAt)` (sert le comptage de rate limiting).
* `DbSet<AuthEvent>` sur `DigitalContext` (schéma `digital_net`).

**2. `AuthEventService`** (Core.Http, `Services/Authentication`) — seul point d'écriture/lecture
* `RecordAsync(type, success, ip, userAgent, userId, login?)`.
* `CountRecentFailedLoginsAsync(ip)` — porte la requête de rate limiting (3 échecs / 15 min → 429), à l'identique.
* `AuthenticationService` (login/logout/logout-all), l'endpoint `is-locked` et l'endpoint password (UserEndpoints) l'utilisent.

**3. Suppression du système générique**
* Supprimés : `AuditService`/`IAuditService`, `Event`/`EventState`, `EventSignal`/`IEventSignalService`/`EventSignalService`, `SseStreamService`/`ISseStreamService` + endpoint `cms/events/stream`, `EventDto`/`EventQuery`, constantes `*Events` (Auth/User/Cms).
* `RegisterEventAsync` retiré de tous les call sites ; param `eventType` retiré de `MapCrudPost/Patch/Delete`.
* Les actions CMS/admin/user-CRUD reposent désormais sur `EntityMutation` (auteur + IP + diff). Le changement de mot de passe → `AuthEvent(PasswordChange)`.

**4. Diff champ-à-champ dans `EntityMutation`** (renforce US-MUT-03/04)
* Colonne `Payload` (text, JSON) : `{ champ: { from, to } }` pour Modified (champs modifiés), valeurs initiales pour Created, `null` pour Deleted.
* **Exclut les `[Secret]`** (`User.Password`, `ApiKey.Key` marqué `[Secret]`) + ignore `Id`/`CreatedAt`/`UpdatedAt` ; valeurs string longues plafonnées (anti-fuite/anti-bloat, ex. `ConfigValue.Value`).

**5. Migrations & tests**
* `digital_net` : drop `Event`, create `AuthEvent` + index ; `digital_net`+`cms` : add `EntityMutation.Payload`.
* Rate limiting préservé (LoginTest), AuthEvent login/logout/password testés, diff secret-safe testé, suite complète verte.

> **Limite assumée :** les actions **échouées/interdites** (ex. tentative de rétrograder un admin → 403) ne produisent plus d'enregistrement d'audit (EntityMutation ne capture que les mutations réussies ; AuthEvent ne couvre que l'auth). À réintroduire via un « security event » dédié si le besoin se confirme.
