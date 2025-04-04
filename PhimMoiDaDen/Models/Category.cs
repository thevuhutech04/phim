using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhimMoiDaDen.Models
{
    public class Category
    {
        public Category()
        {
            // Khởi tạo collection để tránh lỗi validation
            MovieCategories = new List<MovieCategory>();
            CreatedAt = DateTime.Now;
        }

        public int CategoryId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ICollection<MovieCategory> MovieCategories { get; set; }
    }
} 