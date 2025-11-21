# Sparkly Server – Projects Module

This is the official-but-not-too-serious guide to how Projects work inside Sparkly Server. If Users are the people at the party, Projects are the things they brag about. The Projects module keeps track of them, validates them, and makes sure two people do not name their projects "My Cool App" at the same time.

## What a Project Is

In Sparkly, a Project represents something a user is building or showcasing. The domain model is clean and focused:

• Id – primary key.

• ProjectName – required, unique across all users.

• Description – optional but encouraged.

• Visibility – controls who can see it (public or private).

• OwnerId – reference to the user who created it.

• Owner – navigation property.

• CreatedAt / UpdatedAt – timestamps.

>The entity stays focused on representing state, not performing logic.

## EF Core Mapping

Project mapping in `AppDbContext` ensures:

• Projects live in the `projects` table.

• ProjectName is required and has a max length.

• A unique index on ProjectName.

• A relationship to the User who owns the project.

>This prevents naming collisions and keeps database constraints honest.

## Project Repository

The repository handles all data access for projects. It shields the service layer from EF Core details.

Typical repository operations include (more will be added later):

• Creating a project.

• Fetching by Id.

• Checking if a project name is already taken.

• Fetching all projects for a given user.

• Deleting a project.

>By keeping EF logic here, the rest of the codebase stays clean.

## ProjectService – The Rules and Workflow

The service orchestrates the entire lifecycle of Projects. It contains rules such as:

• Only authenticated users can create projects.
• Project names must be unique.
• The owner must exist.
• Deleting a project should respect authorization.

A typical create workflow looks like:

1. Resolve current user using `ICurrentUser`.
2. Fetch the user from the database.
3. Check if the name is free using `IsProjectNameTakenAsync`.
4. Create a new Project object.
5. Save it through the repository.

When deleting:

• The service confirms ownership.
• Uses EF Core `ExecuteDeleteAsync` for efficient removal.

## Visibility

The visibility field controls who can see the project:

• Public – visible to everyone.

• Private – visible only to the owner and collaborators (coming soon).

>Great for early work-in-progress projects or things that should not be judged yet.

## DTOs for Projects

DTOs ensure only safe and selected data leaves the server:

• CreateProjectDto – incoming data when creating.

• ProjectDto – returned when fetching.

• Delete or Update DTOs, if needed.

>They make API responses predictable and consistent.

## Controllers – The HTTP Face of Projects

Endpoints related to projects include:

• POST /projects – create a new project.

• GET /projects/{id} – fetch project by id.

• GET /projects – fetch user’s projects.

• DELETE /projects/{id} – remove a project.

>Controllers stay thin. They accept input, send it to the service, and return the result.

## Authorization

All project routes require authentication. Creating or deleting requires you to be the owner. The system uses `ICurrentUser` injected via DI to know who is making the request.

Typical rule:

If you made it, you can delete it. If you did not, hands off.

## Data Validation

Project creation validates:

• Name not empty.

• Name length within limits.

• Name not already taken.

This avoids duplicates and cleans up common user mistakes.

## Clean Architecture Layout

The module respects the structure:

Domain → Repository → Service → Controller.

>Nothing leaks across layers. No EF in controllers. No HTTP in domain. No shortcuts.

## Summary

The Projects module ensures that Sparkly has a clean, organized list of user creations. It enforces uniqueness, ownership, and visibility rules without slowing the developer down.

You can extend this module later with features such as:

• Tags.

• Slugs.

• Project collaborators.

• Project statistics.

• Version history.

All the foundation is ready. Sparkly is flexible enough to grow as fast as your ideas do.
