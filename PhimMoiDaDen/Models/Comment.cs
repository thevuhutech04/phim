using System;
using System.ComponentModel.DataAnnotations;

namespace PhimMoiDaDen.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int MovieId { get; set; }
        public int UserId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public Movie Movie { get; set; }
        public User User { get; set; }
    }
} 