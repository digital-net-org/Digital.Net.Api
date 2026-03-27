# US-AUTH-03 : Génération et Gestion de Clé d'API

| Statut |
|:---:|
| `DONE` |
* **En tant que** utilisateur (via le Backoffice)
* **Je veux** pouvoir générer et gérer mes clés d'API
* **Afin de** pouvoir les utiliser dans des scripts ou applications externes pour interagir avec le CMS.

### Critères d'acceptation :

**1. Paramètres de création**
* La clé d'API générée (en clair) ne m'est affichée qu'une seule et unique fois lors de sa création.
* Je dois obligatoirement nommer ma clé d'API. Le nom est soumis aux contraintes suivantes :
  * 64 caractères maximum.
  * Pas de caractères spéciaux (seuls les lettres majuscules et minuscules, les chiffres, les tirets `-`, les underscores `_` et les espaces sont autorisés).
  * Le nom de la clé doit être unique pour un utilisateur donné (un même utilisateur ne peut pas avoir deux clés avec le même nom, mais deux utilisateurs différents peuvent avoir une clé avec le même nom).
* Je peux choisir une date limite de validité pour ma clé, allant de "maintenant" à "indéfiniment". Par défaut, cette durée est fixée à 3 mois.

**2. Limite par utilisateur**
* Je ne peux générer et cumuler qu'un maximum de 5 clés à la fois (indépendamment du fait que mes anciennes clés soient encore actives ou déjà expirées).
* Si je demande la génération d'une nouvelle clé alors que la limite de 5 clés est atteinte, l'API refusera la création et renverra une erreur, m'obligeant à supprimer manuellement une ancienne clé avant de retenter l'opération.

**3. Gestion du cycle de vie**
* Je peux lister mes clés d'API existantes. Cette liste n'affiche pas les valeurs en clair, mais expose uniquement les métadonnées : nom, date de création, et date d'expiration.
* Je peux supprimer ma propre clé d'API à tout moment. Cela révoquera instantanément l'accès aux services qui tentent de l'utiliser.
