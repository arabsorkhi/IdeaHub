

using Application.DTOs;
using Domain.Entity;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace IdeaHub.Controllers
{
    namespace IdeaVault.API.Controllers
    {
        [ApiController]
        [Route("api/[controller]")]
        [Authorize]
        public class IdeaController : ControllerBase
        {
            private readonly IIdeaService _ideaService;

            public IdeaController(IIdeaService ideaService)
            {
                _ideaService = ideaService;
            }

            private Guid GetCurrentUserId()
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    throw new UnauthorizedAccessException("کاربر احراز هویت نشده است.");

                return Guid.Parse(userIdClaim);
            }

            [HttpPost]
            public async Task<IActionResult> CreateIdea([FromBody] CreateIdeaDto createDto)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.CreateIdeaAsync(userId, createDto);
                    return CreatedAtAction(nameof(GetIdea), new { id = result.Id }, result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpGet("{id}")]
            public async Task<IActionResult> GetIdea(Guid id)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.GetIdeaByIdAsync(userId, id);
                    return Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpGet]
            public async Task<IActionResult> GetIdeas(
                [FromQuery] string? status = null,
                [FromQuery] string? visibility = null,
                [FromQuery] string? search = null,
                [FromQuery] int page = 1,
                [FromQuery] int pageSize = 10)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.GetIdeasAsync(userId, status, visibility, search, page, pageSize);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            //پیاده‌سازی Search API با EF Core (با Index روی عنوان‌های رمزنگاری نشده)
            //فیلتر بر اساس: تاریخ، وضعیت، تکنولوژی، بودجه
            //[HttpGet]
            //public async Task<IActionResult> GetIdeas([FromQuery] IdeaFilter filter)
            //{
            //    var query = _context.Ideas
            //        .Where(i => i.Status == IdeaStatus.Public)
            //        .Include(i => i.User)
            //        .AsQueryable();

            //    // فیلتر بر اساس تکنولوژی (ذخیره شده به صورت plain text)
            //    if (!string.IsNullOrEmpty(filter.Technology))
            //        query = query.Where(i => i.Technologies.Contains(filter.Technology));

            //    var ideas = await query
            //        .Skip((filter.Page - 1) * filter.PageSize)
            //        .Take(filter.PageSize)
            //        .Select(i => new IdeaListDto
            //        {
            //            Id = i.Id,
            //            Title = _encryption.Decrypt(i.EncryptedTitle),
            //            Summary = _encryption.Decrypt(i.EncryptedSummary),
            //            // بقیه فیلدها رو نمی‌دیم
            //        })
            //        .ToListAsync();

            //    return Ok(ideas);
            //}


            [HttpPut("{id}")]
            public async Task<IActionResult> UpdateIdea(Guid id, [FromBody] UpdateIdeaDto updateDto)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.UpdateIdeaAsync(userId, id, updateDto);
                    return Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> DeleteIdea(Guid id)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.DeleteIdeaAsync(userId, id);
                    return Ok(new { success = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpPost("{id}/publish")]
            public async Task<IActionResult> PublishIdea(Guid id)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.PublishIdeaAsync(userId, id);
                    return Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpPost("{id}/archive")]
            public async Task<IActionResult> ArchiveIdea(Guid id)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.ArchiveIdeaAsync(userId, id);
                    return Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpPost("{id}/reviews")]
            public async Task<IActionResult> AddReview(Guid id, [FromBody] IdeaReviewDto reviewDto)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.AddReviewAsync(userId, id, reviewDto);
                    return Ok(result);
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpGet("{id}/reviews")]
            public async Task<IActionResult> GetReviews(Guid id)
            {
                try
                {
                    var result = await _ideaService.GetIdeaReviewsAsync(id);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpDelete("reviews/{reviewId}")]
            public async Task<IActionResult> DeleteReview(Guid reviewId)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.DeleteReviewAsync(userId, reviewId);
                    return Ok(new { success = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpPost("{id}/collaborators")]
            public async Task<IActionResult> AddCollaborator(Guid id, [FromBody] IdeaCollaboratorDto collaboratorDto)
            {
                try
                {
                    var userId = GetCurrentUserId();
                    var result = await _ideaService.AddCollaboratorAsync(userId, id, collaboratorDto);
                    return Ok(new { success = result });
                }
                catch (KeyNotFoundException ex)
                {
                    return NotFound(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException ex)
                {
                    return Unauthorized(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }

            [HttpGet("{id}/collaborators")]
            public async Task<IActionResult> GetCollaborators(Guid id)
            {
                try
                {
                    var result = await _ideaService.GetCollaboratorsAsync(id);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "خطای داخلی سرور", error = ex.Message });
                }
            }
        }
    }
}
