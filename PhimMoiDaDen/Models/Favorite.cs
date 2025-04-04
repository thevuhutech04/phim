using System;

namespace PhimMoiDaDen.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public DateTime AddedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Movie Movie { get; set; }
    }
} 