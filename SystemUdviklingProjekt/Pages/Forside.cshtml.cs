using Microsoft.AspNetCore.Mvc.RazorPages;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages;

public class ForsideModel : PageModel
{
    private readonly BooksRepository _repo;
    public int TotalBooks { get; set; }
    public int AvailableBooks { get; set; }
    public List<BookModel> Latest { get; set; } = new();
    public bool IsLoggedIn => !string.IsNullOrEmpty(HttpContext.Session.GetString("Username"));

    public ForsideModel(BooksRepository repo) => _repo = repo;

    public void OnGet()
    {
        var all = _repo.GetAll();
        TotalBooks = all.Count;
        AvailableBooks = all.Count(b => b.IsAvailable);
        Latest = all.OrderByDescending(b => b.CreatedAt).Take(6).ToList();
    }
}
