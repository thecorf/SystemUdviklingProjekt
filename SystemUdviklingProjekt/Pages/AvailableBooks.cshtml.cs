using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Repo;

namespace SystemUdviklingProjekt.Pages
{


    public class AvailableBooksModel : PageModel
    {
        private readonly BooksRepository _repo;

        public List<BookModel> Books { get; set; } = new();
        public bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        public AvailableBooksModel(BooksRepository repo) => _repo = repo;

        public void OnGet()
        {
            Genres = _repo.GetAll()
                          .Select(b => b.Genre)
                          .Where(g => !string.IsNullOrWhiteSpace(g))
                          .Distinct(StringComparer.OrdinalIgnoreCase)
                          .OrderBy(g => g)
                          .ToList();

            ApplyFilters();
        }


        [BindProperty(SupportsGet = true)] public string? Genre { get; set; }
        [BindProperty(SupportsGet = true)] public string? Author { get; set; }
        [BindProperty(SupportsGet = true)] public string? Title { get; set; }
        [BindProperty(SupportsGet = true)] public int? Year { get; set; }

        public List<string> Genres { get; set; } = new();
        public IActionResult OnPostRent(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/AvailableBooks", new { Title, Author, Genre, Year }) });

            var ok = _repo.Rent(id, username);
            TempData["Message"] = ok ? "Bogen er nu lejet." : "Bogen er ikke ledig.";
            // behold filteret i URL’en efter POST
            return RedirectToPage(new { Title, Author, Genre, Year });

        }
        public void UpdateNumberOfBooksAfterRent() // Call this method after a book is rented
        {
            var books = _repo.GetAll();
            foreach (var book in books)
            {
                if (book.NumberOfBooks > 0)
                {
                    book.NumberOfBooks--;
                }
            }
            // Save the updated book list back to the repository
            foreach (var book in books)
            {
                _repo.Update(book); // Assuming Add method updates if the book already exists
            }

        }


        // NY: hvis true => hårdt filter på titel, ellers blot sortér så matches ligger øverst
        [BindProperty(SupportsGet = true)] public bool OnlyTitleMatches { get; set; }

        public void FilterBooks(string? genre, string? author, string? title, int? year, bool onlyTitleMatches)
        {
            var q = _repo.GetAll().AsEnumerable();

            // (Behold evt. kun ledige her)
            q = q.Where(b => b.IsAvailable);

            if (!string.IsNullOrWhiteSpace(genre))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Genre) &&
                                 b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(author))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Author) &&
                                 b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

            if (year.HasValue && year.Value > 0)
                q = q.Where(b => b.Year == year.Value);

            // Titel: enten hårdt filter ELLER blot sortering med matches først
            if (!string.IsNullOrWhiteSpace(title))
            {
                if (onlyTitleMatches)
                {
                    q = q.Where(b => !string.IsNullOrWhiteSpace(b.Title) &&
                                     b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
                }
                else
                {
                    // bubble-up matches: true > false
                    q = q.OrderByDescending(b => (!string.IsNullOrWhiteSpace(b.Title) &&
                                                  b.Title.Contains(title, StringComparison.OrdinalIgnoreCase)))
                         .ThenBy(b => b.Title);
                }
            }
            else
            {
                // normal sortering når der ikke er titelfilter
                q = q.OrderBy(b => b.Title);
            }

            Books = q.ToList();
        }
        private void ApplyFilters()
        {
            var q = _repo.GetAll().AsEnumerable();

            // siden viser “Ledige bøger”, så filtrér til ledige
            q = q.Where(b => b.IsAvailable);

            if (!string.IsNullOrWhiteSpace(Genre))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Genre) &&
                                 b.Genre.Equals(Genre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Author))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Author) &&
                                 b.Author.Contains(Author, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(Title))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Title) &&
                                 b.Title.Contains(Title, StringComparison.OrdinalIgnoreCase));

            if (Year.HasValue && Year.Value > 0)
                q = q.Where(b => b.Year == Year.Value);

            Books = q.OrderBy(b => b.Title).ToList();
        }
    
    
    }
}

