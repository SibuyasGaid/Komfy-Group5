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

            return user != null ? LoginResult.Success : LoginResult.Failed;
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
            if (!_repository.UserExists(model.UserId))
            {
                _mapper.Map(model, user);
                user.Password = PasswordManager.EncryptPassword(model.Password);
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
    }
}
