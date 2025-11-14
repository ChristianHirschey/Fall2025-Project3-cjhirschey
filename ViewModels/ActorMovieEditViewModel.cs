using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace Fall2025_Project3_cjhirschey.ViewModels
{
    public class ActorMovieEditViewModel
    {
        public int Id { get; set; }
        public int ActorId { get; set; }
        public int MovieId { get; set; }

        // Select lists for the dropdowns - not bound/validated from the request
        [ValidateNever]
        public SelectList Actors { get; set; }
        [ValidateNever]
        public SelectList Movies { get; set; }
    }
}