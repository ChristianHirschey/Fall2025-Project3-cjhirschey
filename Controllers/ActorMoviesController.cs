using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Fall2025_Project3_cjhirschey.Data;
using Fall2025_Project3_cjhirschey.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Fall2025_Project3_cjhirschey.ViewModels;

namespace Fall2025_Project3_cjhirschey.Controllers
{
    public class ActorMoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ActorMoviesController> _logger;

        public ActorMoviesController(ApplicationDbContext context, ILogger<ActorMoviesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ActorMovies
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.ActorMovie.Include(a => a.Actor).Include(a => a.Movie);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: ActorMovies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovie
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        // GET: ActorMovies/Create
        public async Task<IActionResult> Create()
        {
            var actors = await _context.Actor.OrderBy(a => a.Name).ToListAsync();
            var movies = await _context.Movie.OrderBy(m => m.Title).ToListAsync();

            var vm = new ActorMovieEditViewModel
            {
                Actors = new SelectList(actors, "Id", "Name"),
                Movies = new SelectList(movies, "Id", "Title")
            };

            return View(vm);
        }

        // POST: ActorMovies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActorMovieEditViewModel vm)
        {
            // Basic validation
            if (vm.ActorId <= 0)
            {
                ModelState.AddModelError("ActorId", "Please select an actor.");
            }
            if (vm.MovieId <= 0)
            {
                ModelState.AddModelError("MovieId", "Please select a movie.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ActorMovies Create model state invalid: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                var actorsList = await _context.Actor.OrderBy(a => a.Name).ToListAsync();
                var moviesList = await _context.Movie.OrderBy(m => m.Title).ToListAsync();
                vm.Actors = new SelectList(actorsList, "Id", "Name", vm.ActorId);
                vm.Movies = new SelectList(moviesList, "Id", "Title", vm.MovieId);
                return View(vm);
            }

            // Prevent duplicate relationships
            var exists = await _context.ActorMovie.AnyAsync(am => am.ActorId == vm.ActorId && am.MovieId == vm.MovieId);
            if (exists)
            {
                ModelState.AddModelError(string.Empty, "This relationship already exists.");
                var actors = await _context.Actor.OrderBy(a => a.Name).ToListAsync();
                var movies = await _context.Movie.OrderBy(m => m.Title).ToListAsync();
                vm.Actors = new SelectList(actors, "Id", "Name", vm.ActorId);
                vm.Movies = new SelectList(movies, "Id", "Title", vm.MovieId);
                return View(vm);
            }

            try
            {
                var actorMovie = new ActorMovie { ActorId = vm.ActorId, MovieId = vm.MovieId };
                _context.Add(actorMovie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error saving ActorMovie relationship. ActorId={ActorId}, MovieId={MovieId}", vm.ActorId, vm.MovieId);
                ModelState.AddModelError(string.Empty, "Unable to save relationship. Ensure the selected actor and movie exist and the relationship is not duplicated.");
                var actors = await _context.Actor.OrderBy(a => a.Name).ToListAsync();
                var movies = await _context.Movie.OrderBy(m => m.Title).ToListAsync();
                vm.Actors = new SelectList(actors, "Id", "Name", vm.ActorId);
                vm.Movies = new SelectList(movies, "Id", "Title", vm.MovieId);
                return View(vm);
            }
        }

        // GET: ActorMovies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovie.FindAsync(id);
            if (actorMovie == null)
            {
                return NotFound();
            }
            var vm = new ActorMovieEditViewModel
            {
                Id = actorMovie.Id,
                ActorId = actorMovie.ActorId,
                MovieId = actorMovie.MovieId,
                Actors = new SelectList(await _context.Actor.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", actorMovie.ActorId),
                Movies = new SelectList(await _context.Movie.OrderBy(m => m.Title).ToListAsync(), "Id", "Title", actorMovie.MovieId)
            };
            return View(vm);
        }

        // POST: ActorMovies/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ActorMovieEditViewModel vm)
        {
            if (vm == null) return NotFound();

            var id = vm.Id;

            // Load existing tracked entity
            var existing = await _context.ActorMovie.FirstOrDefaultAsync(am => am.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            // Basic validation
            if (vm.ActorId <= 0)
            {
                ModelState.AddModelError("ActorId", "Please select an actor.");
            }
            if (vm.MovieId <= 0)
            {
                ModelState.AddModelError("MovieId", "Please select a movie.");
            }

            // Prevent setting to a duplicate relationship
            var duplicate = await _context.ActorMovie.AnyAsync(am => am.ActorId == vm.ActorId && am.MovieId == vm.MovieId && am.Id != id);
            if (duplicate)
            {
                ModelState.AddModelError(string.Empty, "This relationship already exists.");
            }

            if (!ModelState.IsValid)
            {
                vm.Actors = new SelectList(await _context.Actor.OrderBy(a => a.Name).ToListAsync(), "Id", "Name", vm.ActorId);
                vm.Movies = new SelectList(await _context.Movie.OrderBy(m => m.Title).ToListAsync(), "Id", "Title", vm.MovieId);

                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                if (errors.Any()) _logger.LogWarning("ActorMovies Edit model state invalid: {Errors}", string.Join("; ", errors));

                return View(vm);
            }

            // Update scalar properties on the tracked entity
            existing.ActorId = vm.ActorId;
            existing.MovieId = vm.MovieId;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ActorMovieExists(existing.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // GET: ActorMovies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actorMovie = await _context.ActorMovie
                .Include(a => a.Actor)
                .Include(a => a.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actorMovie == null)
            {
                return NotFound();
            }

            return View(actorMovie);
        }

        // POST: ActorMovies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actorMovie = await _context.ActorMovie.FindAsync(id);
            if (actorMovie != null)
            {
                _context.ActorMovie.Remove(actorMovie);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ActorMovieExists(int id)
        {
            return _context.ActorMovie.Any(e => e.Id == id);
        }
    }
}
