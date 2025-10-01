using System.Text.Json;
using SystemUdviklingProjekt.Model;
using Microsoft.AspNetCore.Hosting;
using SystemUdviklingProjekt.Service;
using SystemUdviklingProjekt.Pages.Books;




public class BooksRepository
{
    private readonly string _file;
    private readonly object _lock = new();

    public BooksRepository(IWebHostEnvironment env)
    {
        var dir = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _file = Path.Combine(dir, "books.json");
        if (!File.Exists(_file)) File.WriteAllText(_file, "[]");
    }

    private List<BookModel> Read()
    {
        lock (_lock)
        {
            var json = File.ReadAllText(_file);
            return JsonSerializer.Deserialize<List<BookModel>>(json) ?? new();
        }
    }

    private void Write(List<BookModel> data)
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_file, json);
        }
    }

    public List<BookModel> GetAll() => Read();

    public IEnumerable<BookModel> GetByOwner(string username) =>
        Read().Where(b => string.Equals(b.CreatedBy, username, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<BookModel> GetRentedBy(string username) =>
        Read().Where(b => b.RentedByUsers.Any(u => string.Equals(u, username, StringComparison.OrdinalIgnoreCase)));

    public void Add(BookModel book)
    {
        var list = Read();
        list.Add(book);
        Write(list);
    }
    public BookModel? GetById(Guid id) => Read().FirstOrDefault(b => b.Id == id);


    public bool Rent(Guid id, string username)
    {
        var list = Read();
        var b = list.FirstOrDefault(x => x.Id == id);
        if (b is null) return false;
        if (!b.IsAvailable) return false;
        if (b.RentedByUsers.Any(u => string.Equals(u, username, StringComparison.OrdinalIgnoreCase))) return false; // én pr. bruger
        b.RentedByUsers.Add(username);
        Write(list);
        return true;
    }

    public bool Update(BookModel updated)
    {
        var list = Read();                                
        var i = list.FindIndex(b => b.Id == updated.Id);  
        if (i < 0) return false;

        list[i] = updated;                                
        Write(list);                                     
        return true;
    }


    public bool Return(Guid id, string username)
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

    public void EditBook(List<BookModel> books) 
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



    public string DeleteBook(Guid id)
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

