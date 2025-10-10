using ASI.Basecode.Services.ServiceModels;
using System.Collections.Generic;

namespace ASI.Basecode.Services.Interfaces
{
    public interface IBookService
    {
        // READ operations
        List<BookModel> GetAvailableBooks();
        BookModel GetBookDetails(int bookId);

        // CREATE operation
        void AddBook(BookModel model);

        // UPDATE operation
        void UpdateBook(BookModel model);

        // DELETE operation
        void DeleteBook(int bookId);
    }
}
