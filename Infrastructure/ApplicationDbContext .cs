
using Domain.Entity;
 
 
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<User> Users { get; set; }
            public DbSet<Idea> Ideas { get; set; }
           public DbSet<CollaborationRequest> Collaborations { get; set; }
            public DbSet<IdeaReview> IdeaReviews { get; set; }
            public DbSet<IdeaCollaborator> IdeaCollaborators { get; set; }
            public DbSet<IdeaTag> IdeaTags { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // تنظیمات ایندکس
                modelBuilder.Entity<Idea>()
                    .HasIndex(i => new { i.OwnerId, i.Status });

                modelBuilder.Entity<Idea>()
                    .HasIndex(i => i.Visibility);

                modelBuilder.Entity<User>()
                    .HasIndex(u => u.Email)
                    .IsUnique();

                // روابط
                modelBuilder.Entity<Idea>()
                    .HasOne(i => i.Owner)
                    .WithMany(u => u.Ideas)
                    .HasForeignKey(i => i.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<IdeaReview>()
                    .HasOne(r => r.Idea)
                    .WithMany(i => i.Reviews)
                    .HasForeignKey(r => r.IdeaId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<IdeaReview>()
                    .HasOne(r => r.Reviewer)
                    .WithMany(u => u.Reviews)
                    .HasForeignKey(r => r.ReviewerId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<IdeaCollaborator>()
                    .HasOne(c => c.Idea)
                    .WithMany(i => i.Collaborators)
                    .HasForeignKey(c => c.IdeaId)
                    .OnDelete(DeleteBehavior.Cascade);

                modelBuilder.Entity<IdeaCollaborator>()
                    .HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<IdeaTag>()
                    .HasOne(t => t.Idea)
                    .WithMany(i => i.Tags)
                    .HasForeignKey(t => t.IdeaId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
 
