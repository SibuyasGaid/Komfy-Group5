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

        // Inject IBorrowingService
        public BorrowingController(ILogger<BorrowingController> logger, IBorrowingService borrowingService)
        {
            _logger = logger;
            _borrowingService = borrowingService;
        }

        // GET: /Borrowing/Index (READ: List all borrowings)
        public IActionResult Index()
        {
            var borrowings = _borrowingService.GetAllBorrowings();
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
        public IActionResult BorrowBook(int bookId)
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

                // Create a new borrowing record
                var borrowingModel = new BorrowingModel
                {
                    UserId = userId,
                    BookID = bookId,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(14), // Default 14 days borrowing period
                    Status = "Active"
                };

                _borrowingService.AddBorrowing(borrowingModel);
                TempData["SuccessMessage"] = "Book borrowed successfully! Due date is " + borrowingModel.DueDate.ToString("MMM dd, yyyy");

                return RedirectToAction("Index", "Book");
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error borrowing book: {ex.Message}";
                _logger.LogError(ex, "Error borrowing book.");
                return RedirectToAction("Index", "Book");
            }
        }
    }
}
