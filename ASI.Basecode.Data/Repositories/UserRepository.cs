using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<User> GetUsers()
        {
            return this.GetDbSet<User>();
        }

        public User GetUserById(string userId)
        {
            return this.GetDbSet<User>().FirstOrDefault(u => u.UserId == userId);
        }

        public User GetUserByEmail(string email)
        {
            return this.GetDbSet<User>().FirstOrDefault(u => u.Email == email);
        }

        public void AddUser(User user)
        {
            this.GetDbSet<User>().Add(user);
            UnitOfWork.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var dbSet = this.GetDbSet<User>();
            var existingUser = dbSet.Find(user.UserId);
            if (existingUser != null)
            {
                // Update all properties
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Password = user.Password;
                existingUser.Role = user.Role;
                existingUser.IsUserActive = user.IsUserActive;
                existingUser.UpdatedBy = user.UpdatedBy;
                existingUser.UpdatedTime = user.UpdatedTime;
                existingUser.PasswordResetToken = user.PasswordResetToken;
                existingUser.PasswordResetTokenExpiry = user.PasswordResetTokenExpiry;
            }
            UnitOfWork.SaveChanges();
        }

        public void DeleteUser(User user)
        {
            this.SetEntityState(user, EntityState.Deleted);
            UnitOfWork.SaveChanges();
        }

        public bool UserExists(string userId)
        {
            return this.GetDbSet<User>().Any(x => x.UserId == userId);
        }

        public bool EmailExists(string email)
        {
            return this.GetDbSet<User>().Any(x => x.Email == email);
        }

        // CRITICAL FEATURE #1: Password Reset
        public User GetUserByPasswordResetToken(string token)
        {
            return this.GetDbSet<User>()
                .FirstOrDefault(u => u.PasswordResetToken == token &&
                                    u.PasswordResetTokenExpiry > System.DateTime.Now);
        }
    }
}