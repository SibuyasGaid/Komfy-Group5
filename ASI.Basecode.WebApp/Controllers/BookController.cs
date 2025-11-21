using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASI.Basecode.WebApp.Controllers
{
    /// <summary>
    /// Controller for managing book-related operations including listing, searching, filtering, and CRUD operations.
    /// Requires user authentication for all actions.
    /// </summary>
    [Authorize]
    public class BookController : Controller
    {
        private readonly ILogger<BookController> _logger;
        private readonly IBookService _bookService;
        private readonly IReviewService _reviewService;
        private readonly IBorrowingService _borrowingService;

        /// <summary>
        /// Initializes a new instance of the BookController with required service dependencies.
        /// </summary>
        /// <param name="logger">Logger instance for error tracking and debugging</param>
        /// <param name="bookService">Service for book-related business logic</param>
        /// <param name="reviewService">Service for review-related operations</param>
        /// <param name="borrowingService">Service for borrowing history operations</param>
        public BookController(ILogger<BookController> logger, IBookService bookService, IReviewService reviewService, IBorrowingService borrowingService)
        {
            _logger = logger;
            _bookService = bookService;
            _reviewService = reviewService;
            _borrowingService = borrowingService;
        }

        /// <summary>
        /// Displays the book catalog page with pagination, search, and filtering capabilities.
        /// </summary>
        /// <param name="searchTerm">Search term to filter books by title, code, author, genre, or description</param>
        /// <param name="genre">Filter books by genre</param>
        /// <param name="author">Filter books by author name</param>
        /// <param name="publisher">Filter books by publisher</param>
        /// <param name="yearPublished">Filter books by publication year</param>
        /// <param name="minRating">Filter books by minimum average rating</param>
        /// <param name="page">Current page number for pagination (default: 1)</param>
        /// <returns>View displaying filtered and paginated book list</returns>
        public IActionResult Index(string searchTerm, string genre, string author, string publisher, int? yearPublished, decimal? minRating, int page = 1)
        {
            const int pageSize = 10;
            List<BookModel> books;

            // Get all books first (either from search or all books)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                books = _bookService.SearchBooks(searchTerm);
                ViewBag.SearchTerm = searchTerm;
            }
            else
            {
                books = _bookService.GetAllBooks();
            }

            // Load all reviews for rating calculation BEFORE filtering
            var allReviewsData = _reviewService.GetAllReviews()
                .GroupBy(r => r.BookID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Populate review statistics for ALL books
            foreach (var book in books)
            {
                if (allReviewsData.TryGetValue(book.BookID, out var reviews))
                {
                    book.ReviewCount = reviews.Count;
                    book.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                }
                else
                {
                    book.ReviewCount = 0;
                    book.AverageRating = 0;
                }
            }

            // Apply filters BEFORE pagination
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

            // Get total count AFTER filtering
            var totalCount = books.Count;

            // Apply pagination AFTER filtering
            var paginatedBooks = books
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Load reviews and borrowings only for paginated books
            var bookIds = paginatedBooks.Select(b => b.BookID).ToList();
            var allReviews = allReviewsData
                .Where(kvp => bookIds.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var allBorrowings = _borrowingService.GetAllBorrowings()
                .Where(b => bookIds.Contains(b.BookID))
                .GroupBy(b => b.BookID)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Prepare filter dropdown options from all books in the system
            var allBooks = _bookService.GetAllBooks();
            ViewBag.Genres = allBooks.Where(b => !string.IsNullOrWhiteSpace(b.Genre)).Select(b => b.Genre).Distinct().OrderBy(g => g).ToList();
            ViewBag.Authors = allBooks.Where(b => !string.IsNullOrWhiteSpace(b.Author)).Select(b => b.Author).Distinct().OrderBy(a => a).ToList();
            ViewBag.Publishers = allBooks.Where(b => !string.IsNullOrWhiteSpace(b.Publisher)).Select(b => b.Publisher).Distinct().OrderBy(p => p).ToList();
            ViewBag.Years = allBooks.Where(b => b.DatePublished.HasValue).Select(b => b.DatePublished.Value.Year).Distinct().OrderByDescending(y => y).ToList();

            // Prepare review data for modal display using pre-loaded bulk data
            ViewBag.AllReviews = paginatedBooks.ToDictionary(
                b => b.BookID,
                b => allReviews.TryGetValue(b.BookID, out var reviews) ? reviews : new List<ReviewModel>()
            );

            // Prepare borrowing history for modal display using pre-loaded bulk data
            ViewBag.AllBorrowings = paginatedBooks.ToDictionary(
                b => b.BookID,
                b => allBorrowings.TryGetValue(b.BookID, out var borrowings) ? borrowings : new List<BorrowingModel>()
            );

            // Pagination data
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;

            return View(paginatedBooks);
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
        public IActionResult Create(BookModel model, IFormFile CoverImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Handle cover image upload
                    if (CoverImage != null && CoverImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                        var uniqueFileName = $"{Guid.NewGuid()}_{CoverImage.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            CoverImage.CopyTo(fileStream);
                        }

                        model.CoverImagePath = $"/uploads/covers/{uniqueFileName}";
                    }

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
        public IActionResult Edit(int id, BookModel model, IFormFile CoverImage)
        {
            if (id != model.BookID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle cover image upload
                    if (CoverImage != null && CoverImage.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                        Directory.CreateDirectory(uploadsFolder); // Ensure directory exists

                        var uniqueFileName = $"{Guid.NewGuid()}_{CoverImage.FileName}";
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            CoverImage.CopyTo(fileStream);
                        }

                        // Delete old cover image if it exists and is a local file
                        if (!string.IsNullOrEmpty(model.CoverImagePath) && model.CoverImagePath.StartsWith("/uploads/"))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", model.CoverImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        model.CoverImagePath = $"/uploads/covers/{uniqueFileName}";
                    }

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

            // Load reviews for this book
            var reviews = _reviewService.GetReviewsByBookId(id);
            ViewBag.Reviews = reviews;
            ViewBag.AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
            ViewBag.ReviewCount = reviews.Count;

            // Load borrowing history for this book
            var borrowings = _borrowingService.GetAllBorrowings()
                .Where(b => b.BookID == id)
                .OrderByDescending(b => b.BorrowDate)
                .ToList();
            ViewBag.Borrowings = borrowings;

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
