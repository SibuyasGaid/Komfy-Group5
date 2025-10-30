using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class BorrowingService : IBorrowingService
    {
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        // Inject repositories and notification service
        public BorrowingService(
            IBorrowingRepository borrowingRepository,
            IBookRepository bookRepository,
            IUserRepository userRepository,
            INotificationService notificationService)
        {
            _borrowingRepository = borrowingRepository;
            _bookRepository = bookRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }

        public List<BorrowingModel> GetAllBorrowings()
        {
            var borrowings = _borrowingRepository.GetBorrowings().ToList();

            // Mapping Data Model (Borrowing) to Service Model (BorrowingModel)
            return borrowings.Select(b => new BorrowingModel
            {
                BorrowingID = b.BorrowingID,
                UserId = b.UserId,
                BookID = b.BookID,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status,
                UserName = b.User?.Name,
                UserEmail = b.User?.Email,
                BookTitle = b.Book?.Title,
                BookCode = b.Book?.BookCode
            }).ToList();
        }

        public BorrowingModel GetBorrowingDetails(int borrowingId)
        {
            var borrowing = _borrowingRepository.GetBorrowingById(borrowingId);

            if (borrowing == null)
            {
                return null;
            }

            return new BorrowingModel
            {
                BorrowingID = borrowing.BorrowingID,
                UserId = borrowing.UserId,
                BookID = borrowing.BookID,
                BorrowDate = borrowing.BorrowDate,
                DueDate = borrowing.DueDate,
                ReturnDate = borrowing.ReturnDate,
                Status = borrowing.Status,
                UserName = borrowing.User?.Name,
                UserEmail = borrowing.User?.Email,
                BookTitle = borrowing.Book?.Title,
                BookCode = borrowing.Book?.BookCode
            };
        }

        public List<BorrowingModel> GetBorrowingsByUserId(string userId)
        {
            var borrowings = _borrowingRepository.GetBorrowingsByUserId(userId).ToList();

            return borrowings.Select(b => new BorrowingModel
            {
                BorrowingID = b.BorrowingID,
                UserId = b.UserId,
                BookID = b.BookID,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status,
                UserName = b.User?.Name,
                UserEmail = b.User?.Email,
                BookTitle = b.Book?.Title,
                BookCode = b.Book?.BookCode
            }).ToList();
        }

        public List<BorrowingModel> GetBorrowingsByBookId(int bookId)
        {
            var borrowings = _borrowingRepository.GetBorrowingsByBookId(bookId).ToList();

            return borrowings.Select(b => new BorrowingModel
            {
                BorrowingID = b.BorrowingID,
                UserId = b.UserId,
                BookID = b.BookID,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status,
                UserName = b.User?.Name,
                UserEmail = b.User?.Email,
                BookTitle = b.Book?.Title,
                BookCode = b.Book?.BookCode
            }).ToList();
        }

        public List<BorrowingModel> GetActiveBorrowings()
        {
            var borrowings = _borrowingRepository.GetActiveBorrowings().ToList();

            return borrowings.Select(b => new BorrowingModel
            {
                BorrowingID = b.BorrowingID,
                UserId = b.UserId,
                BookID = b.BookID,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status,
                UserName = b.User?.Name,
                UserEmail = b.User?.Email,
                BookTitle = b.Book?.Title,
                BookCode = b.Book?.BookCode
            }).ToList();
        }

        public List<BorrowingModel> GetOverdueBorrowings()
        {
            var borrowings = _borrowingRepository.GetOverdueBorrowings().ToList();

            return borrowings.Select(b => new BorrowingModel
            {
                BorrowingID = b.BorrowingID,
                UserId = b.UserId,
                BookID = b.BookID,
                BorrowDate = b.BorrowDate,
                DueDate = b.DueDate,
                ReturnDate = b.ReturnDate,
                Status = b.Status,
                UserName = b.User?.Name,
                UserEmail = b.User?.Email,
                BookTitle = b.Book?.Title,
                BookCode = b.Book?.BookCode
            }).ToList();
        }

        public void AddBorrowing(BorrowingModel model)
        {
            // Business Logic: Check if user exists
            if (!_userRepository.UserExists(model.UserId))
            {
                throw new Exception("User not found.");
            }

            // Business Logic: Check if book exists
            var book = _bookRepository.GetBookById(model.BookID);
            if (book == null)
            {
                throw new Exception("Book not found.");
            }

            // Business Logic: Check if book is available
            if (book.Status != "Available")
            {
                throw new Exception("Book is not available for borrowing.");
            }

            // Mapping Service Model (BorrowingModel) to Data Model (Borrowing)
            var borrowingEntity = new Borrowing
            {
                UserId = model.UserId,
                BookID = model.BookID,
                BorrowDate = model.BorrowDate,
                DueDate = model.DueDate,
                ReturnDate = null,
                Status = "Active"
            };

            _borrowingRepository.AddBorrowing(borrowingEntity);

            // Update book status to "Borrowed"
            book.Status = "Borrowed";
            _bookRepository.UpdateBook(book);

            // Increment borrow count for analytics (QUICK WIN #5)
            _bookRepository.IncrementBorrowCount(book.BookID);

            // QUICK WIN #2: Create notification for successful borrow
            var notification = new NotificationModel
            {
                UserId = model.UserId,
                Message = $"Successfully borrowed '{book.Title}' (Code: {book.BookCode}). Due date: {model.DueDate:MMM dd, yyyy}",
                Timestamp = DateTime.Now,
                IsRead = false
            };
            _notificationService.AddNotification(notification);
        }

        public void UpdateBorrowing(BorrowingModel model)
        {
            var borrowingEntity = _borrowingRepository.GetBorrowingById(model.BorrowingID);

            if (borrowingEntity == null)
            {
                throw new KeyNotFoundException($"Borrowing with ID {model.BorrowingID} not found.");
            }

            // Update fields
            borrowingEntity.UserId = model.UserId;
            borrowingEntity.BookID = model.BookID;
            borrowingEntity.BorrowDate = model.BorrowDate;
            borrowingEntity.DueDate = model.DueDate;
            borrowingEntity.ReturnDate = model.ReturnDate;
            borrowingEntity.Status = model.Status;

            _borrowingRepository.UpdateBorrowing(borrowingEntity);
        }

        public void ReturnBook(int borrowingId)
        {
            var borrowingEntity = _borrowingRepository.GetBorrowingById(borrowingId);

            if (borrowingEntity == null)
            {
                throw new KeyNotFoundException($"Borrowing with ID {borrowingId} not found.");
            }

            // Update borrowing record
            borrowingEntity.ReturnDate = DateTime.Now;
            borrowingEntity.Status = "Returned";
            _borrowingRepository.UpdateBorrowing(borrowingEntity);

            // Update book status back to "Available"
            var book = _bookRepository.GetBookById(borrowingEntity.BookID);
            if (book != null)
            {
                book.Status = "Available";
                _bookRepository.UpdateBook(book);
            }

            // QUICK WIN #2: Create notification for successful return
            var notification = new NotificationModel
            {
                UserId = borrowingEntity.UserId,
                Message = $"Successfully returned '{book?.Title}' (Code: {book?.BookCode}). Thank you!",
                Timestamp = DateTime.Now,
                IsRead = false
            };
            _notificationService.AddNotification(notification);
        }

        public void MarkAsOverdue(int borrowingId)
        {
            var borrowingEntity = _borrowingRepository.GetBorrowingById(borrowingId);

            if (borrowingEntity == null)
            {
                throw new KeyNotFoundException($"Borrowing with ID {borrowingId} not found.");
            }

            borrowingEntity.Status = "Overdue";
            _borrowingRepository.UpdateBorrowing(borrowingEntity);
        }

        public void DeleteBorrowing(int borrowingId)
        {
            var borrowingEntity = _borrowingRepository.GetBorrowingById(borrowingId);

            if (borrowingEntity == null)
            {
                throw new KeyNotFoundException($"Borrowing with ID {borrowingId} not found.");
            }

            // If the borrowing is still active, make the book available again
            if (borrowingEntity.Status == "Active" || borrowingEntity.Status == "Overdue")
            {
                var book = _bookRepository.GetBookById(borrowingEntity.BookID);
                if (book != null)
                {
                    book.Status = "Available";
                    _bookRepository.UpdateBook(book);
                }
            }

            _borrowingRepository.DeleteBorrowing(borrowingEntity);
        }
    }
}
