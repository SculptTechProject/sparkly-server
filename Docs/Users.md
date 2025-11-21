# Sparkly Server – Users & Authentication

This document is the slightly chaotic but fully functional guide to how Sparkly Server handles Users and Authentication. If Sparkly were a nightclub, this module would manage the guest list, check IDs, validate wristbands, and occasionally throw someone out when their token expires.

## What a User Is in Sparkly

A User is a simple creature. The domain model represents only what matters:

• Id – primary key, a GUID with attitude.

• Email – unique, required, cannot be nonsense.

• UserName – the name that appears publicly.

• PasswordHash – the encrypted version of whatever password the user typed.

• Projects – navigation to the projects the user owns.

>The entity carries no business logic. It is deliberately boring. That is good.

## Persistence: EF Core Mapping

Users are mapped inside `AppDbContext` to a clean `users` table. The key details:

• Email has a unique index.

• All required fields are enforced.

• Relationship between User → Projects is defined, so ownership is always clear.

>EF Core takes care of the SQL, so you never have to think about migrations while half‑asleep.

## User Repository – Access Without Drama

The repository provides controlled access to the database. It typically allows:

• Finding users by Id.

• Finding users by Email.

• Creating new users.

• Checking email uniqueness.

>The repository keeps the rest of the app blissfully unaware of how the database works.

## Authentication: The Sparkly Way

Sparkly uses JWTs. Nothing exotic. Just clean, industry‑standard authentication.

The flow is straightforward:

1. A user sends credentials.
2. The service checks email and password.
3. If valid, Sparkly issues two tokens:

    • Access Token – short lived, used for API calls.
    
    • Refresh Token – long lived, used to renew sessions.
4. The refresh token is tied to the user, stored in the database, and rotated on use.

>If you lose the refresh token, the session dies. That is life.

## Password Handling

Passwords are hashed using a proper hashing algorithm (no Base64 nonsense). Only hashes are stored. The service handles hashing and verification.

Your password never leaves the service in plain text. The system could not leak it even if it tried.

## UserService – The Traffic Director

This layer does the actual work:

• Registering users.

• Validating credentials.

• Managing tokens.

• Ensuring business rules are respected.

>If something feels too smart for the controller or too specific for the repository, it belongs here.

## DTOs (Data Transfer Objects)

DTOs protect the system from leaking internal structures into the API.

Typical ones:

• UserRegisterDto

• UserLoginDto

• UserDto for returning minimal identity information.

>They shape the data for the outside world so the domain remains clean.

## UsersController – The Doorway

Endpoints typically include:

• POST /users/register – Create a new account.

• POST /users/login – Authenticate and receive tokens.

• POST /users/refresh – Renew access token.

• GET /users/me – Return info about the current user.

>Controllers stay thin. They pass data to services and return results without thinking too much.

## Authorization

Once a user is authenticated, requests carry the Access Token in the Authorization header.

Rules are simple:

• No token → No entry.

• Invalid token → No entry.

• Expired token → Renew using refresh.

>Most protected endpoints have the `[Authorize]` attribute to ensure that only logged‑in users can reach them.

## Error Handling

Authentication failure cases include:

• Wrong email or password.

• Email already registered.

• Expired or missing token.

• Refresh token not found or invalid.

>The API returns appropriate error responses. Clear, predictable, and not misleading.

## Refresh Token Lifecycle

Refresh tokens in Sparkly are treated like long-lived session keys. They live in the database and have a clear lifecycle so they do not turn into immortal zombies.

### Storage

Each refresh token is typically associated with:

• User Id – who owns it.

• Token value – secure random string.

• Expiration date – when it stops being valid.

• Creation time – when it was issued.

• Revocation data (optional) – when and why it was invalidated.

>This allows you to answer questions like: "Is this token still valid?" and "Did we revoke it already?".

### Rotation on Use

Sparkly should use **rotation** when a refresh token is used:

1. Client sends the current refresh token.
2. Server validates it:

    • belongs to an existing user,
   
    • not expired,
   
    • not revoked.
3. If valid:
   • issue a new access token,
   • issue a brand new refresh token,
   • revoke (or mark as used) the old refresh token.

>This rotation means that every refresh token is single-use. Once it is exchanged, it should not work again. This greatly limits the damage if one token gets stolen.

### Invalidation Rules

A refresh token can be invalidated when:

• User logs out.

• User changes password.

• Admin forces logout / security reset.

• Token is used and rotated.

>>Invalidation usually means either deleting the token row from the database or marking a `RevokedAt` / `IsRevoked` field.
>
>For extra security, on critical events (like password change) you can invalidate **all** refresh tokens for the user.

### Compromised Token Scenario

If an attacker steals a refresh token, rotation rules help:

• If the legitimate client uses the token first → attacker’s token becomes useless after rotation.

• If the attacker uses it first → legitimate user’s refresh token stops working, which is a strong signal that something is wrong.

You can then:

• invalidate all tokens for that user,

• force re-login,

• optionally log suspicious activity.

### Expiration Strategy

Typical strategy:

• Access token – short lifetime (minutes).

• Refresh token – longer lifetime (days/weeks).

>This gives a good balance between usability and security. Access tokens are cheap, short-lived, and stateless. Refresh tokens are stateful and more powerful, so they are carefully tracked.

## Summary

Users and Authentication in Sparkly are built to be solid, understandable, and easy to maintain. The architecture enforces separation:

• Domain defines what a User is.

• Repository retrieves data.

• Services execute business logic.

• Controllers expose it cleanly.

• JWTs handle authentication.

>The result is a system where you always know where to look. No magic, just structure.
