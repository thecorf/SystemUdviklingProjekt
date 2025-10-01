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

        var book = new BookModel
        {
            Title = Input.Title.Trim(),
            Author = Input.Author?.Trim(),
            Year = Input.Year,
            Genre = Input.Genre,
            NumberOfBooks = Math.Max(1, Input.NumberOfBooks),
            Description = Input.Description,
            CreatedBy = username
        };

        // Cover -> wwwroot/uploads/books
        if (Input.ImageFile is { Length: > 0 })
        {
            var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "books");
            Directory.CreateDirectory(uploadsRoot);
            var ext = Path.GetExtension(Input.ImageFile.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var absolutePath = Path.Combine(uploadsRoot, fileName);
            using var s = System.IO.File.Create(absolutePath);
            await Input.ImageFile.CopyToAsync(s);
            book.ImagePath = $"/uploads/books/{fileName}";
        }

        // PDF -> ContentRoot/Private/books  (bemærk: INGEN leading slash i PdfPath)
        if (Input.PdfFile is { Length: > 0 })
        {
            var ext = Path.GetExtension(Input.PdfFile.FileName).ToLowerInvariant();
            if (ext != ".pdf")
            {
                ModelState.AddModelError("Input.PdfFile", "Kun PDF-filer er tilladt.");
                return Page();
            }
            if (Input.PdfFile.Length > 50 * 1024 * 1024)
            {
                ModelState.AddModelError("Input.PdfFile", "PDF'en må maks. være 50 MB.");
                return Page();
            }

            var privateRoot = Path.Combine(_env.ContentRootPath, "Private", "books");
            Directory.CreateDirectory(privateRoot);
            var fileName = $"{Guid.NewGuid()}.pdf";
            var savePath = Path.Combine(privateRoot, fileName);
            using var fs = System.IO.File.Create(savePath);
            await Input.PdfFile.CopyToAsync(fs);

            book.PdfPath = Path.Combine("Private", "books", fileName); // fx "Private/books/xxx.pdf"
        }

        _repo.Add(book);
        TempData["Message"] = "Bogen er oprettet.";
        return RedirectToPage("/Books/Details", new { id = book.Id });
    }
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
        public IFormFile? PdfFile { get; set; }
    }


