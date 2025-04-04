using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhimMoiDaDen.Data;
using PhimMoiDaDen.Models;
using System.Linq;
using System.Threading.Tasks;

namespace PhimMoiDaDen.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
                
            return View(categories);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories
                .Include(c => c.MovieCategories)
                    .ThenInclude(mc => mc.Movie)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
} 