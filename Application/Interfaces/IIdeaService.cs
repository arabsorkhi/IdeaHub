using Application.DTOs;

namespace Domain.Interfaces
{
   
 
        public interface IIdeaService
        {
            Task<IdeaResponseDto> CreateIdeaAsync(Guid userId, CreateIdeaDto createDto);
            Task<IdeaResponseDto> UpdateIdeaAsync(Guid userId, Guid ideaId, UpdateIdeaDto updateDto);
            Task<bool> DeleteIdeaAsync(Guid userId, Guid ideaId);
            Task<IdeaResponseDto> GetIdeaByIdAsync(Guid userId, Guid ideaId);
            Task<IdeaListResponseDto> GetIdeasAsync(
                Guid? userId,
                string? status = null,
                string? visibility = null,
                string? search = null,
                int page = 1,
                int pageSize = 10);
            Task<IdeaResponseDto> PublishIdeaAsync(Guid userId, Guid ideaId);
            Task<IdeaResponseDto> ArchiveIdeaAsync(Guid userId, Guid ideaId);

            // Reviews
            Task<IdeaReviewResponseDto> AddReviewAsync(Guid userId, Guid ideaId, IdeaReviewDto reviewDto);
            Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId);
            Task<List<IdeaReviewResponseDto>> GetIdeaReviewsAsync(Guid ideaId);

            // Collaborators
            Task<bool> AddCollaboratorAsync(Guid userId, Guid ideaId, IdeaCollaboratorDto collaboratorDto);
            Task<bool> RemoveCollaboratorAsync(Guid userId, Guid ideaId, Guid collaboratorUserId);
            Task<List<UserDto>> GetCollaboratorsAsync(Guid ideaId);
        }
    }
 
