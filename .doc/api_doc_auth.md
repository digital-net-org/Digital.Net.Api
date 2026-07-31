# Authentication

How a client proves it's identity to a Digital.Net API, what the server does with that proof, and what a
client *(the shipped JavaScript SDK or one you write yourself)* has to get right.

- [1. Schemes](#1-schemes)
- [2. Using the JavaScript SDK](#2-using-the-javascript-sdk)
- [3. Using a custom client](#3-using-a-custom-client)
- [4. The `DN-Requested-With` header (CSRF)](#4-the-dn-requested-with-header-csrf)
- [5. Session lifecycle](#5-session-lifecycle)
- [6. Login hardening](#6-login-hardening)
- [7. API keys](#7-api-keys)
- [8. Application key](#8-application-key)
- [9. Status codes](#9-status-codes)

---

## 1. Schemes

Three schemes coexist. Every protected route declares the ones it accepts through
`RequireAuthentication(AuthorizeType…)`; the endpoint filter tries them in the order below and stops on
the first that authorizes the request.

| Scheme        | Carrier                     | Identity                | For                               |
|---------------|-----------------------------|-------------------------|-----------------------------------|
| `ApiKey`      | `DN-Api-Key` header         | the key's owner         | machine-to-machine, per user      |
| `Session`     | `dn_session` cookie         | the logged-in user      | browser clients (the back-office) |
| `Application` | `DN-Application-Key` header | none (`Guid.Empty`)     | one trusted server-side consumer  |

A few consequences worth internalising:

- **`Application` carries no user.** Routes that read the caller's identity (`user/self`, API-key
  management, anything behind `RequireAdmin()`) never declare it. It only opens read-mostly public
  content routes.
- **`ApiKey` and `Session` are interchangeable** on nearly every authenticated route, so a script can do
  what the back-office does — with one exception: creating an API key requires a session, to prevent a
  leaked key from minting successors.
- **Order matters only when several schemes are allowed.** Sending both an API key and a session cookie
  is not an error; the API key wins.

When no declared scheme authorizes the request, the API answers `401` **and** clears the session cookie
if one was sent.

---

## 2. Using the JavaScript SDK

The API ships with an isomorphic TypeScript SDK, `@digital-net-org/digital-api-sdk`, which is the
reference client: it is what the back-office use. Prefer it over a hand-rolled client — it already implements everything you need *(hopefully)*.

```ts
import { DigitalApi } from '@digital-net-org/digital-api-sdk';

const api = new DigitalApi({ baseUrl: 'https://api.example.com' });
```

`DigitalApi` wraps a single `HttpClient` and exposes `catalog`, a namespace of typed endpoint groups
(`catalog.auth`, `catalog.user`, `catalog.cms…`). Auth is configured **once, at construction**.

### 2.1. One configuration per kind of consumer

```ts
// Browser app (back-office): session cookie. Nothing to configure — the cookie travels on its own.
new DigitalApi({ baseUrl });

// Server-side consumer (the Nuxt site): shared application key, never exposed to the browser.
new DigitalApi({ baseUrl, applicationKey, applicationKeyAuth: true });

// Script / integration acting as a user: long-lived API key.
new DigitalApi({ baseUrl, apiKey });
```

| Option               | Effect                                                                                        |
|----------------------|-----------------------------------------------------------------------------------------------|
| `apiKey`             | Sends `DN-Api-Key` on every request.                                                          |
| `applicationKey`     | Stored and exposed via `getApplicationKey()`. **Not** sent unless `applicationKeyAuth` is on. |
| `applicationKeyAuth` | Turns the application key into an auth header on every request. Off by default.               |
| `keyPrefix`          | Prepended verbatim to the API-key header name, so several clients can coexist on one origin.  |

> `applicationKey` must never reach code that runs in a browser.

### 2.2. What the SDK does for you

On every request:

- `credentials: 'include'` — the only reason the session cookie is sent cross-origin.
- `DN-Requested-With: digital-net` — the CSRF marker *([see section 4](#4-the-dn-requested-with-header-csrf))*.
- `DN-Client-Id: <per-instance uuid>` — echoed back in mutation signals so a client can ignore its own
  writes. Not an auth header.
- The key headers matching the configuration above, unless the call sets `skipAuth`.

`skipAuth` suppresses the key headers only; the cookie still travels, since `credentials` is what carries
it. `credentials: 'omit'` is the way to make a request truly anonymous.

### 2.3. Login and identity

```ts
await api.catalog.auth.login({ login, password });   // sets the cookie, returns no identity
const self = await api.catalog.user.getSelf();       // the single source of truth
```

`login()` deliberately resolves to `null`. The session is in an `HttpOnly` cookie the SDK cannot read, so
there is no client-side auth state to keep in sync — and nothing to leave stale in `localStorage`.
`user/self` is the identity, and a failing `user/self` *is* the logged-out state.

Other calls: `catalog.auth.logout()`, `catalog.auth.logoutAll()`, `catalog.auth.isLocked()` *(public
pre-check, [see section 6](#6-login-hardening))*.

### 2.4. Reacting to a session that dies mid-flight

A session can expire or be revoked between two requests. The client subscribes once:

```ts
const unsubscribe = api.http.subscribeAuthErrorEvent(() => {
    // Any 401 on an authenticated request. Drop the cached identity and route back to login.
});
```

This is how the back-office avoids the failure mode where the UI still looks logged in while every
request quietly 401s.

### 2.5. Mutation stream (SSE)

`MutationStreamClient` opens `events/mutation/stream` with `fetch` + `credentials: 'include'`, so it
authenticates with the session cookie. It sends no CSRF header and needs none: `GET` is a safe method.
It reconnects with exponential backoff and resumes from `?lastEventId=`.

---

## 3. Using a custom client

Any HTTP client works. What follows is the complete contract.

### 3.1. Session (browser or cookie-jar client)

**Log in.** The response body carries no identity; the session arrives as a `Set-Cookie`.

```bash
curl -i -X POST https://api.example.com/authentication/user/login \
  -H 'Content-Type: application/json' \
  -H 'DN-Requested-With: my-client' \
  -c cookies.txt \
  -d '{"login":"alice","password":"…"}'
```

The `DN-Requested-With` header is **mandatory here too**, even though the route is public.

**Read.** Safe methods need the cookie and nothing else:

```bash
curl -s https://api.example.com/user/self -b cookies.txt
```

**Mutate.** Cookie **and** CSRF header, or the API answers `403`:

```bash
curl -i -X PUT https://api.example.com/user/self/password \
  -H 'Content-Type: application/json' \
  -H 'DN-Requested-With: my-client' \
  -b cookies.txt \
  -d '{"currentPassword":"…","newPassword":"…"}'
```

**Log out.** `POST authentication/user/logout` (this session) or `logout-all` (every device).

Checklist for a browser client:

1. The origin is listed in `CorsAllowedOrigins` — nothing is inferred, an unlisted origin cannot reach
   the API at all.
2. Requests are sent with credentials (`fetch(..., { credentials: 'include' })`,
   `XMLHttpRequest.withCredentials`, `axios({ withCredentials: true })`).
3. Every non-`GET`/`HEAD`/`OPTIONS` request carries `DN-Requested-With` with any non-empty value.
4. Nothing tries to read the cookie: it is `HttpOnly` by design.
5. A `401` on an authenticated call clears the local identity; a `403` does not — it is a bug in the
   client, not an expired session.

### 3.2. API key

Self-contained, no cookie jar, no CSRF header, and exempt from the `SameSite` question entirely:

```bash
curl -s https://api.example.com/user/self \
  -H 'DN-Api-Key: <128-char key>'
```

The key identifies its owner, so the request is subject to the same authorization rules as that user
(including `RequireAdmin()`).

### 3.3. Application key

```bash
curl -s https://api.example.com/cms/articles/public \
  -H 'DN-Application-Key: <shared secret>'
```

Server-side only. It authenticates *an application*, not a person: no user is attached, and only the
public content routes accept it.

---

## 4. The `DN-Requested-With` header (CSRF)

### 4.1. The problem

A cookie is *ambient authority*: the browser attaches it to any request to the API's origin, including
one triggered by a page the user never meant to interact with. `evil.example` can submit a form or fire
a `fetch` at the API, and the session cookie rides along. Nothing about the credential itself
distinguishes "the back-office asked for this" from "another site asked for this on the user's behalf".

The two other schemes do not have this problem: an attacker's page cannot set `DN-Api-Key` or
`DN-Application-Key` any more than it can read them, so a header-carried credential is never sent by
accident.

### 4.2. The guard, and why presence is the whole check

Every *mutating* request authenticated by session must carry `DN-Requested-With`. **Only its presence is
checked** — the value is not a secret, is not verified, and adds no cryptographic strength. The SDK sends
`digital-net`; any non-empty string does.

That works because of what a browser must do before it can set a custom header cross-origin:

1. A custom header makes the request non-simple, so the browser **must** send a `OPTIONS` preflight
   first.
2. The preflight is answered by the CORS policy, which is built from `CorsAllowedOrigins` alone —
   `AddDefaultCorsPolicy` throws at startup if that list is empty, and nothing is ever inferred from
   `ApplicationDomain` or anywhere else.
3. An origin that is not on the list gets no CORS approval, so the browser never sends the real request.

So an attacker's page is left with exactly two options: send the header (blocked at the preflight,
because its origin is not allowed) or omit it (rejected by the API). The security boundary is the CORS
allow-list; the header is what forces every cross-site attempt through it. This is the OWASP
["custom request headers"](https://cheatsheetseries.owasp.org/cheatsheets/Cross-Site_Request_Forgery_Prevention_Cheat_Sheet.html#employing-custom-request-headers-for-ajaxapi)
pattern, and it is why there is no token to issue, store, rotate or double-submit.

### 4.3. What is exempt, and why

| Exempt                            | Reason                                                                                                     |
|-----------------------------------|------------------------------------------------------------------------------------------------------------|
| `GET`, `HEAD`, `OPTIONS`          | They must stay reachable from contexts that cannot set headers: `<img src>`, a plain link, an `EventSource`. Safe methods change no state, so there is nothing to forge. |
| `ApiKey` requests                 | No ambient authority to abuse, and requiring the header would break every non-browser client for no gain.   |
| `Application` requests            | Same reasoning: the public Nuxt server authenticates this way and sends no custom header.                   |

The exemption is decided by the *scheme that actually authorized the request*, not by what the client
sent. A request holding both a cookie and a valid API key is authorized as `ApiKey` and skips the check.

### 4.4. Why login is guarded too

`login` is public — there is no session to protect yet — and it is still behind `RequireCsrfHeader()`,
which applies the same rule to the whole `authentication/user` group.

The attack it stops is **login CSRF**: a third-party page silently logs the victim into *the attacker's*
account, and everything the victim then does happens under an account the attacker can read. `SameSite`
does not help here — it governs whether the browser *sends* an existing cookie cross-site, not whether it
*accepts* a new one from a response. So the login route needs its own guard, and gets it.

`is-locked` is a `GET` and stays exempt, as any safe method does.

### 4.5. `SameSite` is defence in depth, not the guard

The session cookie's `SameSite` is decided at startup, from configuration:

- every entry of `CorsAllowedOrigins` is under `ApplicationDomain` → `SameSite=Lax`;
- at least one is not (a back-office on a different registrable domain) → `SameSite=None`.

`None` is a legitimate deployment, not a weakening — which is precisely why the CSRF guard cannot rest on
`SameSite`. The header check behaves identically in both cases.

### 4.6. Rejection is `403`, never `401`

A missing header is a bug in the client (or an attack), not an expired credential. Answering `401` would
make well-behaved clients — which treat `401` as "the session is gone" — log the operator out on what is
really a transport mistake. `403` says: the session is fine, this particular request is not.

### 4.7. What it does not cover

The guard defends against *cross-site* forgery. It does nothing against script running **on an allowed
origin** — an XSS there can set the header itself. The countermeasures for that are elsewhere: a strict
CSP, an `HttpOnly` cookie no script can exfiltrate, and immediate server-side revocation
*([see section 5](#5-session-lifecycle))*.

---

## 5. Session lifecycle

### 5.1. Creation

`POST authentication/user/login` generates a 64-character CSPRNG identifier over the alphanumeric
alphabet, stores its **SHA-256 hash** in the `Session` table, and returns the clear-text value only in
the `Set-Cookie`. That is the single moment it exists in clear: a database dump yields hashes, and
nothing ever reaches JavaScript, so an XSS cannot steal a session id.

Sessions are opaque and server-side — the token proves nothing by itself, it is a lookup key. Revocation
is therefore immediate and unconditional, which no self-contained token can offer.

### 5.2. Cookie attributes

| Attribute  | Value                              | Why                                                                                              |
|------------|------------------------------------|--------------------------------------------------------------------------------------------------|
| Name       | `dn_session`                       |                                                                                                  |
| `HttpOnly` | yes                                | Unreachable from JavaScript.                                                                     |
| `Secure`   | yes                                | HTTPS only.                                                                                      |
| `Path`     | `/`                                |                                                                                                  |
| `Domain`   | *unset*                            | Host-only: scoped to the API host, never shared with sibling subdomains.                         |
| `SameSite` | `Lax` or `None`                    | Derived from configuration, *[see section 4.5](#45-samesite-is-defence-in-depth-not-the-guard)*. |
| `Expires`  | the **absolute** expiration        | The browser drops it no later than the server would.                                             |

### 5.3. Two deadlines

- **Idle** (`Auth:SessionIdleExpiration`, default 2 h) — slides forward as the session is used, but never
  past the absolute deadline. The write is throttled to at most one per session per 10 minutes, so a busy
  session does not turn every read into a database write.
- **Absolute** (`Auth:SessionAbsoluteExpiration`, default 7 d) — never extended, however active the
  session is. The host refuses to start if the idle window exceeds it, since that would silently disable
  the idle deadline.

An expired session is deleted on the spot, the moment it is presented.

Note that the cookie's own `Expires` is the absolute deadline, set once at login: the browser will keep
sending a session the server may already consider idle-expired. That is intentional — the server decides,
and answers `401`.

### 5.4. Concurrency

An account keeps at most **5** live sessions. Logging in a sixth time evicts the least recently used
ones, ordered by last activity.

### 5.5. Revocation

| Event                   | Effect                                                                                                                                            |
|-------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|
| `logout`                | Deletes this session's row; the next request with that cookie is rejected.                                                                        |
| `logout-all`            | Deletes every session of the account, on all devices.                                                                                             |
| Password change         | Deletes every session **and revokes every API key**; the caller gets a fresh session cookie so they are not logged out of the tab they just used. |
| User deactivated        | Every authorization attempt fails, session rows included.                                                                                         |
| Idle or absolute expiry | Row deleted on the next presentation; `RetentionPurgeService` sweeps the rest.                                                                    |

---

## 6. Login hardening

- **Payload bounds.** `login` 1–48 characters, `password` 1–256, checked before anything touches the
  database.
- **Lockout, two counters.** Over a 15-minute window: **3** failed attempts per **IP**, and **10** per
  **account**. The per-account limit is deliberately looser — a tight one would let anyone lock a known
  account out at will.
- **Constant minimum duration.** Every login takes at least **5 seconds**, success or failure. Response
  time cannot be used to tell "unknown login" from "wrong password", and it caps the practical rate of
  online guessing. Disabled in the `Test` environment only.
- **Pre-check.** `GET authentication/user/is-locked` reports whether the caller's IP has reached the
  threshold, so a UI can say so instead of making the user wait five seconds for a refusal.
- **Audit.** Every attempt is recorded as an `AuthEvent` (type, success, IP, user agent, submitted
  login), which is what the lockout counters are computed from. Logout, logout-all and password changes
  are recorded too.
- **Rate limit.** The default fixed-window policy (200 req/s per client IP) applies on top, over the
  whole `authentication/user` group.
- **Answers.** Bad credentials, unknown login and inactive account are indistinguishable from outside:
  all `401`. Lockout is `429`.

---

## 7. API keys

Managed under `user/self/api-key`.

| Rule                     | Value                                                                        |
|--------------------------|------------------------------------------------------------------------------|
| Creation                 | `POST` — **session only**, an API key cannot mint another one.               |
| Plaintext                | 128 CSPRNG alphanumeric characters, returned **once**, at creation.          |
| Storage                  | SHA-256 hash; `List` returns metadata only.                                  |
| Per user                 | 5 maximum.                                                                   |
| Expiration               | 90 days by default, 180 maximum. A key past `ExpiredAt` is refused.          |
| Name                     | `^[a-zA-Z0-9 _-]{1,64}$`.                                                    |
| Revocation               | `DELETE /{id}`, or automatically on the owner's password change.             |
| Deactivated owner        | The key stops working.                                                       |

## 8. Application key

A single shared secret, `Auth:ApplicationKey`, validated at startup (the host throws if it is missing or
blank) and compared in constant time — `CryptographicOperations.FixedTimeEquals` over the SHA-256 of both
sides.

> It authenticates one trusted server-side consumer. It grants no user identity and opens only the public
content routes. **It must never be handed to a browser or to any build artefact that
ships to a browser** — anyone holding it can read every public content route directly.

