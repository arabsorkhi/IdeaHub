using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Data;
namespace Domain.Entity
{
    
 
        public class IdeaCollaborator
        {
            [Key]
            public Guid Id { get; set; } = Guid.NewGuid();

            public Guid IdeaId { get; set; }
            public virtual Idea Idea { get; set; } = null!;

            public Guid UserId { get; set; }
            public virtual User User { get; set; } = null!;

            public CollaboratorRole Role { get; set; }
            public CollaboratorStatus Status { get; set; } = CollaboratorStatus.Pending;

            public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
            public DateTime? LeftAt { get; set; }
        }
        public class CollaborationRequest
        {
            public Guid Id { get; set; }

            public Guid IdeaId { get; set; }

            public Guid UserId { get; set; }

            public RequestStatus Status { get; set; }

            public DateTime CreatedAt { get; set; }
        }
        public enum RequestStatus
        {
            Pending,

            Accepted,

            Rejected
        }
        public class Connection
        {
            public Guid Id { get; set; }
            public Guid IdeaId { get; set; }
            public string UserId { get; set; } // Developer/Investor
            public ConnectionType Type { get; set; } // WantToDevelop, WantToInvest
            public string? Message { get; set; }
            public ConnectionStatus Status { get; set; } // Pending, Accepted, Rejected
        }

        public enum ConnectionStatus
        {
            Pending, Accepted, Rejected
        }

        public enum ConnectionType
        {
              WantToDevelop  ,
                   WantToInvest
        }

        public class NdaAgreement
        {
            public Guid Id { get; set; }
            public Guid ConnectionId { get; set; }
            public DateTime SignedAt { get; set; }
            public bool IsActive { get; set; }
        }

        public enum CollaboratorRole
        {
            Developer,
            Designer,
            Marketing,
            Advisor,
            Investor
        }

        public enum CollaboratorStatus
        {
            Pending,
            Active,
            Inactive,
            Rejected
        }
    }
 
