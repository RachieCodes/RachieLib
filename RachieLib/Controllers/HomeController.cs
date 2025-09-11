using Microsoft.AspNetCore.Mvc;
using RachieLib.Models;
using RachieLib.Services;

namespace RachieLib.Controllers {
    public class SearchRequest
    {
        public string Query { get; set; } = "";
    }

    public class HomeController : Controller {
        private readonly LibraryService _service;

        public HomeController(LibraryService service) {
            _service = service;
        }

        public IActionResult Index() => View(_service.GetAvailableBooks());

        [HttpGet("/book/{isbn}")]
        public IActionResult Details(string isbn)
        {
            var book = _service.GetBookByISBN(isbn);
            if (book == null)
                return NotFound();
            return View(book);
        }

        [HttpGet("/Add")]
        public IActionResult Add() => View();

        [HttpPost("/Add")]
        public async Task<IActionResult> Add(string title, string isbn = "")
        {
            if (string.IsNullOrEmpty(isbn))
            {
                // If no ISBN is provided, use the first search result
                var searchResults = await _service.SearchBooks(title);
                var bookDetails = searchResults.FirstOrDefault();
                
                if (bookDetails != null)
                {
                    _service.AddBook(bookDetails.ToBook());
                }
                else
                {
                    // If no results found, create a basic book entry
                    _service.AddBook(new Book {
                        Title = title,
                        Author = "Unknown",
                        ISBN = Guid.NewGuid().ToString("N").Substring(0, 13),
                        DateAdded = DateTime.Now,
                        IsAvailable = true
                    });
                }
            }
            else
            {
                // If ISBN is provided, search for that specific book
                var searchResults = await _service.SearchBooks(title);
                var selectedBook = searchResults.FirstOrDefault(b => b.ISBN == isbn)?.ToBook();
                
                if (selectedBook != null)
                {
                    _service.AddBook(selectedBook);
                }
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost("/Search/Books")]
        public async Task<IActionResult> SearchBooks([FromBody] SearchRequest request)
        {
            var results = await _service.SearchBooks(request.Query);
            return Json(results);
        }

        [HttpGet("/Remove")]
        public IActionResult Remove() => View();

        [HttpPost("/Remove")]
        public IActionResult Remove(string isbn) {
            _service.RemoveBook(isbn);
            return RedirectToAction("Index");
        }
    }
}