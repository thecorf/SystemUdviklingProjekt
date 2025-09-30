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

        public void OnGet() => Books = _repo.GetAll();

      


        public IActionResult OnPostRent(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/AvailableBooks") });

            var ok = _repo.Rent(id, username);
            TempData["Message"] = ok ? "Bogen er nu lejet." : "Bogen er ikke ledig.";
            return RedirectToPage();

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
    }

}
