# US-CRUD-01 : Opérations de base sur une Entité

* **Statut Backend :** `DONE`
* **Statut Backoffice :** `TO DO`
* **En tant que** développeur / système
* **Je veux** disposer d'un service générique (`ICrudService<T>`)
* **Afin de** pouvoir créer (Create), lire (Get/GetFirst), mettre à jour partiellement (Patch) et supprimer (Delete) n'importe quelle entité du système sans réécrire la logique d'accès aux données.

### Critères d'acceptation :
* Le service doit valider les données entrantes (Create et Patch) via le `ICrudValidationService`.
* La mise à jour partielle doit supporter le format JSON Patch.
* Les méthodes doivent retourner un objet `Result` (ou `Result<T>`) centralisant les éventuelles erreurs ou exceptions (ex: `ResourceNotFoundException`).
