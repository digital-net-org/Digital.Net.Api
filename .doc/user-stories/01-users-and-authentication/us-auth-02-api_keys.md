# US-AUTH-02 : Authentification par Clé d'API (ApiKey)

| Statut Backend | Statut Backoffice |
|:--------------:|:-----------------:|
|    `DONE` ✅    |      `TO DO`      |
* **En tant que** système, service tiers ou application Frontend
* **Je veux** pouvoir m'authentifier auprès de l'API en utilisant une clé d'API
* **Afin de** requêter le CMS de manière programmatique et sécurisée, sans passer par une authentification classique par mot de passe.

### Critères d'acceptation :

**1. Base et Sécurité**
* Une clé d'API est toujours rattachée à un utilisateur existant. Elle requiert donc que cet utilisateur soit actif pour être considérée comme valide.
* Pour des raisons de sécurité, l'API ne stocke jamais les jetons en clair en base de données. Ils doivent être préalablement hachés cryptographiquement.

**2. Validation des Requêtes API**
* L'API accepte les requêtes authentifiées par Clé d'API via un en-tête HTTP spécifique.
* Une requête sera rejetée si :
  * L'en-tête d'authentification est manquant, invalide ou mal formaté.
  * La clé d'API fournie n'est pas reconnue (après vérification du hachage en base).
  * L'utilisateur propriétaire de la clé d'API a été désactivé entre-temps.
  * La date d'expiration optionnelle de la clé d'API est dépassée.
