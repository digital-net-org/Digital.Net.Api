# US-MUT-03 : Modèle de tracé (`EntityMutation` + `IUntrackedEntity`)

| Statut |
|:---:|
| `DONE` |
* **En tant que** architecte de données
* **Je veux** une entité dédiée à l'historique des mutations et une interface marqueur désignant les entités à **exclure** du tracé
* **Afin de** disposer d'un schéma structuré et homogène (distinct des `Event` métier) couvrant *quoi / par qui / quand*.

> **Note technique :** Périmètre **Core + CMS**. Le modèle est défini dans `Digital.Net.Core.Entities` (accessible aux deux contextes). Une table `EntityMutation` est matérialisée **dans chaque contexte/schéma** (`digital_net` et `cms`).

### Critères d'acceptation :

**1. Interface marqueur**
* `IUntrackedEntity` : interface marqueur vide, dans `Digital.Net.Core.Entities`.
* Toute entité (`Entity`) est tracée **par défaut** (opt-out) ; elle est exclue si et seulement si elle implémente `IUntrackedEntity`.

**2. Enum `ChangeType`**
* Valeurs : `Created`, `Updated`, `Deleted`.

**3. Entité `EntityMutation`**
* Hérite de `Entity` (récupère `Id`, `CreatedAt` — qui matérialise le *quand* —, `UpdatedAt`).
* Propriétés : `ChangeType` (enum), `EntityType` (string — nom CLR de l'entité mutée), `EntityId` (Guid), `UserId` (Guid?, nullable), `IpAddress` (string?, nullable), `UserAgent` (string?, nullable), `Payload` (string?, JSON diff des champs modifiés — ajouté par [US-MUT-08](./us-mut-08-authevent-extraction.md), exclut `[Secret]`).
* Implémente `IUntrackedEntity` (jamais auto-tracée → pas de boucle de capture). `Event` est marquée de même.

**4. Mapping & contextes**
* `DbSet<EntityMutation>` ajouté à `DigitalContext` (schéma `digital_net`) **et** à `CmsContext` (schéma `cms`).
* Tailles de colonnes cohérentes avec `Event` (`IpAddress` ≤ 45, `UserAgent` ≤ 1024).

**5. Migrations**
* Migration EF Core pour `DigitalContext` (schéma `digital_net`).
* Migration EF Core pour `CmsContext` (schéma `cms`).
