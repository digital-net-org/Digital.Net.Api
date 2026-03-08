# US-BLOG-01 : Création d'un article

* **Statut Backend :** `TO DO`
* **Statut Backoffice :** `TO DO`
* **En tant que** rédacteur / utilisateur (via le Backoffice)
* **Je veux** pouvoir créer un article de blog avec un titre, un résumé (excerpt), un contenu riche (Rich Text / Markdown) et une image de couverture (liée via le module `Document`)
* **Afin de** publier des actualités sur mon site.

### Critères d'acceptation :
* L'article doit générer automatiquement son URL (`slug`) à partir du titre s'il n'est pas fourni manuellement.
* L'article doit pouvoir étendre le modèle `Page` (ou l'imiter) pour bénéficier des métadonnées SEO (Titre SEO, Open Graph, JsonLD).
