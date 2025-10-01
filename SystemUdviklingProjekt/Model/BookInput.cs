using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SystemUdviklingProjekt.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class BookInput
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Required]
        public string Title { get; set; } = "";

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        public string? Author { get; set; }

        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the genre.
        /// </summary>
        /// <value>
        /// The genre.
        /// </value>
        public string? Genre { get; set; }

        /// <summary>
        /// Gets or sets the number of books.
        /// </summary>
        /// <value>
        /// The number of books.
        /// </value>
        [Range(1, int.MaxValue)]
        public int NumberOfBooks { get; set; } = 1;

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the image file.
        /// </summary>
        /// <value>
        /// The image file.
        /// </value>
        public IFormFile? ImageFile { get; set; }

        /// <summary>
        /// Gets or sets the PDF file.
        /// </summary>
        /// <value>
        /// The PDF file.
        /// </value>
        public IFormFile? PdfFile { get; set; }
    }
}
