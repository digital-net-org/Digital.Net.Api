# US-USER-01 : Consultation du profil utilisateur

| Statut Backend | Statut Backoffice |
|:--------------:|:-----------------:|
|    `DONE` ✅    |      `TO DO`      |
* **En tant que** utilisateur authentifié
* **Je veux** pouvoir lire les informations de mon profil
* **Afin de** vérifier mes données personnelles enregistrées dans le système.

### Critères d'acceptation :

**1. Données retournées**
* L'API doit retourner les informations suivantes sur le profil de l'utilisateur courant :
  * Nom d'utilisateur (Username)
  * Adresse email
  * Statut du type de compte (Administrateur ou non)
  * L'image d'avatar actuelle (fichier/lien) si elle existe.

---

### Notes Techniques (Front-end)

L'API de récupération d'images lié a des documents déstinés au backoffice est protégée par un token JWT (Bearer). Il n'est donc pas possible d'injecter directement l'URL dans une balise `<img src="..." />` (le navigateur ne pouvant pas joindre de header `Authorization` personnalisé à cette requête).

A la place, le client (SPA/Javascript) doit :
1. Fetch le binaire de l'image (Blob) en envoyant le header `Authorization`.
2. Créer une URL locale dynamique via `URL.createObjectURL(blob)`.
3. Assigner cette URL locale à l'élément `<img>`.
4. Révoquer l'URL locale une fois l'image chargée pour libérer la RAM (`URL.revokeObjectURL(...)`).

Le call respecte le cache du navigateur : même si c'est géré via `fetch`, le backend C# peut évaluer les eTag (`If-None-Match`) et renvoyer une réponse extrêmement légère (`304 Not Modified`), laissant ainsi la promesse `fetch()` extraire le Blob instantanément depuis le cache disque local sans consommer de réseau.

#### Exemple d'intégration (Vanilla JS) :

```javascript
async function loadProtectedAvatar(documentId, imgElement) {
    const token = localStorage.getItem("access_token");

    try {
        const response = await fetch(`https://api/image/${documentId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!response.ok) throw new Error("Avatar indisponible");

        const imageBlob = await response.blob();
        const objectUrl = URL.createObjectURL(imageBlob);

        imgElement.onload = () => {
            URL.revokeObjectURL(objectUrl);
            imgElement.onload = null; // Clean up listener
        };

        imgElement.src = objectUrl;

    } catch (error) {
        console.error(error);
        // imgElement.src = "/assets/default.png";
    }
}
```
