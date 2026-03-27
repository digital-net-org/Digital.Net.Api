# US-DOC-01 : Ajout d'un document via un proxy

| Statut |
|:---:|
| `DONE` |
* **En tant que** développeur/système (ou indirectement via l'utilisateur d'une fonctionnalité proxy)
* **Je veux** pouvoir uploader et associer un document à une entité "proxy" (ex: Avatar)
* **Afin de** stocker le fichier de manière centralisée et sécurisée sans l'exposer par un accès public direct.

### Critères d'acceptation :

**1. Stockage physique et logique**
* Le système doit enregistrer le fichier physiquement sur le disque et créer la référence correspondante via l'entité `Document` en base de données.
* Les métadonnées du fichier, telles que le type MIME, la taille (FileSize) et l'utilisateur à l'origine de l'upload (UploaderId) doivent être conservées.

**2. Anonymisation du fichier**
* Le nom du fichier physique stocké doit être généré aléatoirement (par exemple un UUID) tout en conservant l'extension originale, pour éviter les collisions et masquer le nom d'origine.

**3. Absence d'endpoint direct**
* L'entité `Document` ne doit pas posséder son propre point d'API CRUD public.
* C'est l'entité proxy (qui possède la référence `DocumentId`, comme l'Avatar) qui se charge de porter ce document fonctionnellement.
