using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SystemUdviklingProjekt.Model
{
    public class BookInput
    {
        [Required]
        public string Title { get; set; } = "";

        public string? Author { get; set; }

        public int? Year { get; set; }

        public string? Genre { get; set; }

        [Range(1, int.MaxValue)]
        public int NumberOfBooks { get; set; } = 1;

        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public IFormFile? PdfFile { get; set; }
    }
}
