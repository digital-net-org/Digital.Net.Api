# US-MUT-09 : API de consultation des `AuthEvent` (écran d'audit connexions)

| Statut |
|:---:|
| `TODO` |
* **En tant que** administrateur
* **Je veux** consulter l'historique des événements d'authentification (login/logout/changement de mot de passe), paginé et filtrable
* **Afin de** surveiller les connexions (succès/échecs, IP) — en complément de l'audit des mutations ([US-MUT-07](./us-mut-07-mutations-audit-api.md)).

> **Note technique :** Dépend de [US-MUT-08](./us-mut-08-authevent-extraction.md). Même patron que l'API mutations (US-MUT-07) : `MapPaginationGet` / `MapCrudGet`, DTO + Query dans un namespace *Endpoints*, lecture seule, admin-only. **Non SSE** (écran d'audit, pas temps réel).

### Critères d'acceptation :

**1. Endpoints**
* `GET /auth-events` — liste paginée et filtrable ; `GET /auth-events/{id:guid}` — consultation unitaire. Lecture seule, admin-only.

**2. Pagination** — `Index`, `Size`, `OrderBy` hérités de `Query`.

**3. Filtrage** — `Type` (enum?), `Success` (bool?), `UserId` (Guid?), `IpAddress` (string?), `CreatedFrom` / `CreatedTo` (DateTime?).

**4. DTO** — un `AuthEventDto` (constructeur vide + constructeur depuis l'entité), conforme à `rules/conventions.md`.
