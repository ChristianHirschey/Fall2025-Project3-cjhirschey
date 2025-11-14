using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_cjhirschey.Models
{
    public class Movie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "IMDb Link")]
        [Url]
        public string ImdbUrl { get; set; }

        [StringLength(50)]
        public string Genre { get; set; }

        [Display(Name = "Release Year")]
        public int ReleaseYear { get; set; }

        public byte[]? Poster { get; set; }

        public virtual ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}