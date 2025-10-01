using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Service;

/// <summary>
/// 
/// </summary>
public class CreateBookModel : PageModel
{
    /// <summary>
    /// The repo
    /// </summary>
    private readonly BooksRepository _repo;
    /// <summary>
    /// The env
    /// </summary>
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateBookModel"/> class.
    /// </summary>
    /// <param name="repo">The repo.</param>
    /// <param name="env">The env.</param>
    public CreateBookModel(BooksRepository repo, IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    /// <summary>
    /// Gets or sets the input.
    /// </summary>
    /// <value>
    /// The input.
    /// </value>
    [BindProperty] public BookInput Input { get; set; } = new();

    /// <summary>Called when [get].</summary>
    /// <returns>
    ///   <br />
    /// </returns>
    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Create") });
        return Page();
    }

    /// <summary>
    /// Called when [post asynchronous].
    /// </summary>
    /// <returns></returns>
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
    [Required, Display(Name = "Titel")]
        public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author.
    /// </summary>
    /// <value>
    /// The author.
    /// </value>
    [Display(Name = "Forfatter")]
        public string? Author { get; set; }

    /// <summary>
    /// Gets or sets the year.
    /// </summary>
    /// <value>
    /// The year.
    /// </value>
    [Display(Name = "År")]
        public int? Year { get; set; }

    /// <summary>
    /// Gets or sets the genre.
    /// </summary>
    /// <value>
    /// The genre.
    /// </value>
    [Display(Name = "Genre")]
        public string? Genre { get; set; }

    /// <summary>
    /// Gets or sets the number of books.
    /// </summary>
    /// <value>
    /// The number of books.
    /// </value>
    [Range(1, 999), Display(Name = "Antal bøger")]
        public int NumberOfBooks { get; set; } = 1;

    /// <summary>
    /// Gets or sets the description.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    [Display(Name = "Beskrivelse")]
        public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the image file.
    /// </summary>
    /// <value>
    /// The image file.
    /// </value>
    [Display(Name = "Billede")]
        public IFormFile? ImageFile { get; set; }
    /// <summary>
    /// Gets or sets the PDF file.
    /// </summary>
    /// <value>
    /// The PDF file.
    /// </value>
    public IFormFile? PdfFile { get; set; }
    }


