using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IUserService
    {
        // Authentication (existing method)
        LoginResult AuthenticateUser(string userid, string password, ref User user);

        // READ operations
        List<UserModel> GetAllUsers();
        UserModel GetUserDetails(string userId);
        UserModel GetUserByEmail(string email);

        // CREATE operation
        void AddUser(UserModel model);
        void AddUser(UserViewModel model); // Keep existing for backward compatibility

        // UPDATE operation
        void UpdateUser(UserModel model);

        // DELETE operation
        void DeleteUser(string userId);

        // QUICK WIN #4: Admin access management
        void GrantAdminAccess(string userId);
        void RevokeAdminAccess(string userId);
    }
}
