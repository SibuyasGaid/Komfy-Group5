using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [AllowAnonymous] // <--- ADD THIS LINE TEMPORARILY TO ALLOW ACCESS TO ALL USERS
    public class BookController : Controller
    {
        private readonly ILogger<BookController> _logger;
        private readonly IBookService _bookService;

        // Inject IBookService
        public BookController(ILogger<BookController> logger, IBookService bookService)
        {
            _logger = logger;
            _bookService = bookService;
        }

        // GET: /Book/Index (READ: List all available books)
        public IActionResult Index()
        {
            var books = _bookService.GetAvailableBooks();
            return View(books);
        }

        // GET: /Book/Create (CREATE: Display form)
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Book/Create (CREATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _bookService.AddBook(model);
                    TempData["SuccessMessage"] = "Book added successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    // Add model error if business logic fails (e.g., duplicate BookCode)
                    ModelState.AddModelError(string.Empty, $"Error adding book: {ex.Message}");
                    _logger.LogError(ex, "Error adding book.");
                }
            }
            return View(model);
        }

        // GET: /Book/Edit/5 (UPDATE: Display form with existing data)
        public IActionResult Edit(int id)
        {
            var book = _bookService.GetBookDetails(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: /Book/Edit/5 (UPDATE: Process form submission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, BookModel model)
        {
            if (id != model.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _bookService.UpdateBook(model);
                    TempData["SuccessMessage"] = "Book updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error updating book: {ex.Message}");
                    _logger.LogError(ex, "Error updating book.");
                }
            }
            return View(model);
        }

        // POST: /Book/Delete/5 (DELETE: Process deletion)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                _bookService.DeleteBook(id);
                TempData["SuccessMessage"] = "Book deleted successfully.";
            }
            catch (KeyNotFoundException)
            {
                TempData["ErrorMessage"] = "Book not found or already deleted.";
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting book: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
