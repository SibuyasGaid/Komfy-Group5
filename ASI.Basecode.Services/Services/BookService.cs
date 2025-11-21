using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;

        // Inject the BookRepository
        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public List<BookModel> GetAvailableBooks()
        {
            var books = _bookRepository.GetBooks()
                .Where(b => b.Status == "Available")
                .ToList();

            // Mapping Data Model (Book) to Service Model (BookModel)
            return books.Select(b => new BookModel
            {
                BookID = b.BookID,
                Title = b.Title,
                BookCode = b.BookCode,
                Genre = b.Genre,
                Author = b.Author,
                Publisher = b.Publisher,
                Status = b.Status,
                DatePublished = b.DatePublished,
                Description = b.Description,
                CoverImagePath = b.CoverImagePath,
                IsEbook = b.IsEbook,
                EbookPath = b.EbookPath,
                ViewCount = b.ViewCount,
                BorrowCount = b.BorrowCount,
                Quantity = b.Quantity,
                AvailableQuantity = b.AvailableQuantity
            }).ToList();
        }

        public BookModel GetBookDetails(int bookId)
        {
            var book = _bookRepository.GetBookById(bookId);

            if (book == null)
            {
                return null;
            }

            return new BookModel
            {
                BookID = book.BookID,
                Title = book.Title,
                BookCode = book.BookCode,
                Genre = book.Genre,
                Author = book.Author,
                Publisher = book.Publisher,
                Status = book.Status,
                DatePublished = book.DatePublished,
                Description = book.Description,
                CoverImagePath = book.CoverImagePath,
                IsEbook = book.IsEbook,
                EbookPath = book.EbookPath,
                ViewCount = book.ViewCount,
                BorrowCount = book.BorrowCount,
                Quantity = book.Quantity,
                AvailableQuantity = book.AvailableQuantity
            };
        }

        public void AddBook(BookModel model)
        {
            // Business Logic: Check if book code already exists (basic validation)
            if (_bookRepository.GetBooks().Any(b => b.BookCode == model.BookCode))
            {
                throw new System.Exception("Book with this code already exists.");
            }

            // Mapping Service Model (BookModel) to Data Model (Book)
            var bookEntity = new Book
            {
                Title = model.Title,
                BookCode = model.BookCode,
                Genre = model.Genre,
                Author = model.Author,
                Publisher = model.Publisher,
                Status = model.Status,
                DatePublished = model.DatePublished,
                Description = model.Description,
                CoverImagePath = model.CoverImagePath,
                IsEbook = model.IsEbook,
                EbookPath = model.EbookPath,
                ViewCount = 0,
                BorrowCount = 0,
                Quantity = model.Quantity,
                AvailableQuantity = model.AvailableQuantity
            };

            _bookRepository.AddBook(bookEntity);
        }

        public void UpdateBook(BookModel model)
        {
            var bookEntity = _bookRepository.GetBookById(model.BookID);

            if (bookEntity == null)
            {
                throw new KeyNotFoundException($"Book with ID {model.BookID} not found.");
            }

            // Update fields
            bookEntity.Title = model.Title;
            bookEntity.BookCode = model.BookCode;
            bookEntity.Genre = model.Genre;
            bookEntity.Author = model.Author;
            bookEntity.Publisher = model.Publisher;
            bookEntity.Status = model.Status;
            bookEntity.DatePublished = model.DatePublished;
            bookEntity.Description = model.Description;
            bookEntity.CoverImagePath = model.CoverImagePath;
            bookEntity.IsEbook = model.IsEbook;
            bookEntity.EbookPath = model.EbookPath;
            bookEntity.Quantity = model.Quantity;
            bookEntity.AvailableQuantity = model.AvailableQuantity;

            _bookRepository.UpdateBook(bookEntity);
        }

        public void DeleteBook(int bookId)
        {
            var bookEntity = _bookRepository.GetBookById(bookId);

            if (bookEntity == null)
            {
                throw new KeyNotFoundException($"Book with ID {bookId} not found.");
            }

            // Set the status to deleted (soft delete pattern)
            // For a basic CRUD, we'll use the repository's hard delete for simplicity.
            _bookRepository.DeleteBook(bookEntity);
        }

        // QUICK WIN #1: Search and Filter Operations
        public List<BookModel> GetAllBooks()
        {
            var books = _bookRepository.GetBooks().ToList();

            return books.Select(b => new BookModel
            {
                BookID = b.BookID,
                Title = b.Title,
                BookCode = b.BookCode,
                Genre = b.Genre,
                Author = b.Author,
                Publisher = b.Publisher,
                Status = b.Status,
                DatePublished = b.DatePublished,
                Description = b.Description,
                CoverImagePath = b.CoverImagePath,
                IsEbook = b.IsEbook,
                EbookPath = b.EbookPath,
                ViewCount = b.ViewCount,
                BorrowCount = b.BorrowCount,
                Quantity = b.Quantity,
                AvailableQuantity = b.AvailableQuantity,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();
        }

        public (List<BookModel> Books, int TotalCount) GetAllBooksPaginated(int pageNumber, int pageSize)
        {
            var allBooks = _bookRepository.GetBooks().ToList();
            var totalCount = allBooks.Count;

            var paginatedBooks = allBooks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookModel
                {
                    BookID = b.BookID,
                    Title = b.Title,
                    BookCode = b.BookCode,
                    Genre = b.Genre,
                    Author = b.Author,
                    Publisher = b.Publisher,
                    Status = b.Status,
                    DatePublished = b.DatePublished,
                    Description = b.Description,
                    CoverImagePath = b.CoverImagePath,
                    IsEbook = b.IsEbook,
                    EbookPath = b.EbookPath,
                    ViewCount = b.ViewCount,
                    BorrowCount = b.BorrowCount,
                    Quantity = b.Quantity,
                    AvailableQuantity = b.AvailableQuantity,
                    AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                    ReviewCount = b.Reviews.Count
                }).ToList();

            return (paginatedBooks, totalCount);
        }

        public BookModel GetBookByCode(string bookCode)
        {
            var book = _bookRepository.GetBookByCode(bookCode);

            if (book == null)
            {
                return null;
            }

            return new BookModel
            {
                BookID = book.BookID,
                Title = book.Title,
                BookCode = book.BookCode,
                Genre = book.Genre,
                Author = book.Author,
                Publisher = book.Publisher,
                Status = book.Status,
                DatePublished = book.DatePublished,
                Description = book.Description,
                CoverImagePath = book.CoverImagePath,
                IsEbook = book.IsEbook,
                EbookPath = book.EbookPath,
                ViewCount = book.ViewCount,
                BorrowCount = book.BorrowCount,
                Quantity = book.Quantity,
                AvailableQuantity = book.AvailableQuantity,
                AverageRating = book.Reviews.Any() ? book.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = book.Reviews.Count
            };
        }

        public List<BookModel> SearchBooks(string searchTerm)
        {
            var books = _bookRepository.SearchBooks(searchTerm).ToList();

            return books.Select(b => new BookModel
            {
                BookID = b.BookID,
                Title = b.Title,
                BookCode = b.BookCode,
                Genre = b.Genre,
                Author = b.Author,
                Publisher = b.Publisher,
                Status = b.Status,
                DatePublished = b.DatePublished,
                Description = b.Description,
                CoverImagePath = b.CoverImagePath,
                IsEbook = b.IsEbook,
                EbookPath = b.EbookPath,
                ViewCount = b.ViewCount,
                BorrowCount = b.BorrowCount,
                Quantity = b.Quantity,
                AvailableQuantity = b.AvailableQuantity,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();
        }

        public List<BookModel> FilterBooks(string genre = null, string author = null, string publisher = null)
        {
            var books = _bookRepository.FilterBooks(genre, author, publisher).ToList();

            return books.Select(b => new BookModel
            {
                BookID = b.BookID,
                Title = b.Title,
                BookCode = b.BookCode,
                Genre = b.Genre,
                Author = b.Author,
                Publisher = b.Publisher,
                Status = b.Status,
                DatePublished = b.DatePublished,
                Description = b.Description,
                CoverImagePath = b.CoverImagePath,
                IsEbook = b.IsEbook,
                EbookPath = b.EbookPath,
                ViewCount = b.ViewCount,
                BorrowCount = b.BorrowCount,
                Quantity = b.Quantity,
                AvailableQuantity = b.AvailableQuantity,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();
        }

        // CRITICAL FEATURE #3: Advanced Filter with Date Published and Sorting
        public List<BookModel> FilterBooksAdvanced(string genre = null, string author = null, string publisher = null,
                                                   int? yearPublished = null, string sortBy = null)
        {
            var books = _bookRepository.FilterBooksAdvanced(genre, author, publisher, yearPublished, sortBy).ToList();

            return books.Select(b => new BookModel
            {
                BookID = b.BookID,
                Title = b.Title,
                BookCode = b.BookCode,
                Genre = b.Genre,
                Author = b.Author,
                Publisher = b.Publisher,
                Status = b.Status,
                DatePublished = b.DatePublished,
                Description = b.Description,
                CoverImagePath = b.CoverImagePath,
                IsEbook = b.IsEbook,
                EbookPath = b.EbookPath,
                ViewCount = b.ViewCount,
                BorrowCount = b.BorrowCount,
                Quantity = b.Quantity,
                AvailableQuantity = b.AvailableQuantity,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();
        }

        // QUICK WIN #5: Analytics Operations
        public void IncrementViewCount(int bookId)
        {
            _bookRepository.IncrementViewCount(bookId);
        }

        public void IncrementBorrowCount(int bookId)
        {
            _bookRepository.IncrementBorrowCount(bookId);
        }

        // ADVANCED FEATURE #3: Get All Ebooks
        public List<BookModel> GetAllEbooks()
        {
            var ebooks = _bookRepository.GetBooks()
                .Where(b => b.IsEbook && !string.IsNullOrEmpty(b.EbookPath))
                .ToList();

            return ebooks.Select(b => new BookModel
            {
                BookID = b.BookID,
                Title = b.Title,
                BookCode = b.BookCode,
                Genre = b.Genre,
                Author = b.Author,
                Publisher = b.Publisher,
                Status = b.Status,
                DatePublished = b.DatePublished,
                Description = b.Description,
                CoverImagePath = b.CoverImagePath,
                IsEbook = b.IsEbook,
                EbookPath = b.EbookPath,
                ViewCount = b.ViewCount,
                BorrowCount = b.BorrowCount,
                Quantity = b.Quantity,
                AvailableQuantity = b.AvailableQuantity,
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();
        }

        // ADVANCED FEATURE #3: Check if Ebook is Available
        public bool IsEbookAvailable(int bookId)
        {
            var book = _bookRepository.GetBookById(bookId);
            return book != null && book.IsEbook && !string.IsNullOrEmpty(book.EbookPath);
        }

        // ADVANCED FEATURE #3: Get Ebook File Path
        public string GetEbookPath(int bookId)
        {
            var book = _bookRepository.GetBookById(bookId);
            if (book == null || !book.IsEbook || string.IsNullOrEmpty(book.EbookPath))
            {
                throw new KeyNotFoundException($"Ebook not found for book ID {bookId}.");
            }
            return book.EbookPath;
        }
    }
}
