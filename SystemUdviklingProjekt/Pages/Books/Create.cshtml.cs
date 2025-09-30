using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Service;

public class CreateBookModel : PageModel
{
    private readonly BooksRepository _repo;
    private readonly IWebHostEnvironment _env;

    public CreateBookModel(BooksRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    [BindProperty] public BookInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Create") });
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Create") });

        if (!ModelState.IsValid) return Page();

        string? imagePath = null;

        // Gem uploaded fil (hvis nogen)
        if (Input.ImageFile is not null && Input.ImageFile.Length > 0)
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "books");
            Directory.CreateDirectory(uploadsRoot);

            var ext = Path.GetExtension(Input.ImageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var absolutePath = Path.Combine(uploadsRoot, fileName);

            using (var stream = System.IO.File.Create(absolutePath))
                await Input.ImageFile.CopyToAsync(stream);

            imagePath = $"/uploads/books/{fileName}";
        }
        else
        {
            // fallback – læg en fil "book-placeholder.png" i wwwroot/images
            imagePath = "/images/book-placeholder.png";
        }

        var book = new BookModel
        {
            Title = Input.Title.Trim(),
            Author = Input.Author?.Trim() ?? "",
            Year = Input.Year,
            Genre = Input.Genre,
            NumberOfBooks = Math.Max(1, Input.NumberOfBooks),
            Description = Input.Description,
            ImagePath = imagePath,
            CreatedBy = username
        };

        _repo.Add(book);
        TempData["Message"] = "Bogen er oprettet.";
        return RedirectToPage("/AvailableBooks");
    }

    public class BookInput
    {
        [Required, Display(Name = "Titel")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Forfatter")]
        public string? Author { get; set; }

        [Display(Name = "År")]
        public int? Year { get; set; }

        [Display(Name = "Genre")]
        public string? Genre { get; set; }

        [Range(1, 999), Display(Name = "Antal bøger")]
        public int NumberOfBooks { get; set; } = 1;

        [Display(Name = "Beskrivelse")]
        public string? Description { get; set; }

        [Display(Name = "Billede")]
        public IFormFile? ImageFile { get; set; }
    }
}
