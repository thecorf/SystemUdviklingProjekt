using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Repo;
using SystemUdviklingProjekt.Service;
using static CreateBookModel;


namespace SystemUdviklingProjekt.Pages.Books
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
    public class EditBookModel : PageModel
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
        /// Initializes a new instance of the <see cref="EditBookModel"/> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        /// <param name="env">The env.</param>
        public EditBookModel(BooksRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the input.
        /// </summary>
        /// <value>
        /// The input.
        /// </value>
        [BindProperty] public BookInput Input { get; set; } = new();
        /// <summary>
        /// Gets or sets the book.
        /// </summary>
        /// <value>
        /// The book.
        /// </value>
        public BookModel? Book { get; set; }

        /// <summary>
        /// Called when [get].
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/EditBook", new { id = Id }) });

            Book = _repo.GetById(Id);
            if (Book == null) return RedirectToPage("/UserProfile");
            if (!string.Equals(Book.CreatedBy, username, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Message"] = "Du har ikke tilladelse til at redigere denne bog.";
                return RedirectToPage("/UserProfile");
            }
            // Populate Input model with existing book data

            Input.Title = Book.Title;
            Input.Author = Book.Author;
            Input.Year = Book.Year;
            Input.Genre = Book.Genre;
            Input.NumberOfBooks = (int)Book.NumberOfBooks; // <-- vigtigt
            Input.Description = Book.Description;
            Input.ImageFile = null;
            return Page();

        }

        /// <summary>
        ///     
        public async Task<IActionResult> OnPostAsync() // Called when [post asynchronous].
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/EditBook", new { id = Id }) });

            if (!ModelState.IsValid) return Page();

            var book = _repo.GetById(Id);
            if (book == null || !string.Equals(book.CreatedBy, username, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Message"] = "Du har ikke tilladelse til at redigere denne bog.";
                return RedirectToPage("/UserProfile");
            }

            string? imagePath = book.ImagePath;
            if (Input.ImageFile is not null && Input.ImageFile.Length > 0) // New image uploaded
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "books"); // wwwroot/uploads/books
                Directory.CreateDirectory(uploadsRoot);
                var ext = Path.GetExtension(Input.ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var absolutePath = Path.Combine(uploadsRoot, fileName);
                using var stream = System.IO.File.Create(absolutePath);
                await Input.ImageFile.CopyToAsync(stream);
                imagePath = $"/uploads/books/{fileName}";
            }

            if (Input.PdfFile is { Length: > 0 })
            {
                if (Path.GetExtension(Input.PdfFile.FileName).ToLowerInvariant() != ".pdf")
                {
                    ModelState.AddModelError("Input.PdfFile", "Kun PDF-filer er tilladt.");
                    return Page();
                }

                var privateRoot = Path.Combine(_env.ContentRootPath, "Private", "books"); // ContentRoot/Private/books
                Directory.CreateDirectory(privateRoot);
                var fileName = $"{Guid.NewGuid()}.pdf";
                var savePath = Path.Combine(privateRoot, fileName);
                using var fs = System.IO.File.Create(savePath);
                await Input.PdfFile.CopyToAsync(fs);

                // Update the PdfPath to the new file
                book.PdfPath = Path.Combine("Private", "books", fileName);
                var rentedCount = book.RentedByUsers?.Count ?? 0;
                if (Input.NumberOfBooks < rentedCount)
                {
                    ModelState.AddModelError("Input.NumberOfBooks", $"Antal kan ikke være under {rentedCount}, der er aktive lejemål.");
                    return Page();
                }
                // Update other fields and save
                book.Title = Input.Title.Trim();
                book.Author = Input.Author?.Trim() ?? "";
                book.Year = Input.Year;
                book.Genre = Input.Genre;
                book.NumberOfBooks = Math.Max(1, Input.NumberOfBooks);
                book.Description = Input.Description;
                book.ImagePath = imagePath;

                var ok = _repo.Update(book);
                TempData["Message"] = "Bogen er opdateret.";
                return RedirectToPage("/Books/Details", new { id = book.Id });

            }

            // If no PDF file is uploaded, update other fields and save
            var rentedCountNoPdf = book.RentedByUsers?.Count ?? 0;
            if (Input.NumberOfBooks < rentedCountNoPdf)
            {
                ModelState.AddModelError("Input.NumberOfBooks", $"Antal kan ikke være under {rentedCountNoPdf}, der er aktive lejemål.");
                return Page();
            }

            book.Title = Input.Title.Trim();
            book.Author = Input.Author?.Trim() ?? "";
            book.Year = Input.Year;
            book.Genre = Input.Genre;
            book.NumberOfBooks = Math.Max(1, Input.NumberOfBooks);
            book.Description = Input.Description;
            book.ImagePath = imagePath;

            var okNoPdf = _repo.Update(book);
            TempData["Message"] = "Bogen er opdateret.";
            return RedirectToPage("/Books/Details", new { id = book.Id });
        }
    }
}






