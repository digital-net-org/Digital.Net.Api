# US-DOC-03 : Suppression d'un document via un proxy

| Statut |
|:---:|
| `DONE` |
* **En tant que** développeur/système (ou indirectement via l'utilisateur d'une fonctionnalité proxy)
* **Je veux** qu'en supprimant le document via l'entité proxy (ou en supprimant l'entité proxy elle-même), le document soit physiquement supprimé
* **Afin de** libérer de l'espace de stockage et maintenir la cohérence des données.

### Critères d'acceptation :

**1. Suppression du fichier physique**
* La demande de suppression d'un document via un appel de service (ex: `RemoveDocumentAsync`) doit rechercher le fichier physique correspondant sur le disque et le supprimer définitivement.

**2. Suppression logique**
* L'entité `Document` correspondante doit être supprimée de la base de données.

**3. Tolérance aux erreurs de suppression**
* Si le fichier physique est déjà introuvable ou supprimé sur le disque au moment où la commande de suppression est lancée, cela ne doit pas bloquer la suppression de l'entité `Document` en base de données (l'opération de nettoyage s'achève tout de même avec succès).
