using Microsoft.AspNetCore.Mvc;
using RachieLib.Models;
using RachieLib.Services;

namespace RachieLib.Controllers {
    public class HomeController : Controller {
        private readonly LibraryService _service;

        public HomeController(LibraryService service) {
            _service = service;
        }

        public IActionResult Index() => View(_service.GetAvailableBooks());

        [HttpGet("/Add")]
        public IActionResult Add() => View();

        [HttpPost("/Add")]
        public IActionResult Add(Book book) {
            _service.AddBook(book);
            return RedirectToAction("Index");
        }

        [HttpGet("/Search")]
        public IActionResult Search() => View();

        [HttpPost("/Search")]
        public IActionResult Search(string query) {
            var results = _service.SearchBooks(query);
            return View("Index", results);
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