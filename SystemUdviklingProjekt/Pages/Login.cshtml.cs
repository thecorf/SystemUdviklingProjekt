using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
    public class LoginModel : PageModel
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [BindProperty] public string Username { get; set; } = "";
        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [BindProperty] public string Password { get; set; } = "";

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string? ErrorMessage { get; set; }
        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string? Message { get; set; }

        /// <summary>
        /// The login file path
        /// </summary>
        private readonly string _loginFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginModel"/> class.
        /// </summary>
        /// <param name="env">The env.</param>
        public LoginModel(IWebHostEnvironment env)
        {
            _loginFilePath = Path.Combine(env.ContentRootPath, "JSON", "login.json");
        }

        /// <summary>
        /// Called when [get].
        /// </summary>
        public void OnGet() { }

        /// <summary>
        /// Called when [post].
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPost()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Udfyld både brugernavn og adgangskode.";
                return Page();
            }


            // Read users from JSON file
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
                  
                    var single = JsonSerializer.Deserialize<UserModel>(json);
                    if (single != null) users.Add(single);
                }
            }

            // Validate user
            var user = users.FirstOrDefault(u =>
                u.Username.Equals(Username, StringComparison.OrdinalIgnoreCase));

            if (user == null || user.Password != Password)
            {
                ErrorMessage = "Ugyldigt brugernavn eller adgangskode.";
                return Page();
            }

            // Set session
            HttpContext.Session.SetString("Username", user.Username);
            Message = "Login lykkedes!";
            return RedirectToPage("/Forside");
        }

        /// <summary>
        /// Called when [post logout].
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("/Login");
        }
    }
}
