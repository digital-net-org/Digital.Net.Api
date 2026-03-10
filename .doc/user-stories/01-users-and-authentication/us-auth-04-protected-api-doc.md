# US-AUTH-04 : Protection de la documentation API

| Statut Backend | Statut Backoffice |
| :---: | :---: |
| `TO DO` | `TO DO` |
* **En tant que** utilisateur (via le navigateur ou un script)
* **Je veux** pouvoir consulter la documentation de l'API
* **Afin de** pouvoir l'utiliser dans des scripts ou applications externes pour interagir avec le CMS.

### Critères d'acceptation :

**1. Mode developpement**
* En situation de developpement, je peux consulter la documentation OpenAPI et Scalar sans aucune authentification.

**2. Mode production**
* Je peux consulter la documentation OpenAPI et Scalar en mode production en utilisant une cle d'API valide.

### Notes :
* La documentation OpenAPI et Scalar sont accessibles en mode production.
* Les services concernés sont injectés dans `Digital.Net.Api/DigitalNetInjector.cs`
* Scalar doit être consultable depuis un navigateur web (Firefox, Chrome, etc.)