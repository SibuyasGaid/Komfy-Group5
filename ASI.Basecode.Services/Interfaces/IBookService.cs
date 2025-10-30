using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookService
    {
        // READ operations
        List<BookModel> GetAvailableBooks();
        List<BookModel> GetAllBooks();
        BookModel GetBookDetails(int bookId);
        BookModel GetBookByCode(string bookCode);

        // SEARCH & FILTER operations (QUICK WIN #1)
        List<BookModel> SearchBooks(string searchTerm);
        List<BookModel> FilterBooks(string genre = null, string author = null, string publisher = null);

        // CREATE operation
        void AddBook(BookModel model);

        // UPDATE operation
        void UpdateBook(BookModel model);

        // DELETE operation
        void DeleteBook(int bookId);

        // ANALYTICS operations (QUICK WIN #5)
        void IncrementViewCount(int bookId);
        void IncrementBorrowCount(int bookId);
    }
}
