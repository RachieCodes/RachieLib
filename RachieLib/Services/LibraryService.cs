using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RachieLib.Models;

namespace RachieLib.Services {
    public class LibraryService {
        private readonly List<Book> books = new();
        private readonly HttpClient _httpClient;
        private const string GoogleBooksApiUrl = "https://www.googleapis.com/books/v1/volumes";

        public LibraryService(HttpClient? httpClient = null) {
            _httpClient = httpClient ?? new HttpClient();
        }

        private class GoogleBooksResponse
        {
            [JsonPropertyName("items")]
            public List<VolumeItem>? Items { get; set; }
        }

        private class VolumeItem
        {
            [JsonPropertyName("volumeInfo")]
            public VolumeInfo VolumeInfo { get; set; } = new();
        }

        private class VolumeInfo
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("authors")]
            public List<string>? Authors { get; set; }

            [JsonPropertyName("description")]
            public string? Description { get; set; }

            [JsonPropertyName("industryIdentifiers")]
            public List<IndustryIdentifier>? IndustryIdentifiers { get; set; }

            [JsonPropertyName("imageLinks")]
            public ImageLinks? ImageLinks { get; set; }
        }

        private class ImageLinks
        {
            [JsonPropertyName("thumbnail")]
            public string? Thumbnail { get; set; }
        }

        private class IndustryIdentifier
        {
            [JsonPropertyName("type")]
            public string Type { get; set; } = "";

            [JsonPropertyName("identifier")]
            public string Identifier { get; set; } = "";
        }

        public class BookSearchResult
        {
            public string Title { get; set; } = "";
            public string Author { get; set; } = "";
            public string ISBN { get; set; } = "";
            public string Summary { get; set; } = "";
            public string ThumbnailUrl { get; set; } = "";

            public Book ToBook()
            {
                return new Book
                {
                    Title = Title,
                    Author = Author,
                    ISBN = ISBN,
                    Summary = Summary,
                    DateAdded = DateTime.Now,
                    IsAvailable = true,
                    CoverImageUrl = ThumbnailUrl
                };
            }
        }

        public void AddBook(Book book)
        {
            if (!books.Any(b => b.ISBN == book.ISBN))
            {
                // Ensure required properties are set
                if (string.IsNullOrEmpty(book.ISBN))
                {
                    book.ISBN = Guid.NewGuid().ToString("N").Substring(0, 13);
                }
                if (string.IsNullOrEmpty(book.Author))
                {
                    book.Author = "Unknown";
                }
                book.DateAdded = DateTime.Now;
                book.IsAvailable = true;
                books.Add(book);
            }
        }

        public async Task<List<BookSearchResult>> SearchBooks(string query)
        {
            try {
                var response = await _httpClient.GetFromJsonAsync<GoogleBooksResponse>($"{GoogleBooksApiUrl}?q={Uri.EscapeDataString(query)}&maxResults=5");
                if (response?.Items == null) return new List<BookSearchResult>();

                return response.Items
                    .Select(item => {
                        var volume = item.VolumeInfo;
                        var industryIdentifiers = volume.IndustryIdentifiers ?? new List<IndustryIdentifier>();
                        var isbn = industryIdentifiers.FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier
                               ?? industryIdentifiers.FirstOrDefault(i => i.Type == "ISBN_10")?.Identifier
                               ?? "";

                        return new BookSearchResult {
                            Title = volume.Title ?? "",
                            Author = string.Join(", ", volume.Authors ?? new List<string>()),
                            ISBN = isbn,
                            Summary = volume.Description ?? "",
                            ThumbnailUrl = volume.ImageLinks?.Thumbnail ?? ""
                        };
                    })
                    .Where(result => !string.IsNullOrWhiteSpace(result.Title))
                    .ToList();
            }
            catch (Exception) {
                return new List<BookSearchResult>();
            }
        }

        public async Task<Book?> FetchBookDetailsByTitle(string title)
        {
            try {
                var response = await _httpClient.GetFromJsonAsync<GoogleBooksResponse>($"{GoogleBooksApiUrl}?q={Uri.EscapeDataString(title)}");
                if (response?.Items == null || !response.Items.Any()) return null;

                var volume = response.Items[0].VolumeInfo;
                var industryIdentifiers = volume.IndustryIdentifiers ?? new List<IndustryIdentifier>();
                var isbn = industryIdentifiers.FirstOrDefault(i => i.Type == "ISBN_13")?.Identifier
                       ?? industryIdentifiers.FirstOrDefault(i => i.Type == "ISBN_10")?.Identifier
                       ?? "";

                return new Book {
                    Title = volume.Title ?? title,
                    Author = string.Join(", ", volume.Authors ?? new List<string>()),
                    ISBN = isbn,
                    Summary = volume.Description ?? "",
                    DateAdded = DateTime.Now,
                    IsAvailable = true
                };
            }
            catch (Exception) {
                return null;
            }
        }

        public async Task<Book> AddBookWithDetails(string title)
        {
            var bookDetails = await FetchBookDetailsByTitle(title);
            if (bookDetails == null)
            {
                bookDetails = new Book {
                    Title = title,
                    Author = "Unknown",
                    DateAdded = DateTime.Now,
                    IsAvailable = true,
                    ISBN = Guid.NewGuid().ToString("N").Substring(0, 13) // Fallback ISBN
                };
            }
            AddBook(bookDetails);
            return bookDetails;
        }

        public Book? GetBookByISBN(string isbn) => 
            books.FirstOrDefault(b => b.ISBN == isbn);

        public List<Book> SearchLocalBooks(string query) =>
            books.Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || 
                           b.Author.Contains(query, StringComparison.OrdinalIgnoreCase))
                 .OrderBy(b => b.Title)
                 .ToList();
                           
        public List<Book> GetAvailableBooks() =>
            books.OrderBy(b => b.Title).ToList();

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