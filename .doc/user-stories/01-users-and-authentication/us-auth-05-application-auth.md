# US-AUTH-05 : Authentification de type "Application"

| Statut Backend | Statut Backoffice |
|:--------------:|:-----------------:|
|    `DONE` ✅    |       `N/A`       |
* **En tant que** développeur / architecte système
* **Je veux** ajouter un nouveau type d'authentification "Application"
* **Afin de** permettre à des systèmes de confiance (comme le générateur de site statique Next.js) d'accéder à des endpoints spécifiques (ex: liste des urls pour le sitemap) sans utiliser de compte utilisateur classique.

> **Note technique :** Cette fonctionnalité vient enrichir le système d'authentification Core existant (`Digital.Net.Api/Services/Authentication`). 

### Critères d'acceptation :

**1. Nouveau type d'autorisation**
* L'énumération `AuthorizeType` (`Digital.Net.Api/Services/Authentication/Filters/AuthorizeType.cs`) doit être enrichie d'une nouvelle valeur `Application`.
* La valeur `Any` doit être mise à jour pour inclure toutes les méthodes, y compris `Application` si pertinent, ou alors on garde `Any` strict pour les utilisateurs et on crée un `All` ou on gère explicitement le bitmask.

**2. Configuration et clé secrète**
* L'authentification de type "Application" doit s'appuyer sur une clé secrète partagée statique, configurable via les variables d'environnement / `appsettings.json`.
* Par exemple, via une section `Authentication:ApplicationKey` ou similaire.

**3. Validation dans les requêtes**
* Le flux dans `AuthorizationExtensions.cs` doit implémenter la vérification de cette clé.
* Le client (ex: générateur Next.js) doit envoyer cette clé secrète dans un header HTTP spécifique (ex: `X-Application-Key`, à définir lors de l'implémentation).
* Si l'endpoint requiert `AuthorizeType.Application` et que la clé correspond à celle de la configuration, la requête est autorisée indépendamment de toute table d'utilisateurs en base de données.
* Le contexte d'autorisation renvoyé (`AuthorizationResult`) doit indiquer qu'il s'agit d'un accès de type Application et non rattaché à un utilisateur (`UserId` = `null`).
