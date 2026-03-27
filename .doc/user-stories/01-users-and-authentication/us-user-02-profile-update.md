# US-USER-02 : Mise à jour du profil utilisateur

| Statut |
|:---:|
| `DONE` |
* **En tant que** utilisateur authentifié
* **Je veux** pouvoir modifier mes informations personnelles (Mot de passe, Nom d'utilisateur, Email)
* **Afin de** maintenir mon compte à jour de manière autonome.

### Critères d'acceptation :

**1. Modification des informations**
* L'utilisateur peut modifier à sa convenance son mot de passe, son nom d'utilisateur ou son adresse email.
* Les nouvelles valeurs fournies (comme la complexité du mot de passe ou l'unicité/format de l'email) sont validées par les mêmes règles de gestion qu'à la création.

**2. Traçabilité**
* Toute modification de ces informations personnelles doit obligatoirement générer un événement d'audit.
* Cet événement tracera avec précision : qui a effectué la modification, quelle information a été modifiée (quoi), et à quel moment (quand).
