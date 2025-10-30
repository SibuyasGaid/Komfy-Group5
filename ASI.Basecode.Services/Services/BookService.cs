using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;
using System.Linq;

namespace ASI.Basecode.Services
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
                BorrowCount = b.BorrowCount
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
                BorrowCount = book.BorrowCount
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
                BorrowCount = 0
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
                AverageRating = b.Reviews.Any() ? b.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = b.Reviews.Count
            }).ToList();
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
    }
}
