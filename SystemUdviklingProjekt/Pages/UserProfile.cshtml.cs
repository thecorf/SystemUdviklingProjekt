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
    public class UserProfileModel : PageModel
    {
        /// <summary>
        /// The repo
        /// </summary>
        private readonly BooksRepository _repo;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileModel"/> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        public UserProfileModel(BooksRepository repo) => _repo = repo;

        /// <summary>
        /// Gets or sets the owned.
        /// </summary>
        /// <value>
        /// The owned.
        /// </value>
        public List<BookModel> Owned { get; set; } = new();
        /// <summary>
        /// Gets or sets the rented.
        /// </summary>
        /// <value>
        /// The rented.
        /// </value>
        public List<BookModel> Rented { get; set; } = new();

        /// <summary>
        /// Called when [get].
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {
            // Check if user is logged in
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/UserProfile") });

            Owned = _repo.GetByOwner(username).ToList();
            Rented = _repo.GetRentedBy(username).ToList();
            return Page();
        }
        /// <summary>
        /// Called when [post return].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IActionResult OnPostReturn(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/UserProfile") });

            var ok = _repo.Return(id, username);
            TempData["Message"] = ok ? "Bogen er afleveret." : "Kunne ikke aflevere bogen.";
            return RedirectToPage(); 
        }

        /// <summary>
        /// Called when [post delete].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IActionResult OnPostDelete(Guid id)
        {
            // Check if user is logged in
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
                return RedirectToPage("/Login", new { returnUrl = Url.Page("/UserProfile") });
            var ok = _repo.DeleteBook(id);
            TempData["Message"] = ok == "OK" ? "Bogen er slettet." : ok;
            return RedirectToPage();
        }

        /// <summary>
        /// Called when [post edit books].
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostEditBooks()
        {
            return RedirectToPage("/Books/ManageBooks");
        }



        /// <summary>
        /// Called when [post edit].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public IActionResult OnPostEdit(Guid id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            return RedirectToPage("/Login", new { returnUrl = Url.Page("/UserProfile") });
            return RedirectToPage("/Books/Edit", new { id = id });

        }

    }
}
