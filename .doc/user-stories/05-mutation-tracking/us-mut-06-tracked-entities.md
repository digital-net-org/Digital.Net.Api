# US-MUT-06 : Périmètre du tracé — exclusions via `IUntrackedEntity`

| Statut |
|:---:|
| `DONE` |
* **En tant que** mainteneur du modèle de données
* **Je veux** désigner explicitement les entités à **exclure** du tracé et du broadcast
* **Afin de** contrôler le périmètre (opt-out) et éviter le bruit technique.

> **Note technique :** Dépend de [US-MUT-03](./us-mut-03-entity-mutation-model.md). Toute `Entity` est tracée ([US-MUT-04](./us-mut-04-mutation-interceptor.md)) et broadcastée ([US-MUT-05](./us-mut-05-realtime-broadcast.md)) **par défaut** ; l'ajout de `IUntrackedEntity` l'en retire. Les **pivots** (`Pivot<,>`) implémentent `IEntity` mais **pas** `Entity` → exclus **par construction** (le filtre de l'interceptor est `is Entity`), sans marquage.

### Critères d'acceptation :

**1. Tracées par défaut (aucun marquage)**
* Core : `User`, `ConfigValue`, `ApiKey`, `Document`. CMS : `Page`, `Article`, `Media`, `Tag`, `Form`, `FormField`, `FormSubmission`.

**2. Exclues (`IUntrackedEntity`)**
* **Infra d'audit** (anti-boucle / non pertinent) : `ApiToken`, `AuthEvent`, `EntityMutation`.
* **Technique / dérivé** (décidé 7 juin 2026) : `Avatar` (lien User↔Document), `MediaVariant` (variantes générées), `Sheet` + `OpenGraphEntry` (sous-contenu de page édité via le patch de la Page).
* **Pivots** (auto-exclus, non `Entity`) : `ArticleTag`, `ArticleMedia`, `ArticleRelated`, `PageMedia`, `PageSheet`, `PageOpenGraph`.

**3. Vérification**
* `MutationTrackingPerimeterTest` (réflexion) verrouille la partition tracées / exclues / pivots ; `MutationTrackingInterceptorTest` couvre le comportement (entité tracée → ligne `EntityMutation` ; `IUntrackedEntity` → rien).

> ⚠️ **Conséquence pour [US-MUT-05](./us-mut-05-realtime-broadcast.md) :** une modification **uniquement** relationnelle ou de sous-contenu (pivot ajouté/retiré, ou `Sheet`/`OpenGraphEntry` via patch) **ne marque pas le parent `Modified`** → aucune mutation ni signal sur la `Page`/`Article` parent → cache potentiellement périmé. À traiter dans US-MUT-05 (ex. « toucher » le parent lors d'un changement de sous-contenu pour émettre une mutation `Page`/`Article`).
