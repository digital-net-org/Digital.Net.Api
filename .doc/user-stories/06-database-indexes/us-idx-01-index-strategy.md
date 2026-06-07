# US-IDX-01 : Stratégie d'indexation (audit global + conventions)

| Statut |
|:---:|
| `DONE` |
* **En tant que** responsable des performances du backend
* **Je veux** une évaluation **globale et délibérée** de l'indexation (tous contextes, toutes entités) + des conventions
* **Afin de** supprimer les *full scans* sur les chemins chauds et éviter les index ad hoc « top dans un cas, nuisibles dans un autre ».

> **Note technique :** Transverse — `DigitalContext` (`digital_net`) **et** `CmsContext` (`digital_net_cms`). Ce ticket **fige les conventions** (référence projet), **corrige les 2 trous existants**, et **documente** les index des features à venir (livrés *avec* elles).

---

## Conventions d'indexation (référence projet)

**Coût d'un index** — non gratuit : (a) **amplification d'écriture** (chaque INSERT/UPDATE/DELETE met à jour *tous* les index de la table), (b) stockage, (c) un index inutilisé = coût pur.

**Règles :**
1. **Un index par requête réelle** qui en a besoin — **jamais « au cas où »** (no spéculatif).
2. **Égalité d'abord, range/tri en dernier** dans un composite (`(EntityType, CreatedAt)`, pas l'inverse).
3. **Règle du préfixe gauche** : `(A,B,C)` sert déjà `A` et `(A,B)` → ne pas créer d'index redondant.
4. **Pas d'index sur un booléen seul** ni un enum peu varié (Seq Scan préféré) → au mieux en **colonne de queue** d'un composite ou **index partiel** (`WHERE x = true`) si requête dominante.
5. **FK = auto-indexées par EF Core** → ne jamais doubler.
6. **`LIKE '%x%'` / `ILIKE`** : non indexables en B-tree → trigram/GIN seulement si volumétrie le justifie. **`LIKE 'x%'`** : B-tree utilisable **uniquement avec `text_pattern_ops`** sous Postgres (collation). → on **n'indexe pas** spéculativement les filtres texte d'admin.
7. **`unique`** dès que c'est un invariant métier (sert aussi de garde-fou de correction).
8. **Tables write-heavy** (`EntityMutation`) : minimiser les index.
9. Entités **par schéma** (`EntityMutation`) : index dupliqués dans les 2 migrations.
10. **Mesurer** (`EXPLAIN (ANALYZE)`) avant d'ajouter ; pas d'index sans requête qui l'utilise.

---

## Inventaire — chemins chauds **déjà couverts**

| Chemin | Couvert par |
|---|---|
| Auth clé API (`ApiKey.Key ==`) | index unique |
| Rate limiting login (`AuthEvent`) | composite `(IpAddress,Type,Success,CreatedAt)` (US-MUT-08) |
| Page publique (`Page.Path ==`) / Article (`Article.Slug ==`) | index unique |
| « enfants d'un parent » (UserId, FormId, MediaId, PageId, ParentId/ChildId…) | **auto-index FK** EF |
| Unicité `User.Username`/`Email`, `ConfigValue.Name` | index unique |

## Corrections livrées (ce ticket)

| 🔴 Trou (Seq Scan, chemin chaud) | Correction |
|---|---|
| `User.Login` — `First(u => u.Login == login)` à **chaque login** ; + aucune unicité en base (bug latent) | **index UNIQUE** `IX_User_Login` |
| `ApiToken.Key` — `First(a => a.Key == hash)` à **chaque refresh / logout-all** | **index UNIQUE** `IX_ApiToken_Key` |

→ Migration `AddAuthLookupIndexes` (`digital_net`). ⚠️ suppose l'absence de doublons existants (vrai en dev/test).

## Index différés (livrés AVEC leur feature, suivant les conventions)
* **`EntityMutation`** : `(EntityType, EntityId, <curseur>)` (drill-down audit) + index sur le **curseur** de rejeu → [US-MUT-07](../05-mutation-tracking/us-mut-07-mutations-audit-api.md) / [US-MUT-05](../05-mutation-tracking/us-mut-05-realtime-broadcast.md). ⚠️ curseur = colonne `bigint` séquentielle recommandée (vs `CreatedAt`, ex-æquo), tranché en US-MUT-05. Table write-heavy → rester minimal.
* **`AuthEvent`** : `(UserId, CreatedAt)` pour l'audit par utilisateur → [US-MUT-09](../05-mutation-tracking/us-mut-09-authevent-audit-api.md).

## On n'indexe PAS
FK (auto), uniques existants, filtres texte admin (préfixe/contains/ILIKE), booléens seuls (`Published`/`Indexed`/`IsActive`/`IsAdmin`), `ChangeType`/`Success`/`Type` seuls (faible cardinalité).
