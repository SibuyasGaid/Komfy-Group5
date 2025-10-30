using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        // The main entity sets (DbSet) for all six tables:
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<Borrowing> Borrowings { get; set; }
        public virtual DbSet<Review> Reviews { get; set; }
        public virtual DbSet<Notification> Notifications { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // All explicit Fluent API configuration has been removed.
            // EF Core will now use the clean definitions in your .cs model files
            // to generate the entire schema, including the new six tables
            // and all foreign key relationships.

            modelBuilder.Entity<UserSetting>(entity =>
            {
                entity.HasKey(e => e.SettingID);

                // Optional: Also define the one-to-one relationship to enforce the constraint
                entity.HasOne(e => e.User)
                    .WithOne(u => u.UserSetting)
                    .HasForeignKey<UserSetting>(e => e.UserId);
            });

            // Seed default users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = "user",
                    Name = "Regular User",
                    Email = "user@komfy.com",
                    Password = "kQFwF4qT5I8C+TfGr5H8IA==", // Encrypted "user"
                    Role = "Member",
                    CreatedBy = "System",
                    CreatedTime = new DateTime(2025, 1, 1),
                    UpdatedBy = "System",
                    UpdatedTime = new DateTime(2025, 1, 1)
                },
                new User
                {
                    UserId = "admin",
                    Name = "Administrator",
                    Email = "admin@komfy.com",
                    Password = "QpillzkpeKyc+8j/cuKetg==", // Encrypted "admin"
                    Role = "Admin",
                    CreatedBy = "System",
                    CreatedTime = new DateTime(2025, 1, 1),
                    UpdatedBy = "System",
                    UpdatedTime = new DateTime(2025, 1, 1)
                }
            );

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}