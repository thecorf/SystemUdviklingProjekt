using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly BooksRepository _repo;

        public UserProfileModel(BooksRepository repo) => _repo = repo;

        public List<BookModel> Owned { get; set; } = new();
        public List<BookModel> Rented { get; set; } = new();

        public IActionResult OnGet()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/UserProfile") });

            Owned = _repo.GetByOwner(username).ToList();
            Rented = _repo.GetRentedBy(username).ToList();
            return Page();
        }
        public IActionResult OnPostReturn(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/UserProfile") });

            var ok = _repo.Return(id, username);
            TempData["Message"] = ok ? "Bogen er afleveret." : "Kunne ikke aflevere bogen.";
            return RedirectToPage(); // reload profilen
        }
    }
}
