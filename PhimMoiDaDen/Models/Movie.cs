using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PhimMoiDaDen.Models
{
    public class Movie
    {
        public Movie()
        {
            // Khởi tạo các collection để tránh lỗi validation
            MovieCategories = new List<MovieCategory>();
            Comments = new List<Comment>();
            Favorites = new List<Favorite>();
            CreatedAt = DateTime.Now;
        }

        public int MovieId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string Genre { get; set; }

        [Required]
        [StringLength(255)]
        public string PosterUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string VideoUrl { get; set; }

        public int Views { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ReleaseYear { get; set; }

        [Required]
        [StringLength(100)]
        public string Director { get; set; }

        [Required]
        [StringLength(500)]
        public string Cast { get; set; }

        public float Rating { get; set; }

        public int Duration { get; set; }

        [Required]
        [StringLength(50)]
        public string Language { get; set; }

        [Required]
        [StringLength(50)]
        public string Country { get; set; }

        public string BackdropUrl { get; set; }

        public string TrailerUrl { get; set; }

        [StringLength(20)]
        public string Quality { get; set; }

        // Navigation properties
        public ICollection<MovieCategory> MovieCategories { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
} 