# Cahier des Charges - Digital Net CMS (Headless)

## 1. Présentation Générale

### 1.1. Vision et Objectifs
L'objectif principal de ce projet est de construire un CMS Headless sur mesure servant de socle technique réutilisable pour l'ensemble des projets Web actuels et futurs. La volonté est de s'affranchir des solutions clés en main (qui imposent souvent des limites ou des compromis) pour bénéficier de deux avantages majeurs :
* **Maîtrise totale de l'outil** : Le code évolue uniquement selon les choix architecturaux et techniques du concepteur.
* **Maîtrise du périmètre (Scope)** : Aucune nécessité de contourner les obstacles ou les contraintes d'une solution tierce, le système est conçu pour répondre exactement aux besoins ciblés.

### 1.2. Architecture et Fonctionnement
Le système s'articule autour d'une approche "Headless" :
* **Le Backend (Digital Net API)** : Un socle robuste développé en .NET, exposant des API consommables par n'importe quel client.
* **Le Backoffice (BO)** : Une interface d'administration connectée à l'API, pensée pour les clients finaux.
* **Le Frontend (Sur-mesure)** : Des applications ou sites web développés spécifiquement pour chaque client (ex: en NextJS), qui viennent consommer les données du CMS.

### 1.3. Cible et Utilisateurs
Bien que l'outil soit développé par et pour un développeur (pour faciliter ses déploiements), les utilisateurs finaux du CMS (via le Backoffice) sont les **clients** finaux (qu'ils soient des professionnels du web ou des néophytes). 
L'idée est de leur fournir une interface d'administration claire et dédiée pour gérer le contenu de leur site propre, pendant que la partie présentation (Frontend) est entièrement gérée techniquement par le développeur.

---

## 2. Périmètre de la V1 (MVP)

La première version de ce CMS sera validée via un cas d'usage concret : la migration d'un site client existant (actuellement sous Payload CMS) vers cette nouvelle solution. Il s'agit d'un site vitrine couplé à un blog, propulsé par NextJS.

### 2.1. Fonctionnalités cibles pour la V1
Pour répondre aux besoins de ce premier client (et établir la base des futurs projets), la V1 devra inclure :
* **Paramétrage SEO & Pages Web** : Gestion complète des URL, redirections, balises Meta, Open Graph (OG) et JSON-LD pour chaque page.
* **Génération de Sitemaps** : Création et gestion des sitemaps XML pour le référencement.
* **Moteur de Blog** : Création, édition et publication d'articles de blog.
* **Générateur de Formulaires (App)** : Capacité pour le client de créer des formulaires dynamiques depuis le Backoffice (ex: formulaire de contact), qui seront ensuite implémentés et rendus côté frontend par le développeur.

### 2.2. Base Technique Existante
Le développement s'appuie sur une base de code déjà saine et structurée (Dossier `Digital.Net.Entities` entre autres), qui comprend :
* **Authentification & Sécurité** : Gestion via `ApiToken` et `ApiKey`.
* **Ressources Humaines** : Gestion des `User` et de leurs `Avatar`.
* **Moteur CRUD** : Un `CrudService` générique robuste en place.
* **Documents / Médias** : Un modèle `Document` partiellement implémenté (nécessite l'ajout d'endpoints sécurisés et d'une logique de requêtage).
* **Pages & Metas** : Modèles existants (`Page`, `PageOpenGraph`) servant de fondation pour l'implémentation complète prévue en V1.

---

*La suite de la documentation détaille les User Stories associées à ces différentes fonctionnalités, réparties dans le dossier `user-stories/`.*
