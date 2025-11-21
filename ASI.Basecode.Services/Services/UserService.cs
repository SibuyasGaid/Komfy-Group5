using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Manager;
using ASI.Basecode.Services.ServiceModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static ASI.Basecode.Resources.Constants.Enums;

namespace ASI.Basecode.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository repository, IMapper mapper)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public LoginResult AuthenticateUser(string userId, string password, ref User user)
        {
            user = new User();
            var passwordKey = PasswordManager.EncryptPassword(password);
            user = _repository.GetUsers().Where(x => x.UserId == userId &&
                                                     x.Password == passwordKey).FirstOrDefault();

            if (user == null)
            {
                return LoginResult.Failed;
            }

            // Check if user account is active
            if (!user.IsUserActive)
            {
                return LoginResult.Failed;
            }

            return LoginResult.Success;
        }

        public List<UserModel> GetAllUsers()
        {
            var users = _repository.GetUsers().ToList();

            // Mapping Data Model (User) to Service Model (UserModel)
            return users.Select(u => new UserModel
            {
                UserId = u.UserId,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsUserActive = u.IsUserActive,
                CreatedBy = u.CreatedBy,
                CreatedTime = u.CreatedTime,
                UpdatedBy = u.UpdatedBy,
                UpdatedTime = u.UpdatedTime
            }).ToList();
        }

        public UserModel GetUserDetails(string userId)
        {
            var user = _repository.GetUserById(userId);

            if (user == null)
            {
                return null;
            }

            return new UserModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsUserActive = user.IsUserActive,
                CreatedBy = user.CreatedBy,
                CreatedTime = user.CreatedTime,
                UpdatedBy = user.UpdatedBy,
                UpdatedTime = user.UpdatedTime
            };
        }

        public UserModel GetUserByEmail(string email)
        {
            var user = _repository.GetUserByEmail(email);

            if (user == null)
            {
                return null;
            }

            return new UserModel
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsUserActive = user.IsUserActive,
                CreatedBy = user.CreatedBy,
                CreatedTime = user.CreatedTime,
                UpdatedBy = user.UpdatedBy,
                UpdatedTime = user.UpdatedTime
            };
        }

        public void AddUser(UserModel model)
        {
            // Business Logic: Check if user ID or email already exists
            if (_repository.UserExists(model.UserId))
            {
                throw new Exception("User with this ID already exists.");
            }

            if (_repository.EmailExists(model.Email))
            {
                throw new Exception("User with this email already exists.");
            }

            // Mapping Service Model (UserModel) to Data Model (User)
            var userEntity = new User
            {
                UserId = model.UserId,
                Name = model.Name,
                Email = model.Email,
                Password = PasswordManager.EncryptPassword(model.Password),
                Role = model.Role,
                CreatedTime = DateTime.Now,
                UpdatedTime = DateTime.Now,
                CreatedBy = System.Environment.UserName,
                UpdatedBy = System.Environment.UserName
            };

            _repository.AddUser(userEntity);
        }

        public void AddUser(UserViewModel model)
        {
            var user = new User();
            // Prevent duplicate UserId or Email for registrations
            if (!_repository.UserExists(model.UserId))
            {
                if (!string.IsNullOrEmpty(model.Email) && _repository.EmailExists(model.Email))
                {
                    throw new InvalidDataException("Email already exists.");
                }
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
                user.Role = "Member"; // Set default role to Member for new registrations
                user.CreatedTime = DateTime.Now;
                user.UpdatedTime = DateTime.Now;
                user.CreatedBy = System.Environment.UserName;
                user.UpdatedBy = System.Environment.UserName;

                _repository.AddUser(user);
            }
            else
            {
                throw new InvalidDataException(Resources.Messages.Errors.UserExists);
            }
        }

        public void UpdateUser(UserModel model)
        {
            var userEntity = _repository.GetUserById(model.UserId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {model.UserId} not found.");
            }

            // Check if email is being changed and if it already exists for another user
            if (userEntity.Email != model.Email && _repository.EmailExists(model.Email))
            {
                throw new Exception("Email already exists for another user.");
            }

            // Update fields
            userEntity.Name = model.Name;
            userEntity.Email = model.Email;
            userEntity.Role = model.Role;

            // Update password only if a new password is provided
            if (!string.IsNullOrEmpty(model.Password))
            {
                userEntity.Password = PasswordManager.EncryptPassword(model.Password);
            }

            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        public void DeleteUser(string userId)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            _repository.DeleteUser(userEntity);
        }

        // DEACTIVATE USER (Soft Delete)
        public void DeactivateUser(string userId)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (!userEntity.IsUserActive)
            {
                throw new Exception("User is already deactivated.");
            }

            userEntity.IsUserActive = false;
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // ACTIVATE USER
        public void ActivateUser(string userId)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            if (userEntity.IsUserActive)
            {
                throw new Exception("User is already active.");
            }

            userEntity.IsUserActive = true;
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // TOGGLE USER ACTIVATION
        public void ToggleUserActivation(string userId)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            userEntity.IsUserActive = !userEntity.IsUserActive;
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // QUICK WIN #4: Grant Admin Access
        public void GrantAdminAccess(string userId)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Only update if user is not already an admin
            if (userEntity.Role == "Admin")
            {
                throw new Exception("User is already an Admin.");
            }

            userEntity.Role = "Admin";
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // QUICK WIN #4: Revoke Admin Access
        public void RevokeAdminAccess(string userId)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Only update if user is currently an admin
            if (userEntity.Role != "Admin")
            {
                throw new Exception("User is not an Admin.");
            }

            userEntity.Role = "Member";
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // CRITICAL FEATURE #1: Generate Password Reset Token
        public string GeneratePasswordResetToken(string email)
        {
            var userEntity = _repository.GetUserByEmail(email);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"No user found with email {email}.");
            }

            // Generate a unique token (GUID)
            var token = Guid.NewGuid().ToString();

            // Set token and expiry (valid for 1 hour)
            userEntity.PasswordResetToken = token;
            userEntity.PasswordResetTokenExpiry = DateTime.Now.AddHours(1);

            _repository.UpdateUser(userEntity);

            return token;
        }

        // CRITICAL FEATURE #1: Validate Password Reset Token
        public bool ValidatePasswordResetToken(string token)
        {
            var userEntity = _repository.GetUserByPasswordResetToken(token);
            return userEntity != null;
        }

        // CRITICAL FEATURE #1: Reset Password
        public void ResetPassword(string token, string newPassword)
        {
            var userEntity = _repository.GetUserByPasswordResetToken(token);

            if (userEntity == null)
            {
                throw new Exception("Invalid or expired password reset token.");
            }

            // Update password
            userEntity.Password = PasswordManager.EncryptPassword(newPassword);

            // Clear the reset token
            userEntity.PasswordResetToken = null;
            userEntity.PasswordResetTokenExpiry = null;

            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // CRITICAL FEATURE #4: Update User Profile (Name, Email)
        public void UpdateUserProfile(string userId, string name, string email)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Check if email is being changed and if it already exists for another user
            if (userEntity.Email != email && _repository.EmailExists(email))
            {
                throw new Exception("Email already exists for another user.");
            }

            // Update profile fields
            userEntity.Name = name;
            userEntity.Email = email;
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }

        // CRITICAL FEATURE #4: Change Password
        public void ChangePassword(string userId, string currentPassword, string newPassword)
        {
            var userEntity = _repository.GetUserById(userId);

            if (userEntity == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Verify current password
            var encryptedCurrentPassword = PasswordManager.EncryptPassword(currentPassword);
            if (userEntity.Password != encryptedCurrentPassword)
            {
                throw new Exception("Current password is incorrect.");
            }

            // Update to new password
            userEntity.Password = PasswordManager.EncryptPassword(newPassword);
            userEntity.UpdatedTime = DateTime.Now;
            userEntity.UpdatedBy = System.Environment.UserName;

            _repository.UpdateUser(userEntity);
        }
    }
}
