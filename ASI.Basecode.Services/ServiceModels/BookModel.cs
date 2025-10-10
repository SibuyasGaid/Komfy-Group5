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

        // We can simplify status to a string for this basic CRUD
        [StringLength(50)]
        public string Status { get; set; } = "Available"; // Default status for a new book
    }
}
