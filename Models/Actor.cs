using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_cjhirschey.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(20)]
        public string Gender { get; set; }

        public int Age { get; set; }

        [Display(Name = "IMDb Link")]
        [Url]
        public string ImdbUrl { get; set; }

        public byte[]? Photo { get; set; }

        public virtual ICollection<ActorMovie> ActorMovies { get; set; } = new List<ActorMovie>();
    }
}