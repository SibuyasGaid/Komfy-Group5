using ASI.Basecode.Data.Interfaces;
using ASI.Basecode.Data.Models;
using Basecode.Data.Repositories;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ASI.Basecode.Data.Repositories
{
    public class BorrowingRepository : BaseRepository, IBorrowingRepository
    {
        public BorrowingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public IQueryable<Borrowing> GetBorrowings()
        {
            return this.GetDbSet<Borrowing>()
                .Include(b => b.User)
                .Include(b => b.Book);
        }

        public Borrowing GetBorrowingById(int borrowingId)
        {
            return this.GetDbSet<Borrowing>()
                .Include(b => b.User)
                .Include(b => b.Book)
                .FirstOrDefault(b => b.BorrowingID == borrowingId);
        }

        public IQueryable<Borrowing> GetBorrowingsByUserId(string userId)
        {
            return this.GetDbSet<Borrowing>()
                .Include(b => b.User)
                .Include(b => b.Book)
                .Where(b => b.UserId == userId);
        }

        public IQueryable<Borrowing> GetBorrowingsByBookId(int bookId)
        {
            return this.GetDbSet<Borrowing>()
                .Include(b => b.User)
                .Include(b => b.Book)
                .Where(b => b.BookID == bookId);
        }

        public IQueryable<Borrowing> GetActiveBorrowings()
        {
            return this.GetDbSet<Borrowing>()
                .Include(b => b.User)
                .Include(b => b.Book)
                .Where(b => b.Status == "Active");
        }

        public IQueryable<Borrowing> GetOverdueBorrowings()
        {
            var today = DateTime.Now.Date;
            return this.GetDbSet<Borrowing>()
                .Include(b => b.User)
                .Include(b => b.Book)
                .Where(b => (b.Status == "Active" || b.Status == "Overdue") && b.DueDate < today);
        }

        public void AddBorrowing(Borrowing borrowing)
        {
            this.GetDbSet<Borrowing>().Add(borrowing);
            UnitOfWork.SaveChanges();
        }

        public void UpdateBorrowing(Borrowing borrowing)
        {
            this.SetEntityState(borrowing, EntityState.Modified);
            UnitOfWork.SaveChanges();
        }

        public void DeleteBorrowing(Borrowing borrowing)
        {
            this.SetEntityState(borrowing, EntityState.Deleted);
            UnitOfWork.SaveChanges();
        }
    }
}