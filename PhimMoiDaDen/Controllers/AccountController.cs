using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhimMoiDaDen.Data;
using PhimMoiDaDen.Models;
using System.Security.Cryptography;
using System.Text;

namespace PhimMoiDaDen.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var hashedPassword = HashPassword(password);
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashedPassword);

            if (user == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            // Store user info in session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role.RoleName);

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View(new User());
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            // Bỏ qua validation cho các trường không cần thiết
            ModelState.Remove("Comments");
            ModelState.Remove("Favorites");
            ModelState.Remove("Role");
            ModelState.Remove("RoleId");
            
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                    return View(user);
                }

                // Check if email already exists
                if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại");
                    return View(user);
                }

                // Set default role as user
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                if (userRole == null)
                {
                    userRole = new Role { RoleName = "User" };
                    _context.Roles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                user.RoleId = userRole.RoleId;
                user.Password = HashPassword(user.Password);
                user.CreatedAt = DateTime.Now;

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction(nameof(Login));
            }
            else 
            {
                // Hiển thị danh sách lỗi validation
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        foreach (var error in state.Errors)
                        {
                            // Thêm tất cả các lỗi vào ModelState để hiển thị trên form
                            ModelState.AddModelError("", $"Lỗi ở {key}: {error.ErrorMessage}");
                        }
                    }
                }
            }

            return View(user);
        }

        // POST: Account/Logout
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Account/Favorites
        public async Task<IActionResult> Favorites()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var userId = HttpContext.Session.GetInt32("UserId").Value;
            var favorites = await _context.Favorites
                .Include(f => f.Movie)
                    .ThenInclude(m => m.MovieCategories)
                        .ThenInclude(mc => mc.Category)
                .Where(f => f.UserId == userId)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();

            return View(favorites);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 