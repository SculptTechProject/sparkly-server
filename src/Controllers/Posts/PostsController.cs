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

        public PostsController(IPostService posts) => _posts = posts;
        
        /// <summary>
        /// Retrieves the unique identifier (GUID) of the currently authenticated user.
        /// </summary>
        /// <returns>
        /// A <see cref="Guid"/> representing the user's unique identifier, or <see cref="Guid.Empty"/> if the user is not authenticated.
        /// </returns>
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
        /// Retrieves a paginated feed of posts for the current user.
        /// </summary>
        /// <param name="page">The page number of the feed to retrieve. Defaults to 1.</param>
        /// <param name="pageSize">The number of posts to include per page. Defaults to 20.</param>
        /// <param name="ct">A cancellation token to observe while awaiting the task.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing a list of <see cref="PostResponse"/> objects representing the user's feed.
        /// </returns>
        [HttpGet("feed")]
        public async Task<ActionResult<IReadOnlyList<PostResponse>>> GetFeed(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var userId = GetUserId();
            Console.WriteLine($"FEED userId = {userId}");

            var posts = await _posts.GetFeedPostAsync(userId, page, pageSize, ct);

            var response = posts
                .Select(p => p.ToResponse(userId))
                .ToList();

            return Ok(response);
        }


        /// <summary>
        /// Creates a new post associated with a specific project.
        /// </summary>
        /// <param name="request">The details of the post to be created, including title and content.</param>
        /// <param name="projectId">The unique identifier (GUID) of the project the post belongs to.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the created post response including its properties.
        /// </returns>
        [HttpPost("create/project/{projectId:guid}")]
        public async Task<ActionResult<PostResponse>> Create([FromBody] CreatePostRequest request, [FromRoute] Guid projectId)
        {
            Guid userId = GetUserId();

            var post = await _posts.AddProjectPostAsync(
                userId,
                projectId,
                request.Title,
                request.Content);

            var response = post.ToResponse(userId);

            return Created(string.Empty, response);
        }

        /// <summary>
        /// Creates a new post for the feed with the specified content and title.
        /// </summary>
        /// <param name="request">The details of the post to create, including title and content.</param>
        /// <param name="ct">The cancellation token to monitor for request cancellation.</param>
        /// <returns>
        /// An <see cref="ActionResult{T}"/> containing the created feed post details.
        /// </returns>
        [HttpPost("create/feed")]
        public async Task<ActionResult<PostResponse>> CreateFeedPost(
            [FromBody] CreatePostRequest request,
            CancellationToken ct)
        {
            var userId = GetUserId();

            var post = await _posts.AddFeedPostAsync(
                userId,
                request.Title,
                request.Content,
                ct);

            var response = post.ToResponse(userId);

            return Created(string.Empty, response);
        }
        
        /// <summary>
        /// Updates an existing post with new information provided by the user.
        /// </summary>
        /// <param name="postId">The unique identifier (GUID) of the post to be updated.</param>
        /// <param name="request">An <see cref="UpdatePostRequest"/> containing the new title and content for the post.</param>
        /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the update operation.
        /// Returns a NotFound result if the post does not exist, or an Ok result containing the updated post.
        /// </returns>
        [HttpPut("{postId:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid postId, [FromBody] UpdatePostRequest request,
                                                CancellationToken ct = default)
        {
            Guid userId = GetUserId();
            
            var updatedPost = await _posts.UpdatePostAsync(
                postId,
                userId,
                request.Title,
                request.Content,
                ct);
            
            if (updatedPost is null)
            {
                return NotFound();
            }
            
            return Ok(updatedPost);
        }

        /// <summary>
        /// Deletes a post specified by its unique identifier.
        /// </summary>
        /// <param name="postId">The unique identifier (GUID) of the post to delete.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> indicating the result of the delete operation.
        /// Returns a NoContent response if the deletion is successful.
        /// </returns>
        [HttpDelete("{postId:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid postId)
        {
            Guid userId = GetUserId();
            
            await _posts.DeletePostAsync(postId, userId);
            
            return NoContent();
        }
    }
}
