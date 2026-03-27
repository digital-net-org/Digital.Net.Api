# US-DOC-02 : Consultation d'un document via un proxy

| Statut |
|:---:|
| `DONE` |
* **En tant que** utilisateur
* **Je veux** pouvoir consulter ou récupérer un document via le point d'API de l'entité proxy qui le référence (ex: mon Avatar)
* **Afin de** garantir que l'accès au fichier est régi par les mêmes règles de sécurité que l'entité proxy elle-même.

### Critères d'acceptation :

**1. Contrôle d'accès délégué**
* L'accès au document dépend exclusivement des autorisations appliquées sur l'endpoint de l'entité proxy (ex: `GET /user/{id}/avatar`).
* Impossible d'accéder au document sous-jacent directement ou de contourner les règles du proxy.

**2. Restitution du flux**
* Le système doit lire le fichier physique et retourner le flux avec le `ContentType` adéquat (MimeType) enregistré lors de l'upload.
* Le système gère le paramètre HTTP `Last-Modified` ou des mécanismes de cache (ex: via `IDocumentCacheService`) pour optimiser et renvoyer une réponse `304 Not Modified` quand le fichier n'a pas changé.

**3. Résilience en cas de fichier manquant**
* Si l'entité `Document` existe en base mais que le fichier physique n'est pas trouvé (supprimé accidentellement sur le disque), le système retourne une erreur transparente comme `404 Not Found`.
