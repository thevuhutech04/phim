using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhimMoiDaDen.Data;
using PhimMoiDaDen.Models;
using System.Diagnostics;

namespace PhimMoiDaDen.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // If there are no movies, create some sample data
            if (!movies.Any())
            {
                // Create sample categories
                var categories = new List<Category>
                {
                    new Category { Name = "Action", Description = "Action movies", CreatedAt = DateTime.Now },
                    new Category { Name = "Comedy", Description = "Comedy movies", CreatedAt = DateTime.Now },
                    new Category { Name = "Drama", Description = "Drama movies", CreatedAt = DateTime.Now }
                };
                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();

                // Create sample movies
                var sampleMovies = new List<Movie>
                {
                    new Movie
                    {
                        Title = "Doraemon: Nobita's Little Star Wars",
                        Description = "Doraemon và những người bạn bắt đầu cuộc phiêu lưu mới trong không gian",
                        Genre = "Hoạt hình",
                        PosterUrl = "https://via.placeholder.com/300x450",
                        VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        BackdropUrl = "https://via.placeholder.com/1920x1080",
                        Views = 100,
                        CreatedAt = DateTime.Now,
                        ReleaseYear = 2021,
                        Director = "Yamaguchi Susumu",
                        Cast = "Doraemon, Nobita, Shizuka",
                        Rating = 4.5f,
                        Duration = 108,
                        Language = "Tiếng Nhật",
                        Country = "Nhật Bản",
                        TrailerUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        Quality = "HD"
                    },
                    new Movie
                    {
                        Title = "Demon Slayer: Mugen Train",
                        Description = "Tanjiro và nhóm Diệt Quỷ tham gia nhiệm vụ điều tra trên chuyến tàu Vô Tận",
                        Genre = "Hoạt hình",
                        PosterUrl = "https://via.placeholder.com/300x450",
                        VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        BackdropUrl = "https://via.placeholder.com/1920x1080",
                        Views = 150,
                        CreatedAt = DateTime.Now.AddDays(-1),
                        ReleaseYear = 2020,
                        Director = "Sotozaki Haruo",
                        Cast = "Hanae Natsuki, Kitou Akari",
                        Rating = 4.8f,
                        Duration = 117,
                        Language = "Tiếng Nhật",
                        Country = "Nhật Bản",
                        TrailerUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        Quality = "FullHD"
                    },
                    new Movie
                    {
                        Title = "Spider-Man: No Way Home",
                        Description = "Peter Parker phải đối mặt với những hậu quả khi danh tính của anh bị tiết lộ",
                        Genre = "Hành động",
                        PosterUrl = "https://via.placeholder.com/300x450",
                        VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        BackdropUrl = "https://via.placeholder.com/1920x1080",
                        Views = 200,
                        CreatedAt = DateTime.Now.AddDays(-2),
                        ReleaseYear = 2021,
                        Director = "Jon Watts",
                        Cast = "Tom Holland, Zendaya",
                        Rating = 4.7f,
                        Duration = 148,
                        Language = "Tiếng Anh",
                        Country = "Mỹ",
                        TrailerUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        Quality = "4K"
                    },
                    new Movie
                    {
                        Title = "Avengers: Endgame",
                        Description = "Các siêu anh hùng còn sống sót sau cú búng tay của Thanos tập hợp lại để đảo ngược tình thế",
                        Genre = "Hành động",
                        PosterUrl = "https://via.placeholder.com/300x450",
                        VideoUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        BackdropUrl = "https://via.placeholder.com/1920x1080",
                        Views = 250,
                        CreatedAt = DateTime.Now.AddDays(-3),
                        ReleaseYear = 2019,
                        Director = "Anthony Russo, Joe Russo",
                        Cast = "Robert Downey Jr., Chris Evans",
                        Rating = 4.9f,
                        Duration = 181,
                        Language = "Tiếng Anh",
                        Country = "Mỹ",
                        TrailerUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ",
                        Quality = "4K"
                    }
                };

                _context.Movies.AddRange(sampleMovies);
                await _context.SaveChangesAsync();

                // Create movie categories relationships
                var movieCategories = new List<MovieCategory>();
                foreach (var movie in sampleMovies)
                {
                    movieCategories.Add(new MovieCategory
                    {
                        MovieId = movie.MovieId,
                        CategoryId = categories[0].CategoryId // Action category
                    });
                }

                _context.MovieCategories.AddRange(movieCategories);
                await _context.SaveChangesAsync();

                // Reload movies with their relationships
                movies = await _context.Movies
                    .Include(m => m.MovieCategories)
                        .ThenInclude(mc => mc.Category)
                    .OrderByDescending(m => m.CreatedAt)
                    .ToListAsync();
            }

            return View(movies);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
