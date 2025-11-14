using Fall2025_Project3_cjhirschey.Services;
using Fall2025_Project3_cjhirschey.Data;
using Fall2025_Project3_cjhirschey.Models;
using Fall2025_Project3_cjhirschey.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Fall2025_Project3_cjhirschey.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AiApiService _aiService;
        private readonly ILogger<MoviesController> _logger;

        public MoviesController(ApplicationDbContext context, AiApiService aiService, ILogger<MoviesController> logger)
        {
            _context = context;
            _aiService = aiService;
            _logger = logger;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            return View(await _context.Movie
                .Include(m => m.ActorMovies)
                    .ThenInclude(am => am.Actor)
                .ToListAsync());
        }

        // GET: Movies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
            .Include(m => m.ActorMovies)
            .ThenInclude(am => am.Actor)
            .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            // call AI service
            var reviews = await _aiService.GetMovieReviewsAsync(movie.Title);

            // calculate average sentiment for movie
            var avgSentiment = reviews.Any() ? reviews.Average(r => r.SentimentScore) : 0;

            // create viewmodel
            var viewModel = new MovieDetailViewModel
            {
                Movie = movie,
                Reviews = reviews,
                AverageSentiment = avgSentiment
            };

            return View(viewModel);
        }

        // GET: Movies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ImdbUrl,Genre,ReleaseYear")] Movie movie, IFormFile PosterFile)
        {
            if (PosterFile != null && PosterFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await PosterFile.CopyToAsync(ms);
                    movie.Poster = ms.ToArray();
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(movie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(movie);
        }

        // GET: Movies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }

        // POST: Movies/Edit/5
        // update scalar properties on the tracked entity and preserve Poster unless a new file uploaded
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ImdbUrl,Genre,ReleaseYear")] Movie movie, IFormFile PosterFile)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            // load existing tracked entity
            var existing = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            // update scalar properties
            existing.Title = movie.Title;
            existing.ImdbUrl = movie.ImdbUrl;
            existing.Genre = movie.Genre;
            existing.ReleaseYear = movie.ReleaseYear;

            // replace poster only if a new file provided
            if (PosterFile != null && PosterFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await PosterFile.CopyToAsync(ms);
                    existing.Poster = ms.ToArray();
                }
            }

            ModelState.Remove("Poster");
            ModelState.Remove("existing.Poster");
            ModelState.Remove("PosterFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(existing.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (errors.Any()) _logger.LogWarning("Movies Edit model state invalid after removing poster keys: {Errors}", string.Join("; ", errors));

            return View(existing);
        }

        // GET: Movies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movie
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie == null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movie.FindAsync(id);
            if (movie != null)
            {
                // remove actor-movie relationships first
                var links = _context.ActorMovie.Where(am => am.MovieId == id);
                _context.ActorMovie.RemoveRange(links);

                _context.Movie.Remove(movie);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movie.Any(e => e.Id == id);
        }
    }
}
