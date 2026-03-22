# US-CMS-08 : Notification des Mutations CMS via SSE (Server-Sent Events)

| Statut Backend | Statut Backoffice |
|:--------------:|:-----------------:|
|    `TO DO`     |       `N/A`       |
* **En tant que** développeur / architecte système
* **Je veux** que l'API broadcast les mutations CMS en temps réel via SSE (Server-Sent Events)
* **Afin de** permettre aux applications clientes abonnées (ex: Next.js) d'être notifiées des changements de données et d'invalider leur cache de manière réactive.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`. Elle s'appuie sur l'authentification de type `Application` ([US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md)) pour sécuriser la connexion SSE.
> **Pré-requis :** Le développement de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. Endpoint SSE**
* Un endpoint `GET /cms/events/stream` doit être exposé, permettant aux clients de s'abonner à un flux SSE persistant.
* Cet endpoint est protégé par l'authentification de type `Application` (`AuthorizeType.Application`).
* La connexion reste ouverte tant que le client est connecté. Aucune donnée n'est envoyée tant qu'aucune mutation ne survient.

**2. Scope des événements**
* Seuls les événements de mutation CMS (`CMS_*` définis dans `CmsEvents.cs`) sont broadcastés via le flux SSE.
* Les événements relatifs à l'authentification (`AUTH_*`), aux utilisateurs (`USER_*`) et à l'administration (`ADMIN_*`) ne sont pas concernés et doivent être exclus du broadcast.

**3. Format des événements SSE**
* Chaque événement envoyé via le flux SSE doit respecter le format standard SSE :
  * `id` : Identifiant incrémental unique de l'événement.
  * `event` : Type fixe `mutation`.
  * `data` : Objet JSON contenant le nom de l'événement CMS (ex: `CMS_UPDATE_ARTICLE`) et le timestamp UTC de la mutation.

**4. Reconnexion et rattrapage**
* Le serveur doit supporter le header `Last-Event-Id` envoyé par le client lors d'une reconnexion.
* Un buffer circulaire en mémoire (derniers ~50 événements) permet de rejouer les événements manqués depuis le dernier ID connu du client.

**5. Résilience et gestion des ressources**
* Les clients déconnectés doivent être automatiquement retirés du registre des abonnés (détection via échec d'écriture ou `CancellationToken`).
* Le broadcast est un no-op lorsqu'aucun client n'est abonné (aucune ressource consommée).
* Le service de broadcast est enregistré en singleton pour partager le registre des clients entre toutes les requêtes.
