# US-ADMIN-02 : Suppression d'un utilisateur

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** administrateur
* **Je veux** pouvoir supprimer un utilisateur
* **Afin de** retirer définitivement son accès et ses données du système.

### Critères d'acceptation :

**1. Sécurité de l'opération**
* Pour valider cette action critique, je dois obligatoirement fournir mon propre mot de passe (confirmation d'identité).
* Une sécurité empêche formellement la suppression d'un utilisateur qui possède les droits d'administration. Si une telle tentative survient, elle est bloquée et génère un événement de sécurité (tracé avec : qui a tenté l'action, quand, et sur qui).

**2. Traçabilité**
* La suppression réussie d'un utilisateur ordinaire doit générer un événement d'audit indiquant : qui a été supprimé, quand, et par qui l'action a été effectuée.
