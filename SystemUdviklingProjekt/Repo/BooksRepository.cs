using System.Text.Json;
using SystemUdviklingProjekt.Model;
using System.Text.Json;
using SystemUdviklingProjekt.Model;
using Microsoft.AspNetCore.Hosting;




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

        public void Add(BookModel book)
        {
            var list = Read();
            list.Add(book);
            Write(list);
        }

        public bool Rent(Guid id, string username)
        {
            var list = Read();
            var b = list.FirstOrDefault(x => x.Id == id);
            if (b is null || !b.IsAvailable) return false;
            b.RentedBy = username;
            Write(list);
            return true;
        }
    }

