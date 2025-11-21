// ASI.Basecode.Data.Models/User.cs
using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public partial class User
    {
        // Primary Key - Note the name change from 'Id' to 'UserID'
        public string UserId { get; set; } 
        
        // New fields based on ER Diagram
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // e.g., 'Admin' or 'Member'
        public bool IsUserActive { get; set; } = true; // User activation status

        // Password Reset Fields (CRITICAL FEATURE #1)
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // Audit Fields (Retaining the existing ones)
        public string CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedTime { get; set; }

        // -----------------------------------------------------------------
        // NAVIGATION PROPERTIES (To define the relationships for EF Core)
        // -----------------------------------------------------------------

        // USERS ||--|{ BORROWINGS : "initiates"
        public virtual ICollection<Borrowing> Borrowings { get; set; }

        // USERS ||--|{ REVIEWS : "writes"
        public virtual ICollection<Review> Reviews { get; set; }

        // USERS ||--|{ NOTIFICATIONS : "receives"
        public virtual ICollection<Notification> Notifications { get; set; }

        // USERS ||--|| USER_SETTINGS : "has" (One-to-One)
        // Note: ICollection is not strictly required for 1-to-1, 
        // but often used, or it can be a single object reference.
        public virtual UserSetting UserSetting { get; set; } 

        // Constructor to initialize collections
        public User()
        {
            Borrowings = new HashSet<Borrowing>();
            Reviews = new HashSet<Review>();
            Notifications = new HashSet<Notification>();
        }
    }
}