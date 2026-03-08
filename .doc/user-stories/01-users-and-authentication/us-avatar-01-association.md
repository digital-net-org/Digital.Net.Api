# US-AVATAR-01 : Association et Gestion d'un Avatar

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `DONE` ✅ | `TO DO` |
* **En tant que** utilisateur
* **Je veux** pouvoir ajouter, modifier ou supprimer mon avatar (image de profil)
* **Afin de** personnaliser mon profil.

### Critères d'acceptation :

**1. Ajout ou modification**
* L'utilisateur peut uploader une nouvelle image pour définir ou remplacer son avatar actuel.
* Le fichier uploadé ne doit pas excéder la taille maximale autorisée par la configuration du système (par défaut 2 Mo). Si l'image est trop lourde, elle est refusée avec une erreur explicite.

**2. Suppression**
* L'utilisateur peut faire le choix de supprimer son avatar à tout moment, retirant l'image de son profil.
