using Fall2025_Project3_cjhirschey.Services;
using Fall2025_Project3_cjhirschey.Data;
using Fall2025_Project3_cjhirschey.Models;
using Fall2025_Project3_cjhirschey.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Fall2025_Project3_cjhirschey.Controllers
{
    public class ActorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AiApiService _aiService;
        private readonly ILogger<ActorsController> _logger;

        public ActorsController(ApplicationDbContext context, AiApiService aiService, ILogger<ActorsController> logger)
        {
            _context = context;
            _aiService = aiService;
            _logger = logger;
        }

        // GET: Actors
        public async Task<IActionResult> Index()
        {
            // Include ActorMovies and Movies so the view can display associated movies for each actor
            return View(await _context.Actor
                .Include(a => a.ActorMovies)
                    .ThenInclude(am => am.Movie)
                .ToListAsync());
        }

        // GET: Actors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .Include(a => a.ActorMovies)
                .ThenInclude(am => am.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            var tweets = await _aiService.GetActorTweetsAsync(actor.Name);

            // Calculate overall sentiment
            var avgSentiment = tweets.Any() ? tweets.Average(t => t.SentimentScore) : 0;

            // Create the ViewModel
            var viewModel = new ActorDetailViewModel
            {
                Actor = actor,
                Tweets = tweets,
                OverallSentiment = avgSentiment
            };

            return View(viewModel);
        }

        // GET: Actors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Actors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Gender,Age,ImdbUrl")] Actor actor, IFormFile PhotoFile)
        {
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await PhotoFile.CopyToAsync(ms);
                    actor.Photo = ms.ToArray();
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(actor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(actor);
        }

        // GET: Actors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor.FindAsync(id);
            if (actor == null)
            {
                return NotFound();
            }
            return View(actor);
        }

        // POST: Actors/Edit/5
        // Update only scalar properties and preserve Photo unless a new file is uploaded.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Gender,Age,ImdbUrl")] Actor actor, IFormFile PhotoFile)
        {
            if (id != actor.Id)
            {
                return NotFound();
            }

            // Load existing entity (tracked)
            var existing = await _context.Actor.FirstOrDefaultAsync(a => a.Id == id);
            if (existing == null)
            {
                return NotFound();
            }

            // Update scalar properties
            existing.Name = actor.Name;
            existing.Gender = actor.Gender;
            existing.Age = actor.Age;
            existing.ImdbUrl = actor.ImdbUrl;

            // If a new photo was uploaded, replace it; otherwise keep existing.Photo
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    await PhotoFile.CopyToAsync(ms);
                    existing.Photo = ms.ToArray();
                }
            }

            // Remove potential model-state errors related to Photo/PhotoFile so they don't block saving
            ModelState.Remove("Photo");
            ModelState.Remove("existing.Photo");
            ModelState.Remove("PhotoFile");

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActorExists(existing.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Log remaining errors for diagnostics
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            if (errors.Any()) _logger.LogWarning("Actors Edit model state invalid after removing image keys: {Errors}", string.Join("; ", errors));

            return View(existing);
        }

        // GET: Actors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var actor = await _context.Actor
                .FirstOrDefaultAsync(m => m.Id == id);
            if (actor == null)
            {
                return NotFound();
            }

            return View(actor);
        }

        // POST: Actors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var actor = await _context.Actor.FindAsync(id);
            if (actor != null)
            {
                // Remove actor-movie relationships first to avoid FK issues
                var links = _context.ActorMovie.Where(am => am.ActorId == id);
                _context.ActorMovie.RemoveRange(links);

                _context.Actor.Remove(actor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActorExists(int id)
        {
            return _context.Actor.Any(e => e.Id == id);
        }
    }
}
