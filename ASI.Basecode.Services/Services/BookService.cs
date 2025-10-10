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
                Status = b.Status
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
                Status = book.Status
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
                Status = model.Status
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
    }
}
