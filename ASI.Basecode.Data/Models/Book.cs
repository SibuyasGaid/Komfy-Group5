using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASI.Basecode.Data.Models
{
    public partial class Book
    {
        // Primary Key
        public int BookID { get; set; }

        // Basic Information
        public string Title { get; set; }
        public string BookCode { get; set; }
        public string Genre { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Status { get; set; } // e.g., 'Available', 'Borrowed'

        // Extended Information (NEW FIELDS)
        public DateTime? DatePublished { get; set; }
        public string Description { get; set; } // Book description/summary
        public string CoverImagePath { get; set; } // Path to book cover image

        // Ebook Support (NEW FIELDS)
        public bool IsEbook { get; set; } // Flag to indicate if this is an ebook
        public string EbookPath { get; set; } // Path to ebook file (PDF, EPUB, etc.)

        // Analytics (NEW FIELDS)
        public int ViewCount { get; set; } = 0; // Number of times book details viewed
        public int BorrowCount { get; set; } = 0; // Number of times book borrowed

        // Stock Management (NEW FIELDS)
        public int Quantity { get; set; } = 1; // Total number of copies in library
        public int AvailableQuantity { get; set; } = 1; // Number of copies currently available for borrowing

        // -----------------------------------------------------------------
        // NAVIGATION PROPERTIES
        // -----------------------------------------------------------------

        // BOOKS ||--|{ BORROWINGS : "is part of"
        public virtual ICollection<Borrowing> Borrowings { get; set; }

        // BOOKS ||--|{ REVIEWS : "receives"
        public virtual ICollection<Review> Reviews { get; set; }

        public Book()
        {
            Borrowings = new HashSet<Borrowing>();
            Reviews = new HashSet<Review>();
            ViewCount = 0;
            BorrowCount = 0;
            IsEbook = false;
            Quantity = 1;
            AvailableQuantity = 1;
        }
    }
}
