# US-ADMIN-05 : Gestion des rôles d'administration

| Statut |
|:---:|
| `DONE` |
* **En tant que** administrateur
* **Je veux** pouvoir donner les droits d'administration à un autre utilisateur
* **Afin de** déléguer la gestion du CMS à d'autres collaborateurs de confiance.

### Critères d'acceptation :

**1. Sécurité de l'opération**
* Pour valider cette action sensible d'élévation de privilèges, je dois obligatoirement fournir mon propre mot de passe (confirmation d'identité).
* Une sécurité empêche formellement de retirer les droits d'administration d'un utilisateur qui les possède déjà (un admin reste admin). Si une telle tentative de rétrogradation survient, elle est bloquée et génère un événement de sécurité (qui a tenté l'action, quand, et sur qui).

**2. Traçabilité**
* Cette action doit générer un événement d'audit qui consigne de manière précise : qui a reçu ou perdu ses droits d'accès, quand, et par qui l'action a été effectuée.
