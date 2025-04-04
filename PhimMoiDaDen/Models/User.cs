using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhimMoiDaDen.Models
{
    public class User
    {
        public User()
        {
            // Khởi tạo collection để tránh lỗi validation
            Comments = new List<Comment>();
            Favorites = new List<Favorite>();
            CreatedAt = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        // RoleId không cần Required vì sẽ tự động gán khi đăng ký
        public int RoleId { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Role Role { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
} 