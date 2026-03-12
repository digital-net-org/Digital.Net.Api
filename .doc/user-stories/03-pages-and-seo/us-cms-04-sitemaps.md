# US-CMS-04 : Génération et consultation des Sitemaps

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** générateur de rendu front-end (ex: application Next.js)
* **Je veux** pouvoir interroger une API me listant toutes les urls du site
* **Afin de** générer moi-même les fichiers `sitemap.xml` finaux pour les moteurs de recherche.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`. L'architecture étant "Headless", l'API ne sert pas le fichier XML final au public, mais fournit les données structurées à l'application cliente.
> **Pré-requis :** Le développement de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. Agrégation du contenu indexable**
* Le système doit lister exhaustivement toutes les entités configurées comme pages ou articles.
* Seules les entités disposant du statut **Published** = `true` ET **Indexed** = `true` doivent figurer dans le sitemap généré.

**2. Exposition (Application Auth)**
* Un endpoint de consultation interne (ex: `GET /cms/sitemaps/data`) est mis à disposition pour le générateur de rendu (ex: Next.js).
* Cet endpoint nécessite une authentification de type `Application`.
* La fourniture des données est calculée à la volée via cette API et n'est pas cachée.
