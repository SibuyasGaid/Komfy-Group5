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
        private readonly IReviewService _reviewService;

        // Inject IBookService and IReviewService
        public BookController(ILogger<BookController> logger, IBookService bookService, IReviewService reviewService)
        {
            _logger = logger;
            _bookService = bookService;
            _reviewService = reviewService;
        }

        // GET: /Book/Index (READ: List all available books)
        public IActionResult Index(string searchTerm, string genre, string author, string publisher, int? yearPublished, decimal? minRating)
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

            // Populate review data for each book
            foreach (var book in books)
            {
                book.AverageRating = _reviewService.GetAverageRatingForBook(book.BookID);
                var reviews = _reviewService.GetReviewsByBookId(book.BookID);
                book.ReviewCount = reviews.Count;
            }

            // Apply filters
            if (!string.IsNullOrWhiteSpace(genre))
            {
                books = books.Where(b => b.Genre != null && b.Genre.Equals(genre, System.StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.SelectedGenre = genre;
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                books = books.Where(b => b.Author != null && b.Author.Equals(author, System.StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.SelectedAuthor = author;
            }

            if (!string.IsNullOrWhiteSpace(publisher))
            {
                books = books.Where(b => b.Publisher != null && b.Publisher.Equals(publisher, System.StringComparison.OrdinalIgnoreCase)).ToList();
                ViewBag.SelectedPublisher = publisher;
            }

            if (yearPublished.HasValue)
            {
                books = books.Where(b => b.DatePublished.HasValue && b.DatePublished.Value.Year == yearPublished.Value).ToList();
                ViewBag.SelectedYear = yearPublished.Value;
            }

            if (minRating.HasValue)
            {
                books = books.Where(b => b.AverageRating >= (double)minRating.Value).ToList();
                ViewBag.SelectedRating = minRating.Value;
            }

            // Get unique values for filter dropdowns (from all books)
            var allBooks = _bookService.GetAvailableBooks();
            ViewBag.Genres = allBooks.Where(b => !string.IsNullOrWhiteSpace(b.Genre)).Select(b => b.Genre).Distinct().OrderBy(g => g).ToList();
            ViewBag.Authors = allBooks.Where(b => !string.IsNullOrWhiteSpace(b.Author)).Select(b => b.Author).Distinct().OrderBy(a => a).ToList();
            ViewBag.Publishers = allBooks.Where(b => !string.IsNullOrWhiteSpace(b.Publisher)).Select(b => b.Publisher).Distinct().OrderBy(p => p).ToList();
            ViewBag.Years = allBooks.Where(b => b.DatePublished.HasValue).Select(b => b.DatePublished.Value.Year).Distinct().OrderByDescending(y => y).ToList();

            // Pass reviews to ViewBag for display in modals
            ViewBag.AllReviews = books.ToDictionary(
                b => b.BookID,
                b => _reviewService.GetReviewsByBookId(b.BookID)
            );

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
