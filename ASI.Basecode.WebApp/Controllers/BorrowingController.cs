using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous] // Temporarily allow access to all users for development
    public class BorrowingController : Controller
    {
        private readonly ILogger<BorrowingController> _logger;
        private readonly IBorrowingService _borrowingService;
        private readonly IBookService _bookService;

        // Inject IBorrowingService and IBookService
        public BorrowingController(ILogger<BorrowingController> logger, IBorrowingService borrowingService, IBookService bookService)
        {
            _logger = logger;
            _borrowingService = borrowingService;
            _bookService = bookService;
        }

        // GET: /Borrowing/Index (READ: List all borrowings)
        public IActionResult Index()
        {
            List<BorrowingModel> borrowings;

            // If user is Admin, show all borrowings; otherwise show only their own
            if (User.IsInRole("Admin"))
            {
                borrowings = _borrowingService.GetAllBorrowings();
            }
            else
            {
                // Get current logged-in user's ID from claims
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "You must be logged in to view borrowings.";
                    return RedirectToAction("Login", "Account");
                }

                borrowings = _borrowingService.GetBorrowingsByUserId(userId);
            }

            return View(borrowings);
        }

        // GET: /Borrowing/Active (READ: List active borrowings)
        public IActionResult Active()
        {
            var borrowings = _borrowingService.GetActiveBorrowings();
            return View(borrowings);
        }

        // GET: /Borrowing/Overdue (READ: List overdue borrowings)
        public IActionResult Overdue()
        {
            var borrowings = _borrowingService.GetOverdueBorrowings();
            return View(borrowings);
        }

        // GET: /Borrowing/Create (CREATE: Display form)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Borrowing/Create (CREATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BorrowingModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _borrowingService.AddBorrowing(model);
                    TempData["SuccessMessage"] = "Borrowing record created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    // Add model error if business logic fails
                    ModelState.AddModelError(string.Empty, $"Error creating borrowing: {ex.Message}");
                    _logger.LogError(ex, "Error creating borrowing.");
                }
            }
            return View(model);
        }

        // GET: /Borrowing/Edit/{id} (UPDATE: Display form with existing data)
        public IActionResult Edit(int id)
        {
            var borrowing = _borrowingService.GetBorrowingDetails(id);
            if (borrowing == null)
            {
                return NotFound();
            }
            return View(borrowing);
        }

        // POST: /Borrowing/Edit/{id} (UPDATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, BorrowingModel model)
        {
            if (id != model.BorrowingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _borrowingService.UpdateBorrowing(model);
                    TempData["SuccessMessage"] = "Borrowing record updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error updating borrowing: {ex.Message}");
                    _logger.LogError(ex, "Error updating borrowing.");
                }
            }
            return View(model);
        }

        // POST: /Borrowing/Return/{id} (UPDATE: Mark book as returned)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Return(int id)
        {
            try
            {
                _borrowingService.ReturnBook(id);
                TempData["SuccessMessage"] = "Book returned successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Borrowing record not found.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error returning book: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Borrowing/MarkOverdue/{id} (UPDATE: Mark as overdue)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkOverdue(int id)
        {
            try
            {
                _borrowingService.MarkAsOverdue(id);
                TempData["SuccessMessage"] = "Borrowing marked as overdue.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Borrowing record not found.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error marking as overdue: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Borrowing/Delete/{id} (DELETE: Process deletion)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _borrowingService.DeleteBorrowing(id);
                TempData["SuccessMessage"] = "Borrowing record deleted successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Borrowing record not found or already deleted.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting borrowing: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /Borrowing/Details/{id} (READ: View borrowing details)
        public IActionResult Details(int id)
        {
            var borrowing = _borrowingService.GetBorrowingDetails(id);
            if (borrowing == null)
            {
                return NotFound();
            }
            return View(borrowing);
        }

        // QUICK WIN #3: GET: /Borrowing/MyHistory (READ: Current user's borrowing history)
        public IActionResult MyHistory()
        {
            // Get current logged-in user's ID from claims
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "You must be logged in to view your borrowing history.";
                return RedirectToAction("Login", "Account");
            }

            var borrowings = _borrowingService.GetBorrowingsByUserId(userId);
            return View("UserBorrowings", borrowings);
        }

        // GET: /Borrowing/UserBorrowings/{userId} (READ: List borrowings by user)
        public IActionResult UserBorrowings(string userId)
        {
            var borrowings = _borrowingService.GetBorrowingsByUserId(userId);
            return View(borrowings);
        }

        // GET: /Borrowing/BookBorrowings/{bookId} (READ: List borrowings by book)
        public IActionResult BookBorrowings(int bookId)
        {
            var borrowings = _borrowingService.GetBorrowingsByBookId(bookId);
            return View(borrowings);
        }

        // POST: /Borrowing/BorrowBook (CREATE: Quick borrow from Books page)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BorrowBook(int bookId, DateTime borrowDate)
        {
            try
            {
                // Get current logged-in user's ID from claims
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "You must be logged in to borrow a book.";
                    return RedirectToAction("Login", "Account");
                }

                // Validate borrow date is not in the past
                if (borrowDate.Date < DateTime.Now.Date)
                {
                    TempData["ErrorMessage"] = "Borrow date cannot be in the past.";
                    return RedirectToAction("Index", "Book");
                }

                // Create a new borrowing record
                var borrowingModel = new BorrowingModel
                {
                    UserId = userId,
                    BookID = bookId,
                    BorrowDate = borrowDate,
                    DueDate = borrowDate.AddDays(14), // 14 days borrowing period from selected date
                    Status = "Active"
                };

                _borrowingService.AddBorrowing(borrowingModel);

                if (borrowDate.Date == DateTime.Now.Date)
                {
                    TempData["SuccessMessage"] = "Book borrowed successfully! Due date is " + borrowingModel.DueDate.ToString("MMM dd, yyyy");
                }
                else
                {
                    TempData["SuccessMessage"] = "Book reserved successfully! Borrow date: " + borrowingModel.BorrowDate.ToString("MMM dd, yyyy") + ", Due date: " + borrowingModel.DueDate.ToString("MMM dd, yyyy");
                }

                return RedirectToAction("Index", "Book");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error borrowing book: {ex.Message}";
                _logger.LogError(ex, "Error borrowing book.");
                return RedirectToAction("Index", "Book");
            }
        }

        // POST: /Borrowing/CancelReservation/{id} (DELETE: Cancel future reservation)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelReservation(int id)
        {
            try
            {
                var borrowing = _borrowingService.GetBorrowingDetails(id);

                if (borrowing == null)
                {
                    TempData["ErrorMessage"] = "Borrowing record not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get current logged-in user's ID from claims
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Check if user owns this borrowing (unless they're admin)
                if (!User.IsInRole("Admin") && borrowing.UserId != userId)
                {
                    TempData["ErrorMessage"] = "You can only cancel your own reservations.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if borrow date is in the future
                if (borrowing.BorrowDate.Date < DateTime.Now.Date)
                {
                    TempData["ErrorMessage"] = "Cannot cancel reservation - the borrow date has already passed.";
                    return RedirectToAction(nameof(Index));
                }

                // Check if already cancelled
                if (borrowing.Status == "Cancelled")
                {
                    TempData["ErrorMessage"] = "This reservation has already been cancelled.";
                    return RedirectToAction(nameof(Index));
                }

                // Mark the reservation as cancelled and set return date
                borrowing.Status = "Cancelled";
                borrowing.ReturnDate = DateTime.Now; // Set return date to now
                _borrowingService.UpdateBorrowing(borrowing);

                // Get the book and mark it as available
                var book = _bookService.GetBookDetails(borrowing.BookID);
                if (book != null)
                {
                    book.Status = "Available";
                    book.BorrowCount = Math.Max(0, book.BorrowCount - 1); // Decrement borrow count
                    _bookService.UpdateBook(book);
                }

                TempData["SuccessMessage"] = "Reservation cancelled successfully. The book is now available.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Borrowing record not found or already deleted.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling reservation: {ex.Message}";
                _logger.LogError(ex, "Error cancelling reservation.");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
