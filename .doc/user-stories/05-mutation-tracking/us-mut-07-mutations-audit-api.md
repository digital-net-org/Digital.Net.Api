# US-MUT-07 : API de consultation de l'audit des mutations

| Statut |
|:---:|
| `TODO` |
* **En tant que** administrateur
* **Je veux** consulter l'historique des mutations d'entités (paginé et filtrable)
* **Afin de** retracer *qui a modifié quoi et quand*, en complément de l'API d'audit d'authentification ([US-MUT-09](./us-mut-09-authevent-audit-api.md)).

> **Note technique :** Dépend de [US-MUT-03](./us-mut-03-entity-mutation-model.md) ; même patron que [US-MUT-09](./us-mut-09-authevent-audit-api.md) : `MapPaginationGet` / `MapCrudGet`, DTO + Query dans un namespace *Endpoints*, lecture seule, admin-only. Les colonnes filtrées/triées doivent être **indexées** → cf. [US-IDX-01](../06-database-indexes/us-idx-01-index-strategy.md).

### Critères d'acceptation :

**1. Endpoints**
* `GET /entity-mutations` — liste paginée et filtrable.
* `GET /entity-mutations/{id:guid}` — consultation unitaire.
* Lecture seule, strictement réservés aux administrateurs.

**2. Pagination**
* Paramètres standards hérités de `Query` : `Index`, `Size`, `OrderBy`.

**3. Filtrage**
* `EntityType` (string?), `EntityId` (Guid?), `UserId` (Guid?), `ChangeType` (enum?), `CreatedFrom` / `CreatedTo` (DateTime?).

**4. Agrégation multi-schéma**
* Les mutations étant matérialisées dans `digital_net` **et** `cms` ([US-MUT-03](./us-mut-03-entity-mutation-model.md)), la consultation agrège les deux sources (union, ou paramètre de périmètre).

**5. DTO**
* Un `EntityMutationDto` (constructeur vide + constructeur depuis l'entité), conforme aux conventions de `rules/conventions.md`.
