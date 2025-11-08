using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
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
        public IActionResult Index(string searchTerm)
        {
            List<BookModel> books;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = _bookService.SearchBooks(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                books = _bookService.GetAvailableBooks();
            }

            return View(books);
        }

        // GET: /Book/Create (CREATE: Display form)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Book/Create (CREATE: Process form submission)
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

        // QUICK WIN #1: Search and Filter Operations
        // GET: /Book/Search?searchTerm=harry
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var books = _bookService.SearchBooks(searchTerm);
            ViewBag.SearchTerm = searchTerm;
            return View("Index", books);
        }

        // GET: /Book/Filter?genre=Fiction&author=Rowling
        public IActionResult Filter(string genre, string author, string publisher)
        {
            var books = _bookService.FilterBooks(genre, author, publisher);
            ViewBag.Genre = genre;
            ViewBag.Author = author;
            ViewBag.Publisher = publisher;
            return View("Index", books);
        }

        // CRITICAL FEATURE #3: GET: /Book/FilterAdvanced?genre=Fiction&yearPublished=2020&sortBy=popularity
        public IActionResult FilterAdvanced(string genre, string author, string publisher, int? yearPublished, string sortBy)
        {
            var books = _bookService.FilterBooksAdvanced(genre, author, publisher, yearPublished, sortBy);
            ViewBag.Genre = genre;
            ViewBag.Author = author;
            ViewBag.Publisher = publisher;
            ViewBag.YearPublished = yearPublished;
            ViewBag.SortBy = sortBy;
            return View("Index", books);
        }

        // GET: /Book/Details/5 (READ: View book details with reviews and ratings)
        public IActionResult Details(int id)
        {
            var book = _bookService.GetBookDetails(id);
            if (book == null)
            {
                return NotFound();
            }

            // Increment view count when viewing details
            _bookService.IncrementViewCount(id);

            return View(book);
        }

        // ADVANCED FEATURE #3: GET: /Book/Ebooks (List all ebooks)
        public IActionResult Ebooks()
        {
            var ebooks = _bookService.GetAllEbooks();
            return View(ebooks);
        }

        // ADVANCED FEATURE #3: GET: /Book/ViewEbook/5 (View ebook in browser)
        public IActionResult ViewEbook(int id)
        {
            try
            {
                var book = _bookService.GetBookDetails(id);
                if (book == null)
                {
                    return NotFound();
                }

                if (!_bookService.IsEbookAvailable(id))
                {
                    TempData["ErrorMessage"] = "This book is not available as an ebook.";
                    return RedirectToAction("Details", new { id });
                }

                // Increment view count
                _bookService.IncrementViewCount(id);

                ViewBag.EbookPath = book.EbookPath;
                return View(book);
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading ebook.";
                _logger.LogError(ex, "Error loading ebook.");
                return RedirectToAction("Index");
            }
        }

        // ADVANCED FEATURE #3: GET: /Book/DownloadEbook/5 (Download ebook file)
        public IActionResult DownloadEbook(int id)
        {
            try
            {
                var book = _bookService.GetBookDetails(id);
                if (book == null)
                {
                    return NotFound();
                }

                if (!_bookService.IsEbookAvailable(id))
                {
                    TempData["ErrorMessage"] = "This book is not available as an ebook.";
                    return RedirectToAction("Details", new { id });
                }

                var ebookPath = _bookService.GetEbookPath(id);

                // In production, you would read the file from the filesystem
                // For now, return a placeholder response
                TempData["SuccessMessage"] = $"Ebook download initiated for: {book.Title}";
                return RedirectToAction("Details", new { id });
            }
            catch (System.Exception ex)
            {
                TempData["ErrorMessage"] = "Error downloading ebook.";
                _logger.LogError(ex, "Error downloading ebook.");
                return RedirectToAction("Details", new { id });
            }
        }
    }
}
