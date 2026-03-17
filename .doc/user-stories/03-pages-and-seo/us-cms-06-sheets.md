# US-CMS-06 : Sheets (CSS / JS)

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** contributeur / administrateur
* **Je veux** pouvoir créer et éditer des feuilles de code CSS et JavaScript depuis le backoffice et les associer à des pages ou articles
* **Afin de** pouvoir injecter des scripts ou styles personnalisés (analytics, tracking, mise en forme spécifique) sur des pages spécifiques, sans passer par un déploiement front-end.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`. Une Sheet est une entité autonome contenant directement le code source (texte), sans passer par le système de `Document`. Le contenu est éditable via un formulaire dans le backoffice. Contrairement aux Médias, les Sheets sont associées aux pages/articles car leur injection est pilotée côté CMS.
> **Pré-requis :** Le développement de la [US-CMS-02](us-cms-02-pages.md) (Pages), de la [US-CMS-03](us-cms-03-articles.md) (Articles) et de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. Entité Sheet**
* Une `Sheet` est une entité qui stocke directement du code CSS ou JS sous forme de texte.
* Propriétés :
  * Un **Nom** (`name` - obligatoire, max 256 caractères) pour identifier la sheet dans l'interface d'administration.
  * Un **Type** (`type` - obligatoire) qui distingue le langage : `css` ou `js`.
  * Un **Contenu** (`content` - obligatoire, texte) contenant le code source CSS ou JavaScript.
  * Un statut **Publié** (`published` - booléen) pour contrôler la visibilité de la sheet. Une sheet non publiée ne doit pas être servie par l'endpoint de lecture front-end ni apparaître dans la liste des sheets d'une page.

**2. Édition depuis le backoffice**
* Le contenu d'une Sheet est créé et modifié directement via les endpoints CRUD (payload JSON), sans upload de fichier.
* Le contenu est stocké tel quel (pas de minification ni de transformation côté serveur).

**3. Association Sheet ↔ Page/Article**
* Une Sheet peut être associée à une ou plusieurs Pages et/ou Articles via une relation many-to-many.
* L'association et la dissociation se font via des endpoints dédiés ou via les payloads de Page/Article.
* Chaque association porte un **Ordre de chargement** (`loadOrder` - entier, par défaut 0) propre à la page concernée pour permettre un ordonnancement différent selon la page.
* Une même Sheet peut être partagée entre plusieurs pages (ex: un script analytics commun).
* La suppression d'une association Page ↔ Sheet ne supprime pas la Sheet elle-même, seulement le lien.

**4. API CRUD et Audit (Administration)**
* Des routes CRUD (Create, Read, Update, Delete) doivent être créées pour gérer les Sheets.
* La suppression d'une Sheet entraîne la suppression de toutes les associations avec les pages.
* Chaque action doit générer un événement d'Audit détaillé.

**5. API de lecture Front-End (Application Auth)**
* Route de lecture : `GET /cms/resource/{id}.{ext}` pour servir le contenu de la sheet avec le bon Content-Type (`text/css` ou `application/javascript`).
* Un endpoint permet de récupérer la liste des sheets (CSS et JS) associées à une page donnée, triées par ordre de chargement. Seules les sheets publiées sont retournées.
* La réponse inclut le type (`css`/`js`), l'URL de la sheet et l'ordre de chargement.
* L'accès à ces endpoints est sécurisé par le type d'authentification `Application`.
* Une sheet non publiée retourne une erreur 404.
