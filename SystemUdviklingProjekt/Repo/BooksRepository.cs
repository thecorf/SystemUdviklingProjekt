using System.Text.Json;
using SystemUdviklingProjekt.Model;
using Microsoft.AspNetCore.Hosting;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Pages.Books;




/// <summary>
/// 
/// </summary>
public class BooksRepository
{
    /// <summary>
    /// The file
    /// </summary>
    private readonly string _file;
    /// <summary>
    /// The lock
    /// </summary>
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BooksRepository"/> class.
    /// </summary>
    /// <param name="env">The env.</param>
    public BooksRepository(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _file = Path.Combine(dir, "books.json");
        if (!File.Exists(_file)) File.WriteAllText(_file, "[]");
    }

    /// <summary>
    /// Reads this instance.
    /// </summary>
    /// <returns></returns>
    private List<BookModel> Read()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_file);
            return JsonSerializer.Deserialize<List<BookModel>>(json) ?? new();
        }
    }

    /// <summary>
    /// Writes the specified data.
    /// </summary>
    /// <param name="data">The data.</param>
    private void Write(List<BookModel> data)
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_file, json);
        }
    }

    /// <summary>
    /// Gets all.
    /// </summary>
    /// <returns></returns>
    public List<BookModel> GetAll() => Read();

    /// <summary>
    /// Gets the by owner.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    public IEnumerable<BookModel> GetByOwner(string username) =>
        Read().Where(b => string.Equals(b.CreatedBy, username, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Gets the rented by.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    public IEnumerable<BookModel> GetRentedBy(string username) =>
        Read().Where(b => b.RentedByUsers.Any(u => string.Equals(u, username, StringComparison.OrdinalIgnoreCase)));

    /// <summary>
    /// Adds the specified book.
    /// </summary>
    /// <param name="book">The book.</param>
    public void Add(BookModel book)
    {
        var list = Read();
        list.Add(book);
        Write(list);
    }
    /// <summary>
    /// Gets the by identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public BookModel? GetById(Guid id) => Read().FirstOrDefault(b => b.Id == id);


    /// <summary>
    /// Rents the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    public bool Rent(Guid id, string username) // Rent a book
    {
        var list = Read();
        var b = list.FirstOrDefault(x => x.Id == id);
        if (b is null) return false;
        if (!b.IsAvailable) return false;
        if (b.RentedByUsers.Any(u => string.Equals(u, username, StringComparison.OrdinalIgnoreCase))) return false; 
        b.RentedByUsers.Add(username);
        Write(list);
        return true;
    }

    /// <summary>
    /// Updates the specified updated.
    /// </summary>
    /// <param name="updated">The updated.</param>
    /// <returns></returns>
    public bool Update(BookModel updated) // Update book details
    {
        var list = Read();                                
        var i = list.FindIndex(b => b.Id == updated.Id);  
        if (i < 0) return false;

        list[i] = updated;                                
        Write(list);                                     
        return true;
    }


    /// <summary>
    /// Returns the specified identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="username">The username.</param>
    /// <returns></returns>
    public bool Return(Guid id, string username) // Return a rented book
    {
        var list = Read();
        var b = list.FirstOrDefault(x => x.Id == id);
        if (b is null) return false;

        var idx = b.RentedByUsers.FindIndex(u =>
            string.Equals(u, username, StringComparison.OrdinalIgnoreCase));

        if (idx < 0) return false; 

        b.RentedByUsers.RemoveAt(idx);  
        Write(list);
        return true;
    }

    /// <summary>
    /// Edits the book.
    /// </summary>
    /// <param name="books">The books.</param>
    public void EditBook(List<BookModel> books) // Edit multiple books at once
    { 
        var list = Read();
        foreach (var book in books)
        {
            var existingBook = list.FirstOrDefault(b => b.Id == book.Id);
            if (existingBook != null)
            {
                existingBook.Title = book.Title;
                existingBook.Author = book.Author;
                existingBook.Description = book.Description;
                existingBook.ImageUrl = book.ImageUrl;
                existingBook.NumberOfBooks = book.NumberOfBooks;
                existingBook.Genre = book.Genre;
                existingBook.Year = book.Year;
                existingBook.ImagePath = book.ImagePath;
            }
        }
        Write(list);
    }



    /// <summary>
    /// Deletes the book.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns></returns>
    public string DeleteBook(Guid id) // Delete a book by its ID
    {
        var list = Read();
        var bookToRemove = list.FirstOrDefault(b => b.Id == id);
        if (bookToRemove != null)
        {
            list.Remove(bookToRemove);
            Write(list);
            return "Bogen er slettet.";
        }
        return "Kunne ikke slette bogen.";
    }
}

