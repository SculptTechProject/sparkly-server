namespace sparkly_server.DTO.Posts
{
    public class PostResponse
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public Guid AuthorId { get; set; }
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}
