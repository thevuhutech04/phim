using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhimMoiDaDen.Data;
using PhimMoiDaDen.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PhimMoiDaDen.Controllers
{
    public class MovieController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Movie
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
                
            return View("List", movies);
        }

        // GET: Movie/All
        public async Task<IActionResult> All()
        {
            var movies = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .OrderBy(m => m.Title)
                .ToListAsync();
                
            ViewData["Title"] = "Tất cả phim";
            return View("List", movies);
        }

        // GET: Movie/Search
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction(nameof(All));
            }

            var movies = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Where(m => m.Title.Contains(query) || 
                           m.Description.Contains(query) || 
                           m.Director.Contains(query) || 
                           m.Cast.Contains(query))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
                
            ViewData["Title"] = $"Kết quả tìm kiếm: {query}";
            ViewData["SearchQuery"] = query;
            return View("List", movies);
        }

        // GET: Movie/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            // Tăng lượt xem
            movie.Views++;
            await _context.SaveChangesAsync();

            return View("Index", movie);
        }

        // GET: Movie/Watch/5
        public async Task<IActionResult> Watch(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .Include(m => m.Comments)
                    .ThenInclude(c => c.User)
                .Include(m => m.Favorites)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movie/AddComment
        [HttpPost]
        public async Task<IActionResult> AddComment(int movieId, string content, bool isAnonymous)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var comment = new Comment
            {
                MovieId = movieId,
                UserId = userId,
                Content = content,
                IsAnonymous = isAnonymous,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Lấy URL của trang trước đó
            string returnUrl = Request.Headers["Referer"].ToString();
            
            // Kiểm tra URL có phải từ trang Watch không
            if (returnUrl.Contains("/Movie/Watch/"))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Details), new { id = movieId });
        }

        // POST: Movie/ToggleFavorite
        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int movieId)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = HttpContext.Session.GetInt32("UserId").Value;

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.MovieId == movieId && f.UserId == userId);

            if (favorite == null)
            {
                // Add to favorites
                favorite = new Favorite
                {
                    MovieId = movieId,
                    UserId = userId,
                    AddedAt = DateTime.Now
                };
                _context.Favorites.Add(favorite);
            }
            else
            {
                // Remove from favorites
                _context.Favorites.Remove(favorite);
            }

            await _context.SaveChangesAsync();

            // Lấy URL của trang trước đó
            string returnUrl = Request.Headers["Referer"].ToString();
            
            // Quay lại trang trước đó (có thể là Watch hoặc Details)
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction(nameof(Details), new { id = movieId });
        }
    }
} 