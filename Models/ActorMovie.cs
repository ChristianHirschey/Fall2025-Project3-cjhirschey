using System.ComponentModel.DataAnnotations;

namespace Fall2025_Project3_cjhirschey.Models
{
    public class ActorMovie
    {
        public int Id { get; set; }

        [Display(Name = "Actor")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an actor.")]
        public int ActorId { get; set; }
        public virtual Actor? Actor { get; set; }

        [Display(Name = "Movie")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a movie.")]
        public int MovieId { get; set; }
        public virtual Movie? Movie { get; set; }
    }
}