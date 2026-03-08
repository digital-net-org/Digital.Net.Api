# US-ADMIN-04 : Activation / Révocation d'un utilisateur

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** administrateur
* **Je veux** pouvoir activer ou révoquer un utilisateur
* **Afin de** bloquer ou autoriser temporairement son accès au système, sans avoir à supprimer ses données.

### Critères d'acceptation :

**1. Sécurité de l'opération**
* Je peux changer le statut d'un utilisateur pour l'activer ou le désactiver à tout moment.
* Une sécurité empêche formellement la révocation (désactivation) d'un utilisateur possédant les droits d'administration. Si une telle tentative survient, elle est bloquée et génère un événement de sécurité (qui a tenté l'action, quand, et sur qui).

**2. Traçabilité**
* Cette action doit générer un événement d'audit indiquant : qui a été activé/désactivé, quand, et par qui l'action a été effectuée.
