using sparkly_server.Domain.Users;
using sparkly_server.Enum;

namespace sparkly_server.Domain.Projects
{
    public class Project
    {
        public Guid Id { get; private set; }
        public string ProjectName { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public string Slug { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public Guid OwnerId { get; private set; }
        private readonly List<string> _tags = new();
        public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();
        public DateTime UpdatedAt { get; private set; }
        public ProjectVisibility Visibility { get; private set; }
        private readonly List<User> _members = new();
        public IReadOnlyCollection<User> Members => _members.AsReadOnly();
        
        private Project() { }

        public Project(
            Guid ownerId,
            string projectName,
            string description,
            ProjectVisibility visibility = ProjectVisibility.Private)
        {
            Id = Guid.NewGuid();
            OwnerId = ownerId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = CreatedAt;

            SetNameInternal(projectName);
            SetDescriptionInternal(description);
            SetVisibilityInternal(visibility);
            
            Slug = NormalizeSlug(projectName); 
        }
        
        public static Project Create(
            User owner,
            string projectName,
            string description,
            ProjectVisibility visibility = ProjectVisibility.Private)
        {
            if (owner is null)
                throw new ArgumentNullException(nameof(owner));

            var project = new Project(owner.Id, projectName, description, visibility);

            project.AddMember(owner); // owner always member!!!

            return project;
        }
        
        private void Touch()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        
        /// INTERNAL SETTERS (without Touch) ///
        
        private void SetNameInternal(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("ProjectName name cannot be empty.", nameof(name));

            ProjectName = name.Trim();
        }

        private void SetDescriptionInternal(string description)
        {
            Description = description?.Trim() ?? string.Empty;
        }

        private void SetVisibilityInternal(ProjectVisibility visibility)
        {
            Visibility = visibility;
        }

        private static string NormalizeSlug(string value)
        {
            return value
                .Trim()
                .ToLowerInvariant()
                .Replace(" ", "-");
        }

        
        /// PUBLIC SETTERS (with Touch)  ///
        
         public void Rename(string newName)
        {
            SetNameInternal(newName);
            // Jeśli chcesz, aby slug szedł za nazwą:
            Slug = NormalizeSlug(newName);
            Touch();
        }

        public void ChangeDescription(string newDescription)
        {
            SetDescriptionInternal(newDescription);
            Touch();
        }

        public void SetSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("Slug cannot be empty.", nameof(slug));

            Slug = NormalizeSlug(slug);
            Touch();
        }

        public bool HasMember(Guid userId) =>
            _members.Any(m => m.Id == userId);

        public bool IsOwner(Guid userId) => OwnerId == userId;

        public void SetVisibility(ProjectVisibility visibility)
        {
            SetVisibilityInternal(visibility);
            Touch();
        }

        public void MakePublic()
        {
            SetVisibilityInternal(ProjectVisibility.Public);
            Touch();
        }

        public void MakePrivate()
        {
            SetVisibilityInternal(ProjectVisibility.Private);
            Touch();
        }

        public void AddMember(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (_members.Any(m => m.Id == user.Id))
                return;

            _members.Add(user);
            Touch();
        }

        public void RemoveMember(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            if (!_members.Any(m => m.Id == user.Id))
                return;

            _members.Remove(user);
            Touch();
        }

        public void AddTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            tag = tag.Trim();

            if (_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                return;

            _tags.Add(tag);
            Touch();
        }

        public void RemoveTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return;

            var existing = _tags
                .FirstOrDefault(t => string.Equals(t, tag, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
                return;

            _tags.Remove(existing);
            Touch();
        }
    }
}
