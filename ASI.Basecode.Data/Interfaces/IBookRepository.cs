using ASI.Basecode.Data.Models;
using System.Linq;

namespace ASI.Basecode.Data.Interfaces
{
    public interface IBookRepository
    {
        IQueryable<Book> GetBooks();
        Book GetBookById(int bookId);
        Book GetBookByCode(string bookCode);
        IQueryable<Book> SearchBooks(string searchTerm);
        IQueryable<Book> FilterBooks(string genre = null, string author = null, string publisher = null);
        void AddBook(Book book);
        void UpdateBook(Book book);
        void DeleteBook(Book book);
        void IncrementViewCount(int bookId);
        void IncrementBorrowCount(int bookId);
    }
}