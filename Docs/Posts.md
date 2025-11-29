# Posts API Documentation

Posts are short updates attached to a project. They are used to document progress, share context, and keep a public "build in public" log inside Sparkly.

This document describes the domain model and HTTP API for managing posts in `sparkly-server`.

## Domain model

A `Post` represents a single update written by a user, optionally attached to a project.

**Fields**

* `Id` (`Guid`): Unique identifier of the post.
* `AuthorId` (`Guid`): Id of the user who created the post.
* `ProjectId` (`Guid?`): Id of the project this post belongs to. Nullable for potential future global posts.
* `Title` (`string`): Short title or headline of the post.
* `Content` (`string`): Main text body of the post (Markdown-friendly).
* `CreatedAt` (`DateTime`): UTC timestamp when the post was created.
* `UpdatedAt` (`DateTime`): UTC timestamp of the last modification.

**Invariants and behaviour**

* A post must always have a non-empty `Title` and `Content`.
* `CreatedAt` is set once at creation time.
* Every modification calls the internal `Touch()` method, which updates `UpdatedAt` to current UTC time.
* Posts for a project are conceptually ordered by `CreatedAt` from newest to oldest.

## Permissions and authorization

* All endpoints require an authenticated user.
* `AuthorId` is always taken from the authenticated user context; the client does not choose it.
* Only the author (and in the future possibly project owners / admins) may edit or delete a post.
* Read access rules:

    * Project members can read all posts for that project.
    * Public projects may expose posts publicly via separate public endpoints in the future.

The API exposes basic permission flags in the response DTO:

* `CanEdit` (`bool`): `true` if the current user is allowed to edit the post.
* `CanDelete` (`bool`): `true` if the current user is allowed to delete the post.

These flags are computed server-side based on the current user, not stored in the database.

## DTO: PostResponse

`PostResponse` is returned by all read endpoints.

C# shape (conceptual):

```csharp
public sealed class PostResponse
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public Guid AuthorId { get; init; }
    public string Title { get; init; } = default!;
    public string Content { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
}
```

JSON representation (using the global JSON naming policy; here `camelCase` is assumed):

```json
{
  "id": "d7c88f39-2f8b-4b10-8f10-5b93dd0b5c34",
  "projectId": "a3f9d5e2-91b0-4b5a-9530-7f9a6a7a2510",
  "authorId": "4a2537d5-2a7b-4b17-b1a4-2a0fae7b02a1",
  "title": "Shipped the first version of the auth flow",
  "content": "Today I implemented refresh tokens and wired them into the frontend.",
  "createdAt": "2025-11-29T18:30:00Z",
  "updatedAt": "2025-11-29T18:35:12Z",
  "canEdit": true,
  "canDelete": true
}
```

## Base route

All endpoints in this document assume the following controller route:

```text
/api/v1/projects/{projectId:guid}/posts
```

* `projectId` – the `Guid` of the project the posts belong to.

If the actual route in `PostsController` differs, adapt the paths below accordingly.

## Endpoints

### 1. List posts for a project

Returns all posts for the given project, ordered from newest to oldest.

* **Method:** `GET`
* **URL:** `/api/v1/projects/{projectId}/posts`
* **Auth:** required (Bearer token)

**Path parameters**

* `projectId` (`Guid`) – id of the project.

**Query parameters**

Currently none. Pagination can be added later (for example `page`, `pageSize`).

**Responses**

* `200 OK` – list of posts.

Example response:

```json
[
  {
    "id": "d7c88f39-2f8b-4b10-8f10-5b93dd0b5c34",
    "projectId": "a3f9d5e2-91b0-4b5a-9530-7f9a6a7a2510",
    "authorId": "4a2537d5-2a7b-4b17-b1a4-2a0fae7b02a1",
    "title": "Day 3 – basic dashboard UI",
    "content": "Implemented the first version of the dashboard and wired it to the API.",
    "createdAt": "2025-11-28T10:21:00Z",
    "updatedAt": "2025-11-28T10:21:00Z",
    "canEdit": true,
    "canDelete": true
  }
]
```

* `404 Not Found` – if the project does not exist or is not visible to the current user.

---

### 2. Create a new post

Creates a new post for the specified project. The author is taken from the authenticated user.

* **Method:** `POST`
* **URL:** `/api/v1/projects/{projectId}/posts`
* **Auth:** required

**Path parameters**

* `projectId` (`Guid`) – id of the project.

**Request body**

```json
{
  "title": "Short title of the update",
  "content": "Longer description of what changed, what was shipped, etc."
}
```

Validation rules (enforced in domain or service):

* `title` – required, non-empty, reasonable max length.
* `content` – required, non-empty.

**Responses**

* `201 Created` – post created successfully.

Response body: `PostResponse` for the created post.

* `400 Bad Request` – invalid input (for example missing or empty `title`/`content`).
* `404 Not Found` – project does not exist or is not visible to the user.

---

### 3. Get a single post

Returns a single post by id within the context of a project.

* **Method:** `GET`
* **URL:** `/api/v1/projects/{projectId}/posts/{postId}`
* **Auth:** required

**Path parameters**

* `projectId` (`Guid`) – id of the project.
* `postId` (`Guid`) – id of the post.

**Responses**

* `200 OK` – returns `PostResponse`.
* `404 Not Found` – if the post does not exist, does not belong to the given project, or is not visible to the current user.

---

### 4. Update a post

Updates the title and content of an existing post.

* **Method:** `PUT`
* **URL:** `/api/v1/projects/{projectId}/posts/{postId}`
* **Auth:** required

Only the author (and in the future possibly project owners/admins) can update a post.

**Path parameters**

* `projectId` (`Guid`)
* `postId` (`Guid`)

**Request body**

Same shape as create:

```json
{
  "title": "Updated title",
  "content": "Updated content of the post"
}
```

**Responses**

* `200 OK` (or `204 No Content`, depending on implementation) – post updated.

    * If `200`, response body should be the updated `PostResponse`.
* `400 Bad Request` – invalid input.
* `403 Forbidden` – current user is not allowed to edit this post.
* `404 Not Found` – post not found or does not belong to the given project.

---

### 5. Delete a post

Deletes a post permanently.

* **Method:** `DELETE`
* **URL:** `/api/v1/projects/{projectId}/posts/{postId}`
* **Auth:** required

Only the author (and later possibly project admins) can delete a post.

**Path parameters**

* `projectId` (`Guid`)
* `postId` (`Guid`)

**Responses**

* `204 No Content` – post deleted successfully.
* `403 Forbidden` – user is not allowed to delete this post.
* `404 Not Found` – post not found or does not belong to the given project.

## Error format

The Posts API uses the same error format as the rest of Sparkly (for example a problem-details style object).

Typical shape:

```json
{
  "type": "validation_error",
  "title": "Validation failed",
  "status": 400,
  "errors": {
    "title": ["Title is required."],
    "content": ["Content is required."]
  }
}
```

Keep any future validation or authorization errors for posts consistent with this format so the frontend can handle them uniformly.

## Notes for maintainers

* When adding new fields to `Post`, update:

    * the EF Core mapping and migrations,
    * the `PostResponse` DTO and mapper,
    * this document and any frontend models.
* When changing routes or authorization rules, keep the behaviour described above in sync with the actual implementation.
* Consider adding Swagger / OpenAPI annotations to `PostsController` so these endpoints also appear in generated API documentation.
