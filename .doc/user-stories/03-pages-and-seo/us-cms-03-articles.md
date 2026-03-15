# US-CMS-03 : Gestion des Articles

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `DONE` | `TO DO` |
* **En tant que** contributeur / rédacteur
* **Je veux** pouvoir créer, éditer et supprimer des articles
* **Afin de** publier du contenu éditorial riche sur le site.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`. Un Article hérite de toutes les propriétés d'une Page.
> **Pré-requis :** Le développement de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. API CRUD et Audit (Administration)**
* Des routes CRUD de gestion (Create, Read, Update, Delete) doivent être créées pour les Articles, accessibles uniquement aux administrateurs.
* Chaque action de création, modification ou suppression d'un article doit être tracée via un événement d'Audit.

**2. API de lecture Front-End (Application Auth)**
* Un endpoint dédié à la restitution de l'article (ex: `GET /cms/articles/{path}`) doit être exposé pour le générateur de rendu (ex: Next.js).
* Cet endpoint spécifique nécessite l'authentification de type `Application`.

**3. Héritage des propriétés de Page**
* Un article possède toutes les fonctionnalités d'une page classique (cf. `US-CMS-02`) : URI (`path`), statuts `published`/`indexed`, métadonnées SEO, métadonnées OG, et redirections.
* Le paramètre `path` reste obligatoire pour déterminer l'adresse de l'article sur le site.

**4. Informations spécifiques à l'Article**
* Contrairement à une simple page, la création d'un article requiert obligatoirement deux attributs supplémentaires :
  * Un **Nom** (`Name` - chaîne de caractères), qui sert de titre d'affichage pour l'article.
  * Un **Contenu** (`Content` - chaîne de caractères), qui contient le corps du texte (par exemple du format HTML ou Markdown).

**5. Cycle de vie**
* La suppression d'un article supprime également les propriétés héritées de la page associée.
