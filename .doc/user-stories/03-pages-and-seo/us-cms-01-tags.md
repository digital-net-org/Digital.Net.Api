# US-CMS-01 : Catégorisation des Articles par Tags

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** contributeur / rédacteur
* **Je veux** pouvoir associer des tags à mes articles
* **Afin de** classer le contenu thématiquement et faciliter la recherche par les utilisateurs du site.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`.

### Critères d'acceptation :

**1. Ajout et gestion de Tags**
* Un article peut posséder 0, 1 ou plusieurs tags.
* Un tag est composé de deux éléments :
  * Un **Nom** (chaîne de caractères - obligatoire).
  * Une **Couleur** d'affichage (chaîne de caractères - optionnelle).

**2. Association**
* Lors de la création ou l'édition d'un article, l'utilisateur peut ajouter de nouveaux tags ou en sélectionner des existants.
* Les tags sont partagés entre les articles : si le nom et la couleur sont identiques, la même entité doit être utilisée pour éviter les doublons dans la structure de données.

**3. API CRUD et Audit**
* Des routes CRUD (Create, Read, Update, Delete) doivent être créées pour gérer les Tags au niveau global.
* L'accès à ces routes de gestion est réservé aux utilisateurs et administrateurs.
* Chaque action de création, modification ou suppression d'un tag doit générer un événement d'Audit (traçabilité de l'auteur et de l'action).

