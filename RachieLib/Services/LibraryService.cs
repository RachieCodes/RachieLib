using RachieLib.Models;

namespace RachieLib.Services {
    public class LibraryService {
        private List<Book> books = new();

        public void AddBook(Book book) => books.Add(book);
        public List<Book> SearchBooks(string query) =>
            books.Where(b => b.Title.Contains(query) || b.Author.Contains(query)).ToList();
        public List<Book> GetAvailableBooks() =>
            books.Where(b => b.IsAvailable).ToList();
        public bool RemoveBook(string isbn) {
            var book = books.FirstOrDefault(b => b.ISBN == isbn);
            if (book != null) {
                books.Remove(book);
                return true;
            }
            return false;
        }
    }
}