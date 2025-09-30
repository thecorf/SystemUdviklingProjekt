using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using SystemUdviklingProjekt.Model;

namespace SystemUdviklingProjekt.Pages
{
    /// <summary>
    /// Represents the Razor Page model for managing members.
    /// </summary>
    public class MembersModel : PageModel
    {
        /// <summary>
        /// Gets or sets the list of members.
        /// </summary>
        public List<Member> Members { get; set; } = new();

        private readonly string membersFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="MembersModel"/> class.
        /// </summary>
        /// <param name="env">The hosting environment to determine the content root path.</param>
        public MembersModel(IWebHostEnvironment env)
        {
            membersFilePath = Path.Combine(env.ContentRootPath, "JSON", "members.json");
        }

        /// <summary>
        /// Handles GET requests to load the members data.
        /// Redirects to the login page if the user is not logged in.
        /// Deserialize the JSON data into the Members list
        /// Logs any errors during deserialization
        /// </summary>
        public void OnGet()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                // Redirect to login page if not logged in
                Response.Redirect("/Forside");
            }

            if (System.IO.File.Exists(membersFilePath))
            {
                var json = System.IO.File.ReadAllText(membersFilePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    try
                    {
                        Members = JsonSerializer.Deserialize<List<Member>>(json) ?? new();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error loading members.json: " + ex.Message);
                        Members = new();
                    }
                }
                else
                {
                    Members = new();
                }
            }
            else
            {
                Members = new();
            }
        }
    }
}
