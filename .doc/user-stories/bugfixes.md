# Bugfixes

## BUG-001 — Unique constraint check fails when patching a field to its current value

**Fichier :** `Digital.Net.Entities/Crud/CrudValidationService.cs`
**Méthode :** `ValidateProperty<T>`
**Découvert lors de :** l'écriture de `Patch_Succeeds_WhenPatchingUniqueFieldToSameValue`

### Description

Quand `CrudService.Patch` est appelé, l'entité est d'abord chargée via `FindAsync` (elle entre dans le change tracker d'EF Core). Ensuite, `ValidatePatchPayload` appelle `ValidateProperty` qui vérifie les contraintes d'unicité avec :

```csharp
context.Set<T>().Any(x => EF.Property<object>(x, property.Name).Equals(value))
```

`context.Set<T>().Any(...)` exécute une requête SQL. Si la valeur patchée est identique à la valeur actuelle de l'entité, la requête trouve la ligne de cette entité dans la base → `Any` retourne `true` → `EntityValidationException` est levée, alors qu'il n'y a aucun conflit réel.

### Reproduction

```csharp
var entity = GetTestEntity(); // entity.UniqueField = "abc"
await _context.TestEntities.AddAsync(entity);
await _context.SaveChangesAsync();

// PATCH avec la même valeur — devrait passer
var result = await _service.Patch(CreatePatch(e => e.UniqueField, entity.UniqueField), entity.Id);

// Attendu : result.HasError == false
// Actuel  : result.HasError == true  (EntityValidationException: unique constraint)
```

Le test `Patch_Succeeds_WhenPatchingUniqueFieldToSameValue` dans `CrudServiceTest.cs` est annoté `[Skip]` et documente ce comportement.

### Correction suggérée

Exclure l'entité en cours de modification du check d'unicité. Cela nécessite de passer l'ID de l'entité à `ValidateProperty` :

```csharp
context.Set<T>().Any(x => x.Id != currentEntityId && EF.Property<object>(x, property.Name).Equals(value))
```

`ValidateProperty` ne reçoit pas l'ID actuellement — il faudrait soit l'ajouter en paramètre, soit passer l'entité complète au service de validation au moment du patch.

---

## BUG-002 — `Seeder.BuildQuery` plante sur les propriétés de navigation collection

**Fichier :** `Digital.Net.Entities/Seeds/Seeder.cs`
**Méthode :** `BuildQuery`
**Découvert lors de :** l'ajout de `public virtual List<ApiKey> ApiKeys` sur `User`

### Description

`BuildQuery` itère les propriétés de l'entité et ignore celles qui sont `null`, qui héritent de `Entity`, ou dont le nom est `"Password"`. Il ne filtre pas les propriétés de navigation de type collection (`IEnumerable<T>` non-string).

Quand `ApiKeys` vaut `[]` (liste vide non null), la propriété passe tous les guards existants. Dynamic LINQ tente de construire un prédicat avec `ApiKeys == []`, ce qui génère une exception → `result.HasError = true` pour toutes les entités seedées.

`IsEntity()` retourne `false` pour `List<ApiKey>` car `typeof(List<ApiKey>).BaseType == typeof(object)`, pas `typeof(Entity)`.

### Reproduction

```csharp
// Avant le fix : ajouter List<ApiKey> ApiKeys à User → SeederTest plante
var seeder = new UserSeeder(logger, context);
var result = await seeder.SeedAsync([new User { ... }]);
// result.HasError == true (Dynamic LINQ exception)
```

### Correction appliquée

Ajout d'une condition dans `BuildQuery` pour ignorer les propriétés de type `IEnumerable` (hors `string`) :

```csharp
if (value is null
    || property.PropertyType.IsEntity()
    || property.Name == "Password"
    || (property.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(property.PropertyType)))
    continue;
```

---

## NOTE-001 — Rapport de couverture incomplet

L'analyse automatique de couverture a signalé à tort les cas suivants comme **non couverts** alors qu'ils **l'étaient déjà** :

| Endpoint | Fichier de test existant |
|---|---|
| `DELETE /admin/users/{id}` | `AdministrationEndpointsTest.cs` — `DeleteUser_ShouldDeleteUser` + 3 autres tests |
| `PATCH /admin/users/{id}/status` | `AdministrationEndpointsTest.cs` — `UpdateUserStatus_ShouldActivateUser` + 4 autres tests |
| `PATCH /admin/users/{id}/role` | `AdministrationEndpointsTest.cs` — `UpdateUserRole_ShouldPromoteToAdmin` + 4 autres tests |
| `PATCH /cms/pages/{id}` | `PageEndpointsTest.cs` — `PatchPage_ShouldUpdatePage` + `PatchPage_ShouldRejectOverMaxLengthTitle` |
| `DELETE /cms/pages/{id}` | `PageEndpointsTest.cs` — `DeletePage_ShouldDeletePage` |

Ces cas **n'ont pas été réécrits** pour éviter les doublons.
