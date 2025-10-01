using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";

        public string? ErrorMessage { get; set; }
        public string? Message { get; set; }

        private readonly string _loginFilePath;

        public LoginModel(IWebHostEnvironment env)
        {
            _loginFilePath = Path.Combine(env.ContentRootPath, "JSON", "login.json");
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Udfyld både brugernavn og adgangskode.";
                return Page();
            }

            var users = new List<UserModel>();
            if (System.IO.File.Exists(_loginFilePath))
            {
                var json = System.IO.File.ReadAllText(_loginFilePath);
                try
                {
                    users = JsonSerializer.Deserialize<List<UserModel>>(json) ?? new();
                }
                catch
                {
                    // fallback hvis filen var et enkelt objekt
                    var single = JsonSerializer.Deserialize<UserModel>(json);
                    if (single != null) users.Add(single);
                }
            }

            var user = users.FirstOrDefault(u =>
                u.Username.Equals(Username, StringComparison.OrdinalIgnoreCase));

            if (user == null || user.Password != Password)
            {
                ErrorMessage = "Ugyldigt brugernavn eller adgangskode.";
                return Page();
            }

            HttpContext.Session.SetString("Username", user.Username);
            Message = "Login lykkedes!";
            return RedirectToPage("/Forside");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}
