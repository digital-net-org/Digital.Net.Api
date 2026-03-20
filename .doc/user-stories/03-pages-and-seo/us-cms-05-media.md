# US-CMS-05 : Gestion des Médias (Images)

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `DONE` ✅ | `TO DO` |
* **En tant que** contributeur / administrateur
* **Je veux** pouvoir uploader des images et les servir avec redimensionnement et compression à la volée, avec mise en cache des variantes générées
* **Afin de** fournir des images optimisées et adaptées à chaque contexte d'affichage, compatibles avec le composant `<Image>` de Next.js.

> **Note technique :** Cette fonctionnalité fait partie du module optionnel `Digital.Net.Cms`. Elle s'appuie sur le système de `Document` existant dans `Digital.Net.Core` (proxy pattern). Les médias ne sont pas associés aux pages ou articles : le CMS étant headless, c'est l'application front-end qui décide où et comment les utiliser.
> **Pré-requis :** Le développement de la [US-AUTH-05](../01-users-and-authentication/us-auth-05-application-auth.md) (Authentification "Application") doit être terminé en amont.

### Critères d'acceptation :

**1. Entité Media**
* Un Media agit comme proxy vers un `Document` (même pattern que l'Avatar existant).
* Un Media possède les propriétés suivantes :
  * Un **Nom** (`name` - obligatoire, max 256 caractères) pour identifier le média dans l'interface d'administration.
  * Un **Texte alternatif** (`alt` - optionnel, max 512 caractères) pour l'accessibilité.
  * Un statut **Publié** (`published` - booléen) pour contrôler la visibilité du média. Un média non publié ne doit pas être accessible via l'endpoint de lecture front-end.
  * Une référence vers le `Document` original (image source, non modifiée).

**2. Upload et stockage de l'image originale**
* Lors de l'upload d'une image, le fichier original est sauvegardé tel quel via le `DocumentService` existant (anonymisation du nom, stockage sur le système de fichiers).
* Les formats supportés sont au minimum : JPEG, PNG, WebP, GIF et **SVG**.
* Les images SVG sont des images vectorielles : elles sont stockées telles quelles et **ne sont pas concernées** par le redimensionnement ni la compression (cf. sections 3 et 4).
* La taille maximale d'un fichier est de 10 Mo.

**3. Redimensionnement et compression à la volée**
* L'endpoint de lecture d'une image supporte des **query parameters** pour le redimensionnement et la compression, compatible avec le standard Next.js Image :
  * `w` — largeur cible en pixels (obligatoire pour déclencher le redimensionnement).
  * `q` — qualité de compression de 0 à 100 (optionnel, défaut : 100, c'est-à-dire sans compression).
* Le redimensionnement respecte le ratio d'aspect original de l'image (pas de déformation).
* Si la largeur demandée est supérieure à l'image originale, l'image originale est servie telle quelle (pas d'agrandissement).
* Le format de sortie des variantes compressées est **WebP**.
* Les images **SVG** sont toujours servies telles quelles, les query parameters sont ignorés.

**4. Entité MediaVariant et mise en cache**
* Chaque combinaison unique de Media + largeur (`w`) + qualité (`q`) produit une variante qui est stockée pour les appels suivants.
* Un `MediaVariant` lie un `Media` au `Document` contenant l'image redimensionnée/compressée.
* Propriétés :
  * Référence vers le **Media** parent.
  * Référence vers le **Document** de la variante.
  * **Largeur** (`width` - entier) et **Hauteur** (`height` - entier) effectives de la variante.
  * **Qualité** (`quality` - entier) utilisée pour la compression.
* La combinaison Media + width + quality est unique (index unique).
* Lors d'un appel avec des paramètres déjà traités, la variante en cache est servie directement sans retraitement.

**5. Purge des variantes**
* Un endpoint d'administration permet de purger les variantes mises en cache :
  * **Par média** : supprime toutes les variantes associées à un média donné (l'original est conservé).
  * **Par variante** : supprime une variante spécifique par son identifiant.
  * **Globale** : supprime toutes les variantes de tous les médias.
* La purge supprime à la fois l'entité `MediaVariant` et le `Document` physique associé.

**6. API CRUD et Audit (Administration)**
* Des routes CRUD (Create, Read, Update, Delete) doivent être créées pour gérer les Médias.
* L'upload se fait via `multipart/form-data`.
* La suppression d'un Media entraîne la suppression du Document original et de toutes ses variantes (Documents inclus).
* Chaque action doit générer un événement d'Audit détaillé.

**7. API de lecture (Application, JWT, API Key)**
* Route de lecture : `GET /cms/media/image/{id}.{ext}` avec les query parameters `w` et `q`.
* Si aucun paramètre n'est fourni (ou si le média est un SVG), l'image originale est servie.
* Sinon, la variante correspondante est servie (générée et mise en cache si elle n'existe pas encore).
* Le fichier est servi avec support du cache HTTP (ETag / Last-Modified) via le `DocumentCacheService` existant.
* L'accès est autorisé via les trois types d'authentification : `Application` (front-end), `JWT` et `API Key` (backoffice).
* Un média non publié retourne une erreur 404 pour l'authentification `Application`, mais reste accessible via `JWT` et `API Key` (pour la prévisualisation dans le backoffice).
