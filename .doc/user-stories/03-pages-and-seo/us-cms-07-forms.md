# US-CMS-07 : Formulaires dynamiques

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** contributeur / administrateur
* **Je veux** pouvoir créer et configurer des formulaires depuis le backoffice
* **Afin de** permettre aux développeurs front-end de les implémenter sur le site client (formulaire de contact, demande de devis, inscription, etc.) sans passer par un déploiement.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`. Les formulaires sont headless : le CMS stocke la définition et les soumissions, le rendu est entièrement délégué au développeur front-end. Il n'y a pas d'envoi d'email — chaque soumission déclenche un événement via le système d'audit existant, consultable dans le backoffice via SSE.
> **Pré-requis :** Le développement de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. Entité `Form`**
* Un `Form` représente la définition d'un formulaire configurable depuis le backoffice.
* Propriétés :
  * Un **Nom** (`name` - obligatoire, max 256 caractères) pour identifier le formulaire dans le backoffice.
  * Une **Description** (`description` - optionnel, max 512 caractères).
  * Un statut **Publié** (`published` - booléen) pour contrôler l'accès depuis le front-end. Un formulaire non publié retourne une erreur 404 pour l'authentification `Application`.
  * Un **Libellé du bouton** (`submitLabel` - optionnel, max 128 caractères, défaut : `"Submit"`).
* Un `Form` est composé d'une liste ordonnée de `FormField`.

**2. Entité `FormField`**
* Un `FormField` décrit un champ du formulaire.
* Propriétés :
  * Référence vers le **Formulaire** parent (`formId`).
  * Un **Nom** (`name` - obligatoire, max 64 caractères) servant de clé dans les soumissions (ex: `email`, `message`). Doit être unique au sein d'un même formulaire.
  * Un **Type** (`type` - obligatoire, max 16 caractères) parmi les valeurs suivantes :
    * `text` — champ texte mono-ligne
    * `email` — champ email (format validé automatiquement)
    * `textarea` — champ texte multi-lignes
    * `number` — champ numérique
    * `select` — liste déroulante (choix unique)
    * `radio` — groupe de boutons radio (choix unique)
    * `checkbox` — case à cocher (booléen)
    * `message` — texte informatif affiché dans le formulaire, non soumis
  * Un **Libellé** (`label` - obligatoire, max 256 caractères) affiché à côté du champ.
  * Un **Placeholder** (`placeholder` - optionnel, max 256 caractères).
  * Une **Valeur par défaut** (`defaultValue` - optionnel, max 256 caractères).
  * Un indicateur **Obligatoire** (`required` - booléen).
  * Un **Ordre** (`sortOrder` - entier) pour définir la position du champ dans le formulaire.
  * Des **Règles de validation** (`validationJson` - JSON optionnel) sous la forme `{ "minLength": N, "maxLength": N, "pattern": "regex", "min": N, "max": N }`. Applicables selon le type de champ.
  * Des **Options** (`optionsJson` - JSON optionnel) sous la forme `[{ "label": "Libellé", "value": "valeur" }]`. Obligatoire pour les types `select` et `radio`.

**3. Entité `FormSubmission`**
* Un `FormSubmission` représente une réponse soumise par un utilisateur du site.
* Propriétés :
  * Référence vers le **Formulaire** parent (`formId`).
  * Les **Valeurs soumises** (`valuesJson` - JSON) stockées sous la forme d'un objet clé/valeur : `{ "email": "contact@exemple.fr", "message": "Bonjour" }`. Les champs de type `message` ne sont pas inclus dans les soumissions.
  * L'**Adresse IP** du soumetteur (`submitterIp` - optionnel, max 64 caractères).
  * Le **User Agent** du soumetteur (`userAgent` - optionnel, max 512 caractères).

**4. API CRUD et Audit — Formulaires (Administration, JWT / API Key)**
* Des routes CRUD (Create, Read, Update, Delete) doivent être créées pour gérer les `Form` et leurs `FormField`.
* La suppression d'un `Form` entraîne la suppression en cascade de tous ses `FormField` et `FormSubmission`.
* Chaque action de création, modification ou suppression doit générer un événement d'Audit.

**5. API de gestion des soumissions (Administration, JWT / API Key)**
* Des routes permettent de consulter et supprimer les soumissions :
  * `GET /cms/forms/{id}/submissions` — liste paginée des soumissions d'un formulaire.
  * `GET /cms/forms/{id}/submissions/{submissionId}` — détail d'une soumission.
  * `DELETE /cms/forms/{id}/submissions/{submissionId}` — suppression d'une soumission.

**6. API de lecture — Définition du formulaire (Application)**
* Route : `GET /cms/forms/{id:guid}`
* Retourne la définition complète du formulaire publié (champs, types, labels, options, règles de validation, configuration de confirmation).
* Un formulaire non publié retourne une erreur 404 pour l'authentification `Application`, mais reste accessible via `JWT` et `API Key` (prévisualisation backoffice).

**7. API de soumission (Application)**
* Route : `POST /cms/forms/{id:guid}/submit`
* Reçoit un payload JSON contenant les valeurs soumises : `{ "values": { "email": "a@b.com", "message": "Bonjour" } }`.
* L'API valide les soumissions côté serveur contre la définition du formulaire :
  * Présence des champs obligatoires (`required`).
  * Respect des règles `validationJson` (longueurs, pattern, min/max).
  * Format automatique pour les champs de type `email`.
* En cas d'erreur de validation, retourne une réponse `400 Bad Request` listant les erreurs par champ.
* En cas de succès :
  * Persiste la `FormSubmission` avec l'IP et le User Agent du soumetteur.
  * Déclenche un événement d'audit `CMS_FORM_SUBMISSION` via le système existant, consultable dans le backoffice via SSE.
  * Retourne un `Result` vide en cas de succès.
