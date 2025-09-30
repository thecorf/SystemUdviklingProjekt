using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Repo;
using SystemUdviklingProjekt.Service;
using static CreateBookModel;


namespace SystemUdviklingProjekt.Pages.Books
{
    public class EditBookModel : PageModel
    {
        private readonly BooksRepository _repo;
        private readonly IWebHostEnvironment _env;

        public EditBookModel(BooksRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty] public BookInput Input { get; set; } = new();
        public BookModel? Book { get; set; }

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

            Input.Title = Book.Title;
            Input.Author = Book.Author;
            Input.Year = Book.Year;
            Input.Genre = Book.Genre;
            Input.NumberOfBooks = (int)Book.NumberOfBooks; // <-- vigtigt
            Input.Description = Book.Description;
            Input.ImageFile = null;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
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

            // billede
            string? imagePath = book.ImagePath;
            if (Input.ImageFile is not null && Input.ImageFile.Length > 0)
            {
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", "books");
                Directory.CreateDirectory(uploadsRoot);
                var ext = Path.GetExtension(Input.ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var absolutePath = Path.Combine(uploadsRoot, fileName);
                using var stream = System.IO.File.Create(absolutePath);
                await Input.ImageFile.CopyToAsync(stream);
                imagePath = $"/uploads/books/{fileName}";
            }

            // valider total ift. aktive lejemål
            var rentedCount = book.RentedByUsers?.Count ?? 0;
            if (Input.NumberOfBooks < rentedCount)
            {
                ModelState.AddModelError("Input.NumberOfBooks", $"Antal kan ikke være under {rentedCount}, der er aktive lejemål.");
                return Page();
            }

            // opdater
            book.Title = Input.Title.Trim();
            book.Author = Input.Author?.Trim() ?? "";
            book.Year = Input.Year;
            book.Genre = Input.Genre;
            book.NumberOfBooks = Math.Max(1, Input.NumberOfBooks);
            book.Description = Input.Description;
            book.ImagePath = imagePath;

            var ok = _repo.Update(book); 
            TempData["Message"] = ok ? "Bogen er opdateret." : "Kunne ikke gemme ændringer.";
            return RedirectToPage("/UserProfile");
        }
    }
}






