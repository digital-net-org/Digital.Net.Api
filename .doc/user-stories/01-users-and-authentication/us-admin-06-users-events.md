# US-ADMIN-06 : Consultation des événements d'un utilisateur

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** administrateur
* **Je veux** pouvoir consulter l'historique des événements (audit) associés à un utilisateur spécifique
* **Afin de** retracer ses actions, diagnostiquer des problèmes ou surveiller l'activité du compte.

### Critères d'acceptation :

**1. Point d'accès (Endpoint)**
* Un endpoint spécifique `GET /admin/user/{id:guid}/events` doit être créé dans la section d'administration (`AdministrationEndpoints.cs`).
* Cet endpoint est strictement réservé aux utilisateurs ayant les droits d'administration.

**2. Pagination**
* La liste des événements retournée doit être paginée pour éviter de surcharger le réseau et la base de données.
* Les requêtes devront supporter les paramètres habituels de pagination (ex: `page`, `pageSize` ou `Skip`/`Take`).

**3. Filtrage temporel (Plage de dates)**
* L'administrateur peut filtrer les résultats selon une plage de dates via des paramètres de requête optionnels :
  * `from` : Date de début (ex: inclure les événements survenus à partir de cette date).
  * `to` : Date de fin (ex: inclure les événements survenus jusqu'à cette date).
* Si aucune date n'est fournie, tous les événements historiques de l'utilisateur sont pris en compte, sous réserve de la pagination.

**4. Filtrage par type d'événement**
* L'administrateur peut filtrer les résultats par le *type d'événement* (ex: un paramètre `eventType` correspondant aux constantes de `UserEvents.cs`).
* **Contrainte de sécurité** : L'endpoint ne doit renvoyer **uniquement** les événements qui sont directement associés à cet utilisateur (vérification via `UserId` lié à l'événement d'audit).
