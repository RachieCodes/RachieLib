namespace RachieLib.Models {
    public class Book {
        public required string Title { get; set; }
        public required string Author { get; set; }
        public required string ISBN { get; set; }
        public bool IsAvailable { get; set; } = true;

        // Optional: URL to the cover image
        public string? CoverImageUrl { get; set; }

        // Optional: Hex or CSS color for the spine
        public string? SpineColor { get; set; }
    }
}