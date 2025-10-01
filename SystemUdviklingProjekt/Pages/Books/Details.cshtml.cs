using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.IO;
using System.Linq;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Service;

namespace SystemUdviklingProjekt.Pages.Books
{
    public class BookDetailsModel : PageModel
    {
        private readonly BooksRepository _repo;
        private readonly IWebHostEnvironment _env;

        public BookDetailsModel(BooksRepository repo, IWebHostEnvironment env)
        {
            _repo = repo;
            _env = env;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public BookModel? Book { get; set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));
        public bool IsOwner { get; set; }
        public bool HasRentedThis { get; set; }

        public IActionResult OnGet()
        {
            Book = _repo.GetById(Id);
            if (Book is null)
            {
                TempData["Message"] = "Bogen blev ikke fundet.";
                return RedirectToPage("/AvailableBooks");
            }

            var u = HttpContext.Session.GetString("Username");
            IsOwner = !string.IsNullOrEmpty(u) &&
                      string.Equals(Book.CreatedBy, u, StringComparison.OrdinalIgnoreCase);

            HasRentedThis = !string.IsNullOrEmpty(u) &&
                            Book.RentedByUsers.Any(x => string.Equals(x, u, StringComparison.OrdinalIgnoreCase));

            return Page();
        }

        public IActionResult OnPostRent(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Details", new { id }) });

            var ok = _repo.Rent(id, username);
            TempData["Message"] = ok ? "Bogen er nu lejet." : "Bogen er ikke ledig.";
            return RedirectToPage(new { id });
        }

      
        public IActionResult OnPostRateUser(Guid id, int stars, string? comment)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Details", new { id }) });

            var book = _repo.GetById(id);
            if (book == null || !book.RentedByUsers.Contains(username))
            {
                TempData["Message"] = "Du kan kun vurdere bøger, du har lejet.";
                return RedirectToPage(new { id });
            }

            stars = Math.Min(5, Math.Max(1, stars));

            var existing = book.Ratings.FirstOrDefault(r =>
                r.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
            {
                book.Ratings.Add(new Rating
                {
                    Username = username,
                    Stars = stars,
                    Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Stars = stars;
                existing.Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
                existing.CreatedAt = DateTime.UtcNow;
            }

            _repo.Update(book);
            TempData["Message"] = "Tak for din vurdering!";
            return RedirectToPage(new { id });
        }

        public IActionResult OnPostContactOwner(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Details", new { id }) });

            var book = _repo.GetById(id);
            if (book is null)
            {
                TempData["Message"] = "Bogen blev ikke fundet.";
                return RedirectToPage("/AvailableBooks");
            }

            if (string.Equals(book.CreatedBy, username, StringComparison.OrdinalIgnoreCase))
            {
                TempData["Message"] = "Du er selv ejer af bogen.";
                return RedirectToPage(new { id });
            }

          
            TempData["Message"] = $"En kontaktanmodning til {book.CreatedBy} er registreret.";
            return RedirectToPage(new { id });
        }

     
        public IActionResult OnGetDownload(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Details", new { id }) });

            var book = _repo.GetById(id);
            if (book is null)
            {
                TempData["Message"] = "Bogen blev ikke fundet.";
                return RedirectToPage("/AvailableBooks");
            }

            var allowed = string.Equals(book.CreatedBy, username, StringComparison.OrdinalIgnoreCase) ||
                          book.RentedByUsers.Contains(username, StringComparer.OrdinalIgnoreCase);

            if (!allowed)
            {
                TempData["Message"] = "Du har ikke adgang til at downloade denne bog.";
                return RedirectToPage(new { id });
            }

            if (string.IsNullOrEmpty(book.PdfPath))
            {
                TempData["Message"] = "Der er ikke tilknyttet en PDF til denne bog.";
                return RedirectToPage(new { id });
            }

            var abs = Path.Combine(_env.ContentRootPath, book.PdfPath);
            if (!System.IO.File.Exists(abs))
            {
                TempData["Message"] = "PDF-filen blev ikke fundet.";
                return RedirectToPage(new { id });
            }

            var fileName = $"{(string.IsNullOrWhiteSpace(book.Title) ? "bog" : book.Title)}.pdf";
            var stream = System.IO.File.OpenRead(abs);

            return File(stream, "application/pdf", fileName);
        }
    }
}
