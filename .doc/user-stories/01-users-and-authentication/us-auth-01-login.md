# US-AUTH-01 : Connexion, Tokens et Session Utilisateur

| Statut Backend | Statut Backoffice |
|:--------------:|:-----------------:|
|    `DONE` ✅    |      `TO DO`      |
* **En tant que** utilisateur (ou application frontend)
* **Je veux** m'authentifier avec mes identifiants (Login et Mot de passe)
* **Afin de** créer une session sécurisée et accéder aux endpoints restreints de l'API.

### Critères d'acceptation :

**1. Validation et Sécurité des accès**
* L'API valide l'existence de l'utilisateur et la correspondance du mot de passe haché.
* Seuls les utilisateurs marqués comme actifs sont autorisés à se connecter.
* Une erreur non spécifique est retournée en cas d'identifiant ou de mot de passe incorrect, pour éviter les fuites d'informations.

**2. Protection contre le Brute Force**
* Le système trace chaque tentative de connexion (succès et échecs) via le service d'Audit, en enregistrant systématiquement l'adresse IP et l'appareil de l'utilisateur.
* Au-delà de 3 tentatives de connexion échouées en moins de 15 minutes pour un même utilisateur depuis la même adresse IP, l'accès est bloqué de façon temporaire et l'API retourne une erreur appropriée.

**3. Génération et Stockage des Tokens**
* En cas de succès, l'API génère deux éléments cryptographiques distincts :
    * Un **Bearer Token (JWT)**, renvoyé directement dans la réponse (Payload) pour authentifier les appels API ultérieurs.
    * Un **Refresh Token**, pour permettre de renouveler la session.
* La durée de validité de ces tokens est paramétrable via la configuration du serveur.
* L'identifiant de la session (Refresh Token) est sauvegardé en base de données en conjonction avec l'identifiant de l'utilisateur, les informations de son appareil et la date d'expiration.
* Le Refresh Token est impérativement délivré via un **Cookie HTTP sécurisé** afin de minimiser les risques d'attaques XSS.

**4. Cycle de vie de la session (Endpoints complémentaires)**
* Le nombre de sessions simultanées actives par utilisateur est plafonné. Lors de l'émission d'un nouveau jeton, si cette limite est atteinte, les plus anciennes sessions actives sont automatiquement révoquées pour maintenir le seuil.
* Un endpoint `/refresh` permet de renouveler le Bearer JWT dynamiquement en validant le cookie Refresh Token, révoquant si besoin l'ancien jeton en base pour assurer une rotation sécurisée.
* Un endpoint `/logout` permet de révoquer le jeton de l'appareil courant et de supprimer le cookie de session du navigateur.
* Un endpoint `/logout-all` permet de révoquer immédiatement la totalité des sessions actives (tous les jetons) associés à l'utilisateur sur tous ses appareils.
