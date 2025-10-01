using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages
{
    ///<summary> Represents the model for the Register page. </summary>
    public class RegisterModel : PageModel
    {
        // Formfelter
        [BindProperty] public string Username { get; set; } = "";
        [BindProperty] public string Password { get; set; } = "";
        [BindProperty] public string Name { get; set; } = "";
        [BindProperty] public string Phone { get; set; } = "";
        [BindProperty] public string Email { get; set; } = "";
        [BindProperty] public int ZipCode { get; set; }
        [BindProperty] public string? Description { get; set; }

        [BindProperty] public bool isadministrator { get; set; } = false;

        public string? Message { get; set; }

        private readonly string loginFilePath;
        private readonly string membersFilePath;

        public RegisterModel(IWebHostEnvironment env)
        {
            loginFilePath = Path.Combine(env.ContentRootPath, "JSON", "login.json");
            membersFilePath = Path.Combine(env.ContentRootPath, "JSON", "members.json");
        }

        public List<Member> Members { get; set; } = new();

        public void OnGet() { }

        public IActionResult OnPost()
        {
            // Simple validering
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Email))
            {
                Message = "Alle felter skal udfyldes.";
                return Page();
            }

            // Sørg for at JSON-mappen findes
            Directory.CreateDirectory(Path.GetDirectoryName(loginFilePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(membersFilePath)!);

            // --- Hent eksisterende brugere (liste) ---
            var users = new List<UserModel>();
            if (System.IO.File.Exists(loginFilePath))
            {
                var json = System.IO.File.ReadAllText(loginFilePath);
                try
                {
                    users = JsonSerializer.Deserialize<List<UserModel>>(json) ?? new();
                }
                catch
                {
                    // hvis den gamle fil er et enkelt objekt, konverter til liste
                    var single = JsonSerializer.Deserialize<UserModel>(json);
                    if (single != null) users.Add(single);
                }
            }

            // Tjek brugernavn unikt
            if (users.Any(u => !string.IsNullOrEmpty(u.Username) &&
                               u.Username.Equals(Username, StringComparison.OrdinalIgnoreCase)))
            {
                Message = "Brugernavn findes allerede.";
                return Page();
            }

            // --- Opret User og GEM ALLE FELTER ---
            var newUser = new UserModel
            {
                Username = Username.Trim(),
                Password = Password, // TODO: hash i produktion
                Email = Email.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                ZipCode = ZipCode,
                CreatedAt = DateTime.UtcNow
            };

            users.Add(newUser);
            System.IO.File.WriteAllText(
                loginFilePath,
                JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true })
            );

            // --- Hent/skriv members.json som før ---
            var members = new List<Member>();
            if (System.IO.File.Exists(membersFilePath))
            {
                var json = System.IO.File.ReadAllText(membersFilePath);
                members = JsonSerializer.Deserialize<List<Member>>(json) ?? new();
            }

            // Vælg tilfældigt avatar
            var avatars = new[]
            {
                "Avatar1.jpg","Avatar2.jpg","Avatar3.jpg","Avatar4.jpg","Avatar5.jpg",
                "Avatar6.jpg","Avatar7.jpg","Avatar8.jpg","Avatar9.jpg","Avatar10.jpg","Avatar11.jpg"
            };
            var selectedImage = avatars[new Random().Next(avatars.Length)];

            var newMember = new Member(Name, Phone, Email, Username, selectedImage, isadministrator, ZipCode, Description ?? "")
            {
                ID = members.Count + 1
            };

            members.Add(newMember);
            System.IO.File.WriteAllText(
                membersFilePath,
                JsonSerializer.Serialize(members, new JsonSerializerOptions { WriteIndented = true })
            );

            TempData["Message"] = "Bruger og medlem oprettet! Log ind nu.";
            return RedirectToPage("/Login");
        }
    }
}
