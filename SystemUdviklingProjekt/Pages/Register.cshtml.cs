using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages
{
    /// <summary>
    /// Represents the model for the Register page.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
    public class RegisterModel : PageModel
    {
        // Formfelter
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
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [BindProperty] public string Name { get; set; } = "";
        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        [BindProperty] public string Phone { get; set; } = "";
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [BindProperty] public string Email { get; set; } = "";
        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>
        /// The zip code.
        /// </value>
        [BindProperty] public int ZipCode { get; set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [BindProperty] public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RegisterModel"/> is isadministrator.
        /// </summary>
        /// <value>
        ///   <c>true</c> if isadministrator; otherwise, <c>false</c>.
        /// </value>
        [BindProperty] public bool isadministrator { get; set; } = false;

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
        private readonly string loginFilePath;
        /// <summary>
        /// The members file path
        /// </summary>
        private readonly string membersFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterModel"/> class.
        /// </summary>
        /// <param name="env">The env.</param>
        public RegisterModel(IWebHostEnvironment env)
        {
            loginFilePath = Path.Combine(env.ContentRootPath, "JSON", "login.json");
            membersFilePath = Path.Combine(env.ContentRootPath, "JSON", "members.json");
        }

        /// <summary>
        /// Gets or sets the members.
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        public List<Member> Members { get; set; } = new();

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
           
            if (string.IsNullOrWhiteSpace(Username) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Phone) ||
                string.IsNullOrWhiteSpace(Email))
            {
                Message = "Alle felter skal udfyldes.";
                return Page();
            }


            // Ensure the JSON directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(loginFilePath)!);
            Directory.CreateDirectory(Path.GetDirectoryName(membersFilePath)!);

            // Load existing users
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
                  
                    var single = JsonSerializer.Deserialize<UserModel>(json);
                    if (single != null) users.Add(single);
                }
            }

            // Check for duplicate username
            if (users.Any(u => !string.IsNullOrEmpty(u.Username) &&
                               u.Username.Equals(Username, StringComparison.OrdinalIgnoreCase)))
            {
                Message = "Brugernavn findes allerede.";
                return Page();
            }


            // Create and save new user

            var newUser = new UserModel
            {
                Username = Username.Trim(),
                Password = Password, 
                Email = Email.Trim(),
                Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim(),
                ZipCode = ZipCode,
                CreatedAt = DateTime.UtcNow
            };

            // Add to users list and save

            users.Add(newUser);
            System.IO.File.WriteAllText(
                loginFilePath,
                JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true })
            );


            // Load existing members
            var members = new List<Member>();
            if (System.IO.File.Exists(membersFilePath))
            {
                var json = System.IO.File.ReadAllText(membersFilePath);
                members = JsonSerializer.Deserialize<List<Member>>(json) ?? new();
            }

            // Select a random avatar
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

            // Add to members list and save
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
