using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages;

/// <summary>
/// 
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
public class ForsideModel : PageModel
{
    /// <summary>
    /// The repo
    /// </summary>
    private readonly BooksRepository _repo;
    /// <summary>
    /// Gets or sets the total books.
    /// </summary>
    /// <value>
    /// The total books.
    /// </value>
    public int TotalBooks { get; set; }
    /// <summary>
    /// Gets or sets the available books.
    /// </summary>
    /// <value>
    /// The available books.
    /// </value>
    public int AvailableBooks { get; set; }
    /// <summary>
    /// Gets or sets the latest.
    /// </summary>
    /// <value>
    /// The latest.
    /// </value>
    public List<BookModel> Latest { get; set; } = new();
    /// <summary>
    /// Gets a value indicating whether this instance is logged in.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is logged in; otherwise, <c>false</c>.
    /// </value>
    public bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    /// <summary>
    /// Initializes a new instance of the <see cref="ForsideModel"/> class.
    /// </summary>
    /// <param name="repo">The repo.</param>
    public ForsideModel(BooksRepository repo) => _repo = repo;

    /// <summary>
    /// Called when [get].
    /// </summary>
    public void OnGet() // Note: No need to return IActionResult, void is fine for OnGet
    {
        var all = _repo.GetAll();
        TotalBooks = all.Count;
        AvailableBooks = all.Count(b => b.IsAvailable);
        Latest = all.OrderByDescending(b => b.CreatedAt).Take(6).ToList();
    }
}
