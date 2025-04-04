namespace PhimMoiDaDen.Models
{
    public class MovieCategory
    {
        public int MovieCategoryId { get; set; }
        public int MovieId { get; set; }
        public int CategoryId { get; set; }

        // Navigation properties
        public Movie Movie { get; set; }
        public Category Category { get; set; }
    }
} 