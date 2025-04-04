using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PhimMoiDaDen.Data;
using PhimMoiDaDen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhimMoiDaDen.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }

        #region Movie Management

        public async Task<IActionResult> Movies()
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var movies = await _context.Movies
                .Include(m => m.MovieCategories)
                    .ThenInclude(mc => mc.Category)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return View(movies);
        }

        public async Task<IActionResult> CreateMovie()
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            await PrepareMovieFormData();
            return View("MovieForm", new Movie());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateMovie(Movie movie, int[] selectedCategories)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            // Bỏ qua validation cho các collection properties
            ModelState.Remove("MovieCategories");
            ModelState.Remove("Comments");
            ModelState.Remove("Favorites");

            // Đặt CreatedAt và Views nếu cần
            if (movie.CreatedAt == default)
            {
                movie.CreatedAt = DateTime.Now;
            }
            movie.Views = 0;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(movie);
                    await _context.SaveChangesAsync();

                    if (selectedCategories != null && selectedCategories.Length > 0)
                    {
                        foreach (var categoryId in selectedCategories)
                        {
                            _context.MovieCategories.Add(new MovieCategory
                            {
                                MovieId = movie.MovieId,
                                CategoryId = categoryId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Đã thêm phim thành công!";
                    return RedirectToAction(nameof(Movies));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");
            }

            await PrepareMovieFormData(selectedCategories);
            return View("MovieForm", movie);
        }

        public async Task<IActionResult> EditMovie(int id)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var movie = await _context.Movies
                .Include(m => m.MovieCategories)
                .FirstOrDefaultAsync(m => m.MovieId == id);

            if (movie == null)
            {
                return NotFound();
            }

            var selectedCategories = movie.MovieCategories.Select(mc => mc.CategoryId).ToArray();
            await PrepareMovieFormData(selectedCategories);

            return View("MovieForm", movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMovie(int id, Movie movie, int[] selectedCategories)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            if (id != movie.MovieId)
            {
                return NotFound();
            }

            // Bỏ qua validation cho các collection properties
            ModelState.Remove("MovieCategories");
            ModelState.Remove("Comments");
            ModelState.Remove("Favorites");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bản ghi hiện tại để giữ CreatedAt và Views không bị thay đổi
                    if (movie.CreatedAt == default)
                    {
                        var existingMovie = await _context.Movies
                            .AsNoTracking()
                            .FirstOrDefaultAsync(m => m.MovieId == id);
                        
                        if (existingMovie == null)
                        {
                            ModelState.AddModelError("", "Không tìm thấy phim cần sửa!");
                            await PrepareMovieFormData(selectedCategories);
                            return View("MovieForm", movie);
                        }
                        
                        movie.CreatedAt = existingMovie.CreatedAt;
                        movie.Views = existingMovie.Views;
                    }

                    _context.Update(movie);
                    await _context.SaveChangesAsync();

                    // Cập nhật danh mục
                    var existingCategories = await _context.MovieCategories
                        .Where(mc => mc.MovieId == id)
                        .ToListAsync();
                    
                    // Xóa các danh mục cũ
                    _context.MovieCategories.RemoveRange(existingCategories);
                    await _context.SaveChangesAsync();

                    // Thêm danh mục mới
                    if (selectedCategories != null && selectedCategories.Length > 0)
                    {
                        foreach (var categoryId in selectedCategories)
                        {
                            _context.MovieCategories.Add(new MovieCategory
                            {
                                MovieId = movie.MovieId,
                                CategoryId = categoryId
                            });
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Đã cập nhật phim thành công!";
                    return RedirectToAction(nameof(Movies));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");
            }

            await PrepareMovieFormData(selectedCategories);
            return View("MovieForm", movie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            try
            {
                // Xóa tất cả quan hệ MovieCategory
                var movieCategories = await _context.MovieCategories
                    .Where(mc => mc.MovieId == id)
                    .ToListAsync();
                
                _context.MovieCategories.RemoveRange(movieCategories);
                _context.Movies.Remove(movie);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đã xóa phim thành công!";
                return RedirectToAction(nameof(Movies));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa phim: " + ex.Message;
                return RedirectToAction(nameof(Movies));
            }
        }

        #endregion

        #region Category Management

        public async Task<IActionResult> Categories()
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var categories = await _context.Categories
                .Include(c => c.MovieCategories)
                .ToListAsync();

            return View(categories);
        }

        public IActionResult CreateCategory()
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            return View("CategoryForm", new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            // Bỏ qua validation cho các collection properties
            ModelState.Remove("MovieCategories");

            if (category.CreatedAt == default)
            {
                category.CreatedAt = DateTime.Now;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã thêm thể loại thành công!";
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");
            }
            
            return View("CategoryForm", category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View("CategoryForm", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category category)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            if (id != category.CategoryId)
            {
                return NotFound();
            }

            // Bỏ qua validation cho các collection properties
            ModelState.Remove("MovieCategories");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy bản ghi hiện tại để giữ CreatedAt không bị thay đổi
                    if (category.CreatedAt == default)
                    {
                        var existingCategory = await _context.Categories
                            .AsNoTracking()
                            .FirstOrDefaultAsync(c => c.CategoryId == id);
                        
                        if (existingCategory == null)
                        {
                            ModelState.AddModelError("", "Không tìm thấy thể loại cần sửa!");
                            return View("CategoryForm", category);
                        }
                        
                        category.CreatedAt = existingCategory.CreatedAt;
                    }
                    
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật thể loại thành công!";
                    return RedirectToAction(nameof(Categories));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật dữ liệu: " + ex.Message);
                }
            }
            else
            {
                ModelState.AddModelError("", "Dữ liệu không hợp lệ, vui lòng kiểm tra lại!");
            }
            
            return View("CategoryForm", category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Kiểm tra quyền admin
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện hành động này!";
                return RedirectToAction("Index", "Home");
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {
                // Kiểm tra xem có phim nào sử dụng thể loại này không
                var hasMovies = await _context.MovieCategories
                    .AnyAsync(mc => mc.CategoryId == id);

                if (hasMovies)
                {
                    // Nếu có, xóa tất cả các liên kết trước
                    var movieCategories = await _context.MovieCategories
                        .Where(mc => mc.CategoryId == id)
                        .ToListAsync();
                    
                    _context.MovieCategories.RemoveRange(movieCategories);
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Đã xóa thể loại thành công!";
                return RedirectToAction(nameof(Categories));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa thể loại: " + ex.Message;
                return RedirectToAction(nameof(Categories));
            }
        }

        #endregion

        #region Helper Methods

        private async Task PrepareMovieFormData(int[] selectedCategories = null)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategories = selectedCategories ?? Array.Empty<int>();
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.MovieId == id);
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }

        #endregion
    }
} 