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
using Microsoft.Extensions.Configuration;

namespace showcase.Controllers
{
    public class PortfolioController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment env;

        public PortfolioController(ApplicationDbContext context, IHostingEnvironment environment, ApplicationSettings settings)
            : base(settings)
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
        public async Task<IActionResult> Show(int? id, string title)
        {
            PortfolioEntry entry = null;

            if (id != null)
            {
                entry = await db.PortfolioEntries
                    .Include(p => p.Image)
                    .Where(p => p.Id == id)
                    .FirstOrDefaultAsync();
            }
            else if (!String.IsNullOrWhiteSpace(title))
            {
                entry = await db.PortfolioEntries
                    .Include(p => p.Image)
                    .Where(p => p.Title.Replace(" ", "").ToLower() == title.Replace(" ", "").ToLower())
                    .FirstOrDefaultAsync();
            }
            else
            {
                return BadRequest("id and title are null");
            }
            
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
                ImageId = entry.Image?.Id,
                Image = entry.Image
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

            PortfolioEntry oldEntry = await db.PortfolioEntries
                .Where(p => p.Id == entry.Id)
                .Include(p => p.Image)
                .FirstOrDefaultAsync();

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
            oldEntry.ShortDescription = String.Join("\n",
                WebUtility.HtmlEncode(entry.ShortDescription)
                    .Replace("\r", "")
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => String.Format("<p>{0}</p>", s)));
            oldEntry.Image = image;
            oldEntry.Markdown = entry.Markdown;
            oldEntry.Html = ShowcaseUtilities.SanitizeHtml(entry.Html);

            await db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Show), new { id = entry.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            PortfolioEntry entry = await db.PortfolioEntries.FindAsync(id);

            if (entry == null)
            {
                return NotFound("Portfolio Entry not found");
            }

            db.PortfolioEntries.Remove(entry);

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
