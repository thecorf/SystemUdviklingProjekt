using System.ComponentModel.DataAnnotations;

namespace SystemUdviklingProjekt.Model
{
    /// <summary>
    /// Represents a rating for a book, including the username, star value, optional comment, and creation date.
    /// </summary>
    public class Rating
    {
        /// <summary>
        /// The username of the user who submitted the rating.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The number of stars given in the rating (1 to 5).
        /// </summary>
        [Range(1, 5)]
        public int Stars { get; set; }        // 1..5

        /// <summary>
        /// An optional comment provided by the user.
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// The UTC date and time when the rating was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }



    /// <summary>
    /// Represents a book in the system, including metadata, rental information, and ratings.
    /// </summary>
    public class BookModel
    {
        public string? PdfPath { get; set; }
        /// <summary>
        /// The unique identifier for the book.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The title of the book.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The author of the book.
        /// </summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>
        /// The publication year of the book.
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// The genre of the book.
        /// </summary>
        public string? Genre { get; set; }

        /// <summary>
        /// A description of the book.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// The file path to the book's image.
        /// </summary>
        public string? ImagePath { get; set; }  

        /// <summary>
        /// The URL to the book's image. This is ignored during JSON serialization.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string? ImageUrl
        {
            get => ImagePath;
            set => ImagePath = value;
        }

        /// <summary>
        /// The total number of copies of the book available.
        /// </summary>
        public int NumberOfBooks { get; set; } = 1;              

        /// <summary>
        /// The list of usernames who have rented this book.
        /// </summary>
        public List<string> RentedByUsers { get; set; } = new();  

        /// <summary>
        /// The username of the user who created this book entry.
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// The UTC date and time when the book entry was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The list of ratings submitted for this book.
        /// </summary>
        public List<Rating> Ratings { get; set; } = new();

        /// <summary>
        /// The number of times the book has been rented. Ignored during JSON serialization.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public int RentedCount => RentedByUsers?.Count ?? 0;

        /// <summary>
        /// The number of available copies of the book. Ignored during JSON serialization.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public int AvailableCopies => Math.Max(0, NumberOfBooks - RentedCount);

        /// <summary>
        /// Indicates whether the book is available for rent. Ignored during JSON serialization.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public bool IsAvailable => AvailableCopies > 0;

        /// <summary>
        /// The average rating of the book, rounded to one decimal place. Ignored during JSON serialization.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public double AverageRating => Ratings.Count == 0
            ? 0
            : Math.Round(Ratings.Average(r => r.Stars), 1);

        /// <summary>
        /// The total number of ratings for the book. Ignored during JSON serialization.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public int RatingsCount => Ratings.Count;
    }
}
