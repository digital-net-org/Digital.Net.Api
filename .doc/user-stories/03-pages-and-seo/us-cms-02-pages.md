# US-CMS-02 : Gestion des Pages

| Statut |
|:---:|
| `DONE` |
* **En tant que** contributeur / administrateur
* **Je veux** pouvoir créer, modifier et supprimer des pages sur le site web
* **Afin de** structurer l'arborescence et configurer le référencement (SEO) de l'application.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`.
> **Pré-requis :** Le développement de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. API CRUD et Audit (Administration)**
* Des routes CRUD (Create, Read, Update, Delete) doivent être créées pour gérer les Pages.
* L'accès à ces routes de gestion est réservé aux utilisateurs et administrateurs.
* Chaque action de création, modification ou suppression d'une page doit générer un événement d'Audit détaillé (qui a fait quoi et quand).

**2. API de lecture Front-End (Application Auth)**
* Un ou plusieurs endpoints dédiés à la lecture (ex: `GET /cms/pages/{path}`) doivent être exposés pour servir la donnée au générateur de rendu (ex: Next.js).
* L'accès à ces endpoints dédiés au générateur est sécurisé exclusivement par le type d'authentification `Application` (tel que défini dans la US-AUTH-05).

**3. Informations de base**
* Une page est identifiée de manière unique par un **chemin** (`path`), qui est la seule donnée obligatoire lors de la création d'une page basique.
* Il est possible d'activer ou désactiver une page via un statut **Publié** (`published` - booléen).
* Il est possible d'indiquer si la page doit être indexée par les moteurs de recherche via un statut **Indexé** (`indexed` - booléen).

**4. Gestion des métadonnées (SEO)**
* L'utilisateur peut renseigner des métadonnées optionnelles pour optimiser le référencement :
  * Un **Titre** (`Title`).
  * Une **Description** (`Description`).
  * Des données structurées **JSONLD** (`JsonLd`).

**5. Métadonnées OpenGraph (OG)**
* L'utilisateur peut définir des balises OpenGraph pour le partage sur les réseaux sociaux.
* Ces données sont stockées de façon souple sous forme de liste de paires *propriété/contenu* combinées au sein d'une seule colonne en base de données (ex: au format JSON).

**6. Redirections**
* L'utilisateur peut définir une **Redirection** optionnelle vers une autre URL ou un autre chemin.
* Cette redirection est stockée sous forme d'une simple chaîne de caractères.
