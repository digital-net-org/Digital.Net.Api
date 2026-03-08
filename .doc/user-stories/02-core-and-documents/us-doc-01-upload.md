# US-DOC-01 : Enregistrement d'un Document

* **Statut Backend :** `DONE` (Partiellement)
* **Statut Backoffice :** `TO DO`
* **En tant que** utilisateur (via le Backoffice)
* **Je veux** pouvoir uploader un fichier (image, pdf, etc.)
* **Afin de** le stocker et de pouvoir l'utiliser plus tard dans mes contenus.

### Critères d'acceptation (Actuels) :
* Lors de l'upload, un nom de fichier unique et anonymisé (UUID) est généré.
* Les métadonnées du fichier (Type MIME original, taille en octets) sont sauvegardées dans la base de données.
* Le document est lié à l'utilisateur qui l'a uploadé (`UploaderId`).
