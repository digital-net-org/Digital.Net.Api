# US-ADMIN-06 : Consultation des événements (API Events)

| Statut Backend | Statut Backoffice |
|:--------------:|:-----------------:|
|    `DONE` ✅    |      `TO DO`      |
* **En tant que** administrateur
* **Je veux** pouvoir consulter l'historique des événements (audit)
* **Afin de** retracer les actions des utilisateurs, diagnostiquer des problèmes ou surveiller l'activité du compte.

> **Note technique :** Cette fonctionnalité crée une API dédiée `/events` (read-only, admin-only) plutôt qu'un endpoint imbriqué dans la section administration, afin de conserver les patterns génériques existants (`MapCrudGet`, `MapPaginationGet` dans `Digital.Net.Entities/Crud/Enpoints/`).

### Critères d'acceptation :

**1. Points d'accès (Endpoints)**
* `GET /events` — liste paginée et filtrable des événements d'audit.
* `GET /events/{id:guid}` — consultation d'un événement par son identifiant unique.
* Ces endpoints sont strictement réservés aux utilisateurs ayant les droits d'administration.
* Les événements sont en **lecture seule** (pas de création, modification ou suppression via l'API).

**2. Pagination**
* La liste retournée est paginée via les paramètres standards hérités de `Query` : `Index`, `Size`, `OrderBy`.

**3. Filtrage**
* `UserId` (Guid?, optionnel) — filtre les événements associés à un utilisateur spécifique.
* `EventType` (string?, optionnel) — filtre par type d'événement (correspond aux constantes définies dans `UserEvents.cs` et `AuthenticationEvents.cs`).
* `CreatedFrom` / `CreatedTo` (DateTime?, optionnel) — plage de dates sur `CreatedAt` (hérités de `Query`).
* Si aucun filtre n'est fourni, tous les événements sont retournés sous réserve de la pagination.
