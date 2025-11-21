using System;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.ServiceModels
{
    // This model is used for transferring data between the WebApp and the Service Layer
    public class BookModel
    {
        public int BookID { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Book Code is required.")]
        [StringLength(50)]
        public string BookCode { get; set; }

        [StringLength(100)]
        public string Genre { get; set; }

        [StringLength(150)]
        public string Author { get; set; }

        [StringLength(150)]
        public string Publisher { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Default status

        // Extended Information
        [DataType(DataType.Date)]
        public DateTime? DatePublished { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }

        [StringLength(500)]
        public string CoverImagePath { get; set; }

        // Ebook Support
        public bool IsEbook { get; set; } = false;

        [StringLength(500)]
        public string EbookPath { get; set; }

        // Analytics
        public int ViewCount { get; set; } = 0;
        public int BorrowCount { get; set; } = 0;

        // Stock Management
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 999, ErrorMessage = "Quantity must be between 1 and 999.")]
        public int Quantity { get; set; } = 1;

        [Required(ErrorMessage = "Available Quantity is required.")]
        [Range(0, 999, ErrorMessage = "Available Quantity must be between 0 and 999.")]
        public int AvailableQuantity { get; set; } = 1;

        // Calculated fields (from Reviews)
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
    }
}
