using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using showcase.Data;
using showcase.Models;
using showcase.Models.PortfolioViewModels;
using System.Net;
using System.Text.RegularExpressions;
using showcase.UtilityFunctions;

namespace showcase.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment env;

        public PortfolioController(ApplicationDbContext context, IHostingEnvironment environment)
        {
            db = context;
            env = environment;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            return View(await db.PortfolioEntries.Include(p => p.Image).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Show(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            PortfolioEntry entry = await db.PortfolioEntries
                .Include(p => p.Image)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                return NotFound("Portfolio Entry not found");
            }

            return View(entry);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,ShortDescription,Markdown,Html,ImageId")] PortfolioEntryViewModel entry)
        {
            if (!ModelState.IsValid)
            {
                return View(entry);
            }

            Image image = null;

            if (entry.ImageId != null)
            {
                image = db.Images.Find(entry.ImageId);
            }

            PortfolioEntry newEntry = new PortfolioEntry
            {
                Title = entry.Title,
                ShortDescription = String.Join("\n",
                    WebUtility.HtmlEncode(entry.ShortDescription)
                        .Replace("\r", "")
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => String.Format("<p>{0}</p>", s))),
                Markdown = entry.Markdown,
                Html = ShowcaseUtilities.SanitizeHtml(entry.Html),
                Image = image
            };

            db.PortfolioEntries.Add(newEntry);

            await db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Show), new { id = newEntry.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            PortfolioEntry entry = await db.PortfolioEntries
                .Include(p => p.Image)
                .Where(p => p.Id == id)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                return NotFound("Portfolio Entry not found");
            }
            
            return View(new PortfolioEntryViewModel {
                Id = entry.Id,
                Title = entry.Title,
                ShortDescription = entry.ShortDescription.Replace("<p>", "").Replace("</p>", ""),
                Markdown = entry.Markdown,
                Html = entry.Html,
                ImageId = entry.Image.Id
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Title,ShortDescription,Markdown,Html,ImageId")] PortfolioEntryViewModel entry)
        {
            if (entry.Id == null)
            {
                return BadRequest("\"id\" is null");
            }

            PortfolioEntry oldEntry = await db.PortfolioEntries.FindAsync(entry.Id);

            if (oldEntry == null)
            {
                return NotFound("Portfolio Entry not found");
            }

            Image image = null;

            if (entry.ImageId != null)
            {
                image = await db.Images.FindAsync(entry.ImageId);

                if (image == null)
                {
                    ModelState.AddModelError(nameof(entry.ImageId), String.Format("Image with id {0} not found;", entry.ImageId));
                }
            }
            
            oldEntry.Title = entry.Title;
            oldEntry.ShortDescription = entry.ShortDescription;
            oldEntry.Image = image;
            oldEntry.Markdown = entry.Markdown;
            oldEntry.Html = ShowcaseUtilities.SanitizeHtml(entry.Html);

            await db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Show), new { id = entry.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            return View(await db.PortfolioEntries.Include(p => p.Image).ToListAsync());
        }

        /*
        // GET: Portfolio
        public async Task<IActionResult> Index()
        {
            return View(await _context.Portfolio.ToListAsync());
        }

        // GET: Portfolio/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolio = await _context.Portfolio
                .SingleOrDefaultAsync(m => m.Id == id);
            if (portfolio == null)
            {
                return NotFound();
            }

            return View(portfolio);
        }

        // GET: Portfolio/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Portfolio/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,ShortDescription,Markdown,Html")] Portfolio portfolio)
        {
            if (ModelState.IsValid)
            {
                _context.Add(portfolio);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(portfolio);
        }

        // GET: Portfolio/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolio = await _context.Portfolio.SingleOrDefaultAsync(m => m.Id == id);
            if (portfolio == null)
            {
                return NotFound();
            }
            return View(portfolio);
        }

        // POST: Portfolio/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,ShortDescription,Markdown,Html")] Portfolio portfolio)
        {
            if (id != portfolio.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(portfolio);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PortfolioExists(portfolio.Id))
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
            return View(portfolio);
        }

        // GET: Portfolio/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var portfolio = await _context.Portfolio
                .SingleOrDefaultAsync(m => m.Id == id);
            if (portfolio == null)
            {
                return NotFound();
            }

            return View(portfolio);
        }

        // POST: Portfolio/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var portfolio = await _context.Portfolio.SingleOrDefaultAsync(m => m.Id == id);
            _context.Portfolio.Remove(portfolio);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PortfolioExists(int id)
        {
            return _context.Portfolio.Any(e => e.Id == id);
        }
        */
    }
}
