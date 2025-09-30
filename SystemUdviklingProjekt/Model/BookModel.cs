using SystemUdviklingProjekt.Repo;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Pages.Books;
namespace SystemUdviklingProjekt.Model
{
    public class BookModel
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Unique identifier for each book
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int? Year { get; set; }
        public string Genre { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        public string? ImagePath { get; set; }
        public int? NumberOfBooks { get; set; } // Number of copies available
        public string? CreatedBy { get; set; }   
        public string? RentedBy { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Timestamp of creation
        public List<string> RentedByUsers { get; set; } = new(); // List of users who have rented the bookc

        public int AvailableCopies => Math.Max(0, (int)(NumberOfBooks - (RentedByUsers?.Count ?? 0)));

        public bool IsAvailable => string.IsNullOrEmpty(RentedBy); // Indicates if the book is currently available for rent
    }
}
