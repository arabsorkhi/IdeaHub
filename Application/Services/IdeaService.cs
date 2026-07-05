using Application.DTOs;
using Application.Interfaces;
using Domain.Entity;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace Application.Services
{
    public class IdeaService : IIdeaService
    {
        private readonly IRepository<Idea> _ideaRepository;
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<IdeaReview> _reviewRepository;
        private readonly IRepository<IdeaCollaborator> _collaboratorRepository;
        private readonly IRepository<IdeaTag> _tagRepository;
        private readonly IEncryptionService _encryptionService;

        public IdeaService(
            IRepository<Idea> ideaRepository,
            IRepository<User> userRepository,
            IRepository<IdeaReview> reviewRepository,
            IRepository<IdeaCollaborator> collaboratorRepository,
            IRepository<IdeaTag> tagRepository,
            IEncryptionService encryptionService)
        {
            _ideaRepository = ideaRepository;
            _userRepository = userRepository;
            _reviewRepository = reviewRepository;
            _collaboratorRepository = collaboratorRepository;
            _tagRepository = tagRepository;
            _encryptionService = encryptionService;
        }

        public async Task<IdeaResponseDto> CreateIdeaAsync(Guid userId, CreateIdeaDto createDto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("کاربر یافت نشد.");

            var idea = new Idea
            {
                Id = Guid.NewGuid(),
                Title = createDto.Title,
                Description = createDto.Description,
                ProblemStatement = createDto.ProblemStatement,
                SolutionDescription = createDto.SolutionDescription,
                TargetAudience = createDto.TargetAudience,
                UniqueValue = createDto.UniqueValue,
                EstimatedBudget = createDto.EstimatedBudget,
                EstimatedTimelineMonths = createDto.EstimatedTimelineMonths,
                OwnerId = userId,
                Status = IdeaStatus.Draft,
                Visibility = Enum.TryParse<VisibilityLevel>(createDto.Visibility, true, out var vis)
                    ? vis : VisibilityLevel.Private,
                CreatedAt = DateTime.UtcNow
            };

            // رمزگذاری اطلاعات محرمانه
            if (!string.IsNullOrEmpty(createDto.EncryptedDetails))
            {
                var encrypted = await _encryptionService.EncryptAsync(createDto.EncryptedDetails);
                idea.EncryptedDetails = encrypted;
            }

            await _ideaRepository.AddAsync(idea);

            // افزودن برچسب‌ها
            if (createDto.Tags != null)
            {
                foreach (var tagName in createDto.Tags)
                {
                    var tag = new IdeaTag
                    {
                        Id = Guid.NewGuid(),
                        Name = tagName.Trim(),
                        IdeaId = idea.Id
                    };
                    await _tagRepository.AddAsync(tag);
                }
            }

            await _ideaRepository.SaveChangesAsync();

            return await MapToResponseDto(idea, userId);
        }

        public async Task<IdeaResponseDto> UpdateIdeaAsync(Guid userId, Guid ideaId, UpdateIdeaDto updateDto)
        {
            var idea = await GetIdeaWithDetailsAsync(ideaId);

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("شما اجازه ویرایش این ایده را ندارید.");

            if (idea.Status == IdeaStatus.Archived)
                throw new InvalidOperationException("امکان ویرایش ایده بایگانی شده وجود ندارد.");

            // به‌روزرسانی فیلدها
            if (!string.IsNullOrEmpty(updateDto.Title))
                idea.Title = updateDto.Title;

            if (!string.IsNullOrEmpty(updateDto.Description))
                idea.Description = updateDto.Description;

            if (!string.IsNullOrEmpty(updateDto.ProblemStatement))
                idea.ProblemStatement = updateDto.ProblemStatement;

            if (!string.IsNullOrEmpty(updateDto.SolutionDescription))
                idea.SolutionDescription = updateDto.SolutionDescription;

            if (!string.IsNullOrEmpty(updateDto.TargetAudience))
                idea.TargetAudience = updateDto.TargetAudience;

            if (!string.IsNullOrEmpty(updateDto.UniqueValue))
                idea.UniqueValue = updateDto.UniqueValue;

            if (updateDto.EstimatedBudget.HasValue)
                idea.EstimatedBudget = updateDto.EstimatedBudget;

            if (updateDto.EstimatedTimelineMonths.HasValue)
                idea.EstimatedTimelineMonths = updateDto.EstimatedTimelineMonths;

            if (!string.IsNullOrEmpty(updateDto.Visibility))
                idea.Visibility = Enum.TryParse<VisibilityLevel>(updateDto.Visibility, true, out var vis)
                    ? vis : idea.Visibility;

            // به‌روزرسانی رمزگذاری
            if (!string.IsNullOrEmpty(updateDto.EncryptedDetails))
            {
                var encrypted = await _encryptionService.EncryptAsync(updateDto.EncryptedDetails);
                idea.EncryptedDetails = encrypted;
            }

            // به‌روزرسانی برچسب‌ها
            if (updateDto.Tags != null)
            {
                // حذف برچسب‌های قبلی
                var oldTags = await _tagRepository.Query()
                    .Where(t => t.IdeaId == ideaId)
                    .ToListAsync();

                foreach (var tag in oldTags)
                    await _tagRepository.DeleteAsync(tag);

                // افزودن برچسب‌های جدید
                foreach (var tagName in updateDto.Tags)
                {
                    var tag = new IdeaTag
                    {
                        Id = Guid.NewGuid(),
                        Name = tagName.Trim(),
                        IdeaId = idea.Id
                    };
                    await _tagRepository.AddAsync(tag);
                }
            }

            idea.UpdatedAt = DateTime.UtcNow;
            await _ideaRepository.SaveChangesAsync();

            return await MapToResponseDto(idea, userId);
        }

        public async Task<bool> DeleteIdeaAsync(Guid userId, Guid ideaId)
        {
            var idea = await _ideaRepository.GetByIdAsync(ideaId);
            if (idea == null)
                throw new KeyNotFoundException("ایده یافت نشد.");

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("شما اجازه حذف این ایده را ندارید.");

            await _ideaRepository.DeleteAsync(idea);
            await _ideaRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IdeaResponseDto> GetIdeaByIdAsync(Guid userId, Guid ideaId)
        {
            var idea = await GetIdeaWithDetailsAsync(ideaId);

            // بررسی دسترسی
            if (!CanViewIdea(idea, userId))
                throw new UnauthorizedAccessException("شما دسترسی به این ایده را ندارید.");

            // افزایش بازدید
            idea.ViewCount++;
            await _ideaRepository.SaveChangesAsync();

            return await MapToResponseDto(idea, userId);
        }

        public async Task<IdeaListResponseDto> GetIdeasAsync(
            Guid? userId,
            string? status = null,
            string? visibility = null,
            string? search = null,
            int page = 1,
            int pageSize = 10)
        {
            var query = _ideaRepository.Query()
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .Where(i => i.Status != IdeaStatus.Draft || (userId.HasValue && i.OwnerId == userId));

            // فیلتر بر اساس وضعیت
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<IdeaStatus>(status, true, out var statusEnum))
                query = query.Where(i => i.Status == statusEnum);

            // فیلتر بر اساس دید
            if (!string.IsNullOrEmpty(visibility) && Enum.TryParse<VisibilityLevel>(visibility, true, out var visEnum))
                query = query.Where(i => i.Visibility == visEnum);
            else if (!userId.HasValue)
                query = query.Where(i => i.Visibility != VisibilityLevel.Private);

            // جستجو
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.Title.Contains(search) ||
                    i.Description.Contains(search) ||
                    i.TargetAudience.Contains(search) ||
                    i.Tags.Any(t => t.Name.Contains(search)));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var resultItems = new List<IdeaResponseDto>();
            foreach (var idea in items)
            {
                try
                {
                    var dto = await MapToResponseDto(idea, userId);
                    resultItems.Add(dto);
                }
                catch
                {
                    // اگر کاربر دسترسی نداشت، رد می‌شود
                }
            }

            return new IdeaListResponseDto
            {
                Items = resultItems,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                HasNextPage = page * pageSize < totalCount,
                HasPreviousPage = page > 1
            };
        }

        public async Task<IdeaResponseDto> PublishIdeaAsync(Guid userId, Guid ideaId)
        {
            var idea = await GetIdeaWithDetailsAsync(ideaId);

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("شما اجازه انتشار این ایده را ندارید.");

            if (idea.Status != IdeaStatus.Draft)
                throw new InvalidOperationException("فقط ایده‌های پیش‌نویس قابل انتشار هستند.");

            idea.Status = IdeaStatus.PendingReview;
            idea.PublishedAt = DateTime.UtcNow;
            idea.UpdatedAt = DateTime.UtcNow;

            await _ideaRepository.SaveChangesAsync();

            return await MapToResponseDto(idea, userId);
        }

        public async Task<IdeaResponseDto> ArchiveIdeaAsync(Guid userId, Guid ideaId)
        {
            var idea = await GetIdeaWithDetailsAsync(ideaId);

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("شما اجازه بایگانی این ایده را ندارید.");

            idea.Status = IdeaStatus.Archived;
            idea.UpdatedAt = DateTime.UtcNow;

            await _ideaRepository.SaveChangesAsync();

            return await MapToResponseDto(idea, userId);
        }

        // Reviews
        public async Task<IdeaReviewResponseDto> AddReviewAsync(Guid userId, Guid ideaId, IdeaReviewDto reviewDto)
        {
            var idea = await _ideaRepository.GetByIdAsync(ideaId);
            if (idea == null)
                throw new KeyNotFoundException("ایده یافت نشد.");

            // بررسی دسترسی: فقط کاربران تأیید شده می‌توانند نظر دهند
            if (idea.Visibility == VisibilityLevel.Private && idea.OwnerId != userId)
                throw new UnauthorizedAccessException("شما دسترسی به این ایده را ندارید.");

            var review = new IdeaReview
            {
                Id = Guid.NewGuid(),
                IdeaId = ideaId,
                ReviewerId = userId,
                Comment = reviewDto.Comment,
                Rating = reviewDto.Rating,
                Type = Enum.TryParse<ReviewType>(reviewDto.Type, true, out var type) ? type : ReviewType.General,
                CreatedAt = DateTime.UtcNow
            };

            await _reviewRepository.AddAsync(review);
            idea.ReviewCount++;
            await _reviewRepository.SaveChangesAsync();
            await _ideaRepository.SaveChangesAsync();

            return await MapToReviewResponseDto(review);
        }

        public async Task<bool> DeleteReviewAsync(Guid userId, Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review == null)
                throw new KeyNotFoundException("نظر یافت نشد.");

            if (review.ReviewerId != userId)
                throw new UnauthorizedAccessException("شما اجازه حذف این نظر را ندارید.");

            await _reviewRepository.DeleteAsync(review);
            await _reviewRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<IdeaReviewResponseDto>> GetIdeaReviewsAsync(Guid ideaId)
        {
            var reviews = await _reviewRepository.Query()
                .Include(r => r.Reviewer)
                .Where(r => r.IdeaId == ideaId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = new List<IdeaReviewResponseDto>();
            foreach (var review in reviews)
            {
                result.Add(await MapToReviewResponseDto(review));
            }
            return result;
        }

        // Collaborators
        public async Task<bool> AddCollaboratorAsync(Guid userId, Guid ideaId, IdeaCollaboratorDto collaboratorDto)
        {
            var idea = await _ideaRepository.GetByIdAsync(ideaId);
            if (idea == null)
                throw new KeyNotFoundException("ایده یافت نشد.");

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("فقط مالک ایده می‌تواند همکار اضافه کند.");

            // بررسی وجود همکار
            var existing = await _collaboratorRepository.Query()
                .FirstOrDefaultAsync(c => c.IdeaId == ideaId && c.UserId == collaboratorDto.UserId);

            if (existing != null)
                throw new InvalidOperationException("این کاربر قبلاً به عنوان همکار اضافه شده است.");

            var collaborator = new IdeaCollaborator
            {
                Id = Guid.NewGuid(),
                IdeaId = ideaId,
                UserId = collaboratorDto.UserId,
                Role = Enum.TryParse<CollaboratorRole>(collaboratorDto.Role, true, out var role)
                    ? role : CollaboratorRole.Developer,
                Status = CollaboratorStatus.Pending,
                JoinedAt = DateTime.UtcNow
            };

            await _collaboratorRepository.AddAsync(collaborator);
            await _collaboratorRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveCollaboratorAsync(Guid userId, Guid ideaId, Guid collaboratorUserId)
        {
            var idea = await _ideaRepository.GetByIdAsync(ideaId);
            if (idea == null)
                throw new KeyNotFoundException("ایده یافت نشد.");

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("فقط مالک ایده می‌تواند همکار را حذف کند.");

            var collaborator = await _collaboratorRepository.Query()
                .FirstOrDefaultAsync(c => c.IdeaId == ideaId && c.UserId == collaboratorUserId);

            if (collaborator == null)
                throw new KeyNotFoundException("همکار یافت نشد.");

            await _collaboratorRepository.DeleteAsync(collaborator);
            await _collaboratorRepository.SaveChangesAsync();
            return true;
        }

        public async Task<List<UserDto>> GetCollaboratorsAsync(Guid ideaId)
        {
            var collaborators = await _collaboratorRepository.Query()
                .Include(c => c.User)
                .Where(c => c.IdeaId == ideaId && c.Status == CollaboratorStatus.Active)
                .Select(c => c.User)
                .ToListAsync();

            return collaborators.Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Bio = u.Bio,
                Role = u.Role.ToString()
            }).ToList();
        }

        #region Private Methods

        private async Task<Idea> GetIdeaWithDetailsAsync(Guid ideaId)
        {
            var idea = await _ideaRepository.Query()
                .Include(i => i.Owner)
                .Include(i => i.Tags)
                .FirstOrDefaultAsync(i => i.Id == ideaId);

            if (idea == null)
                throw new KeyNotFoundException("ایده یافت نشد.");

            return idea;
        }

        private bool CanViewIdea(Idea idea, Guid? userId)
        {
            if (idea.Visibility == VisibilityLevel.Public)
                return true;

            if (userId.HasValue && idea.OwnerId == userId.Value)
                return true;

            // اگر Confidential باشد، فقط افراد دعوت شده می‌توانند ببینند
            if (idea.Visibility == VisibilityLevel.Confidential)
            {
                // بررسی اینکه کاربر در لیست همکاران است
                // اینجا باید چک شود
            }

            return false;
        }

        private async Task<IdeaResponseDto> MapToResponseDto(Idea idea, Guid? userId)
        {
            var isOwner = userId.HasValue && idea.OwnerId == userId.Value;

            // اگر کاربر دسترسی ندارد، اطلاعات محدود برگردانده شود
            if (!CanViewIdea(idea, userId))
            {
                return new IdeaResponseDto
                {
                    Id = idea.Id,
                    Title = idea.Title,
                    Description = "این ایده محرمانه است",
                    Status = idea.Status.ToString(),
                    Visibility = idea.Visibility.ToString(),
                    Owner = new UserDto
                    {
                        Id = idea.Owner.Id,
                        FullName = idea.Owner.FullName
                    },
                    CreatedAt = idea.CreatedAt,
                    IsOwner = false,
                    CanEdit = false
                };
            }

            var tags = idea.Tags.Select(t => t.Name).ToList();

            return new IdeaResponseDto
            {
                Id = idea.Id,
                Title = idea.Title,
                Description = idea.Description,
                ProblemStatement = idea.ProblemStatement,
                SolutionDescription = idea.SolutionDescription,
                TargetAudience = idea.TargetAudience,
                UniqueValue = idea.UniqueValue,
                EstimatedBudget = idea.EstimatedBudget,
                EstimatedTimelineMonths = idea.EstimatedTimelineMonths,
                Status = idea.Status.ToString(),
                Visibility = idea.Visibility.ToString(),
                ViewCount = idea.ViewCount,
                LikeCount = idea.LikeCount,
                ReviewCount = idea.ReviewCount,
                Owner = new UserDto
                {
                    Id = idea.Owner.Id,
                    FullName = idea.Owner.FullName,
                    Email = idea.Owner.Email,
                    Bio = idea.Owner.Bio
                },
                CreatedAt = idea.CreatedAt,
                PublishedAt = idea.PublishedAt,
                Tags = tags,
                IsOwner = isOwner,
                CanEdit = isOwner && idea.Status != IdeaStatus.Archived
            };
        }

        private async Task<IdeaReviewResponseDto> MapToReviewResponseDto(IdeaReview review)
        {
            return new IdeaReviewResponseDto
            {
                Id = review.Id,
                Comment = review.Comment,
                Rating = review.Rating,
                Type = review.Type.ToString(),
                Reviewer = new UserDto
                {
                    Id = review.Reviewer.Id,
                    FullName = review.Reviewer.FullName,
                    Email = review.Reviewer.Email,
                    Bio = review.Reviewer.Bio
                },
                CreatedAt = review.CreatedAt
            };
        }


        #endregion


        // اضافه کردن این متدها به کلاس IdeaService

        //public async Task<IdeaCollaboratorResponseDto> AddCollaboratorAsync(Guid userId, Guid ideaId, IdeaCollaboratorDto collaboratorDto)
        //{
        //    var idea = await _ideaRepository.GetByIdAsync(ideaId);
        //    if (idea == null)
        //        throw new KeyNotFoundException("ایده یافت نشد.");

        //    if (idea.OwnerId != userId)
        //        throw new UnauthorizedAccessException("فقط مالک ایده می‌تواند همکار اضافه کند.");

        //    // بررسی وجود کاربر مورد نظر
        //    var targetUser = await _userRepository.GetByIdAsync(collaboratorDto.UserId);
        //    if (targetUser == null)
        //        throw new KeyNotFoundException("کاربر مورد نظر یافت نشد.");

        //    // بررسی همکار تکراری
        //    var existing = await _collaboratorRepository.Query()
        //        .FirstOrDefaultAsync(c => c.IdeaId == ideaId && c.UserId == collaboratorDto.UserId);

        //    if (existing != null)
        //        throw new InvalidOperationException("این کاربر قبلاً به عنوان همکار اضافه شده است.");

        //    var collaborator = new IdeaCollaborator
        //    {
        //        Id = Guid.NewGuid(),
        //        IdeaId = ideaId,
        //        UserId = collaboratorDto.UserId,
        //        Role = Enum.TryParse<CollaboratorRole>(collaboratorDto.Role, true, out var role)
        //            ? role : CollaboratorRole.Developer,
        //        Status = CollaboratorStatus.Pending,
        //        JoinedAt = DateTime.UtcNow
        //    };

        //    await _collaboratorRepository.AddAsync(collaborator);
        //    await _collaboratorRepository.SaveChangesAsync();

        //    // ارسال ایمیل دعوت‌نامه
        //    try
        //    {
        //        await _emailService.SendCollaborationInvitationAsync(
        //            targetUser.Email,
        //            idea.Title,
        //            idea.Owner.FullName,
        //            collaborator.Role.ToString()
        //        );
        //    }
        //    catch { /* خطای ایمیل را نادیده بگیر */ }

        //    return await MapToCollaboratorResponseDto(collaborator);
        //}

        public async Task<IdeaCollaboratorResponseDto> UpdateCollaboratorStatusAsync(Guid userId, Guid ideaId, Guid collaboratorId, UpdateCollaboratorStatusDto statusDto)
        {
            var idea = await _ideaRepository.GetByIdAsync(ideaId);
            if (idea == null)
                throw new KeyNotFoundException("ایده یافت نشد.");

            if (idea.OwnerId != userId)
                throw new UnauthorizedAccessException("فقط مالک ایده می‌تواند وضعیت همکار را تغییر دهد.");

            var collaborator = await _collaboratorRepository.Query()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == collaboratorId && c.IdeaId == ideaId);

            if (collaborator == null)
                throw new KeyNotFoundException("همکار یافت نشد.");

            if (!Enum.TryParse<CollaboratorStatus>(statusDto.Status, true, out var newStatus))
                throw new InvalidOperationException("وضعیت نامعتبر است.");

            collaborator.Status = newStatus;

            if (newStatus == CollaboratorStatus.Active)
            {
                // همکار فعال شد
                collaborator.JoinedAt = DateTime.UtcNow;
            }
            else if (newStatus == CollaboratorStatus.Inactive || newStatus == CollaboratorStatus.Rejected)
            {
                collaborator.LeftAt = DateTime.UtcNow;
            }

            _collaboratorRepository.Update(collaborator);
            await _collaboratorRepository.SaveChangesAsync();

            return await MapToCollaboratorResponseDto(collaborator);
        }

        //public async Task<bool> RemoveCollaboratorAsync(Guid userId, Guid ideaId, Guid collaboratorId)
        //{
        //    var idea = await _ideaRepository.GetByIdAsync(ideaId);
        //    if (idea == null)
        //        throw new KeyNotFoundException("ایده یافت نشد.");

        //    if (idea.OwnerId != userId)
        //        throw new UnauthorizedAccessException("فقط مالک ایده می‌تواند همکار را حذف کند.");

        //    var collaborator = await _collaboratorRepository.Query()
        //        .FirstOrDefaultAsync(c => c.Id == collaboratorId && c.IdeaId == ideaId);

        //    if (collaborator == null)
        //        throw new KeyNotFoundException("همکار یافت نشد.");

        //    await _collaboratorRepository.DeleteAsync(collaborator);
        //    await _collaboratorRepository.SaveChangesAsync();
        //    return true;
        //}

        //public async Task<List<IdeaCollaboratorResponseDto>> GetCollaboratorsAsync(Guid ideaId)
        //{
        //    var collaborators = await _collaboratorRepository.Query()
        //        .Include(c => c.User)
        //        .Where(c => c.IdeaId == ideaId)
        //        .OrderBy(c => c.Role)
        //        .ThenBy(c => c.User.FullName)
        //        .ToListAsync();

        //    var result = new List<IdeaCollaboratorResponseDto>();
        //    foreach (var collaborator in collaborators)
        //    {
        //        result.Add(await MapToCollaboratorResponseDto(collaborator));
        //    }
        //    return result;
        //}

        public async Task<IdeaCollaboratorResponseDto> GetCollaboratorByIdAsync(Guid collaboratorId)
        {
            var collaborator = await _collaboratorRepository.Query()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == collaboratorId);

            if (collaborator == null)
                throw new KeyNotFoundException("همکار یافت نشد.");

            return await MapToCollaboratorResponseDto(collaborator);
        }

        // متدهای کمکی

        private async Task<IdeaCollaboratorResponseDto> MapToCollaboratorResponseDto(IdeaCollaborator collaborator)
        {
            return new IdeaCollaboratorResponseDto
            {
                Id = collaborator.Id,
                IdeaId = collaborator.IdeaId,
                User = new UserDto
                {
                    Id = collaborator.User.Id,
                    FullName = collaborator.User.FullName,
                    Email = collaborator.User.Email,
                    Bio = collaborator.User.Bio,
                    AvatarUrl = collaborator.User.AvatarUrl,
                    Role = collaborator.User.Role.ToString()
                },
                Role = collaborator.Role.ToString(),
                Status = collaborator.Status.ToString(),
                JoinedAt = collaborator.JoinedAt,
                LeftAt = collaborator.LeftAt
            };
        }
    }
}