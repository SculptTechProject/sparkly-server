using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sparkly_server.Domain.Posts;
using sparkly_server.DTO.Posts;
using sparkly_server.DTO.Posts.Mapper;
using sparkly_server.Services.Posts.service;
using System.Security.Claims;

namespace sparkly_server.Controllers.Posts
{
    [ApiController]
    [Route("api/v1/posts")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _posts;

        private PostsController(IPostService posts) => _posts = posts;
        
        // helper to get the current user's id
        private Guid GetUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(idValue is null)
            {
                return Guid.Empty;
            }
            return Guid.Parse(idValue);
        }
        
        // Controllers

        /// <summary>
        /// Retrieves a single post by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the post to retrieve.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the requested post if found, or a NotFound result if the post does not exist.
        /// </returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Post>> GetPostById(Guid id)
        {
            var post = await _posts.GetPostByIdAsync(id);
            
            if (post is null)
            {
                return NotFound();
            }
            
            return Ok(post);
        }

        /// <summary>
        /// Retrieves a list of posts associated with a specific project.
        /// </summary>
        /// <param name="projectId">The unique identifier (GUID) of the project for which posts should be retrieved.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a read-only list of posts associated with the specified project.
        /// </returns>
        [HttpGet("project/{projectId:guid}")]
        public async Task<ActionResult<IReadOnlyList<Post>>> GetProjectPosts(
            Guid projectId,
            [FromQuery] CancellationToken ct)
        {
            var posts = await _posts.GetProjectPostsAsync(projectId, ct);
            return Ok(posts);
        }

        /// <summary>
        /// Retrieves a paginated list of posts for the authenticated user's feed.
        /// </summary>
        /// <param name="page">The page number to retrieve, starting from 1.</param>
        /// <param name="pageSize">The number of posts per page.</param>
        /// <param name="ct">A cancellation token for the operation.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a read-only list of posts in the user's feed.
        /// </returns>
        [HttpGet("feed")]
        public async Task<ActionResult<IReadOnlyList<Post>>> GetFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] CancellationToken ct = default)
        {
            var userId = GetUserId();
            var posts = await _posts.GetFeedPostAsync(userId, page, pageSize, ct);
            return Ok(posts);
        }

        /// <summary>
        /// Creates a new post associated with a specific project and authored by the current user.
        /// </summary>
        /// <param name="request">The details of the post to be created, including project ID, title, and content.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the created post's response details,
        /// or a BadRequest result if the creation fails.
        /// </returns>
        [HttpPost]
        public async Task<ActionResult<PostResponse>> Create([FromBody] CreatePostRequest request)
        {
            Guid userId = GetUserId();

            var post = await _posts.AddPostAsync(
                request.ProjectId,
                userId,
                request.Title,
                request.Content);

            var response = post.ToResponse();

            return Created(string.Empty, response);
        }
    }
}
