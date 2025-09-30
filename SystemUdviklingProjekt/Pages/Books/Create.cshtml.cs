using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Model;
using System.ComponentModel.DataAnnotations;
using SystemUdviklingProjekt.Service;


namespace SystemUdviklingProjekt.Pages.Books
{


    

    public class CreateBookModel : PageModel
    {
        private readonly BooksRepository _repo;

        public CreateBookModel(BooksRepository repo) => _repo = repo;

        [BindProperty] public BookInput Input { get; set; } = new();

        public IActionResult OnGet()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Create") });
            return Page();
        }

        public IActionResult OnPost()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/Books/Create") });

            if (!ModelState.IsValid) return Page();

            var book = new BookModel
            {
                Title = Input.Title.Trim(),
                Author = Input.Author?.Trim() ?? "",
                Description = Input.Description,
                ImageUrl = Input.ImageUrl,
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

            [Display(Name = "Beskrivelse")]
            public string? Description { get; set; }

            [Url, Display(Name = "Billede-URL")]
            public string? ImageUrl { get; set; }
        }
    }
}
