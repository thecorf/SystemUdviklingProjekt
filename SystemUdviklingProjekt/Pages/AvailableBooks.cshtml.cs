using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Model;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Repo;

namespace SystemUdviklingProjekt.Pages
{


    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
    public class AvailableBooksModel : PageModel
    {
        /// <summary>
        /// The repo
        /// </summary>
        private readonly BooksRepository _repo;

        /// <summary>
        /// Gets or sets the books.
        /// </summary>
        /// <value>
        /// The books.
        /// </value>
        public List<BookModel> Books { get; set; } = new();
        /// <summary>
        /// Gets a value indicating whether this instance is logged in.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is logged in; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

        /// <summary>
        /// Initializes a new instance of the <see cref="AvailableBooksModel"/> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        public AvailableBooksModel(BooksRepository repo) => _repo = repo;

        /// <summary>
        /// Called when [get].
        /// </summary>
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


        /// <summary>
        /// Gets or sets the genre.
        /// </summary>
        /// <value>
        /// The genre.
        /// </value>
        [BindProperty(SupportsGet = true)] public string? Genre { get; set; }
        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        /// <value>
        /// The author.
        /// </value>
        [BindProperty(SupportsGet = true)] public string? Author { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [BindProperty(SupportsGet = true)] public string? Title { get; set; }
        /// <summary>
        /// Gets or sets the year.
        /// </summary>
        /// <value>
        /// The year.
        /// </value>
        [BindProperty(SupportsGet = true)] public int? Year { get; set; }

        /// <summary>
        /// Gets or sets the genres.
        /// </summary>
        /// <value>
        /// The genres.
        /// </value>
        public List<string> Genres { get; set; } = new();
        /// <summary>
        /// Called when [post rent].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Updates the number of books after rent.
        /// </summary>
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


       
        /// <summary>
        /// Gets or sets a value indicating whether [only title matches].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [only title matches]; otherwise, <c>false</c>.
        /// </value>
        [BindProperty(SupportsGet = true)] public bool OnlyTitleMatches { get; set; }

        /// <summary>
        /// Filters the books.
        /// </summary>
        /// <param name="genre">The genre.</param>
        /// <param name="author">The author.</param>
        /// <param name="title">The title.</param>
        /// <param name="year">The year.</param>
        /// <param name="onlyTitleMatches">if set to <c>true</c> [only title matches].</param>
        /// 
        /// <returns></returns>
        public void FilterBooks(string? genre, string? author, string? title, int? year, bool onlyTitleMatches)
        {
            var q = _repo.GetAll().AsEnumerable();

      
            q = q.Where(b => b.IsAvailable);

            if (!string.IsNullOrWhiteSpace(genre))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Genre) &&
                                 b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(author))
                q = q.Where(b => !string.IsNullOrWhiteSpace(b.Author) &&
                                 b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));

            if (year.HasValue && year.Value > 0)
                q = q.Where(b => b.Year == year.Value);

            // Handle title filtering and ordering
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
               
                q = q.OrderBy(b => b.Title);
            }

            Books = q.ToList();
        }
        /// <summary>
        /// Applies the filters.
        /// </summary>
        private void ApplyFilters()
        {
            var q = _repo.GetAll().AsEnumerable();

            // only show available books
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

