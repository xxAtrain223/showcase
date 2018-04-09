using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using showcase.Data;
using showcase.Models;
using showcase.Models.BlogViewModels;
using showcase.UtilityFunctions;

namespace showcase.Controllers
{
    public class BlogController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment env;

        public BlogController(ApplicationDbContext context, IHostingEnvironment environment)
        {
            db = context;
            env = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await db.BlogEntries
                .Include(b => b.Image)
                .Include(b => b.Tags)
                .Where(b => b.ShowOnList)
                .ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Show(int? id, string title)
        {
            BlogEntry entry = null;

            if (id != null)
            {
                entry = await db.BlogEntries
                    .Include(b => b.Image)
                    .Include(b => b.Tags)
                    .Where(b => b.Id == id)
                    .FirstOrDefaultAsync();
            }
            else if (!String.IsNullOrWhiteSpace(title))
            {
                entry = await db.BlogEntries
                    .Include(b => b.Image)
                    .Include(b => b.Tags)
                    .Where(b => b.Title.Replace(" ", "").ToLower() == title.Replace(" ", "").ToLower())
                    .FirstOrDefaultAsync();
            }
            else
            {
                return BadRequest("id and title are null");
            }

            if (entry == null)
            {
                return NotFound("Blog Entry not found");
            }

            // If Logged in (Admin), always show
            if (entry.ShowOnList == false)
            {
                return NotFound("Blog Entry is not currently visible");
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
        public async Task<IActionResult> Create([Bind("Title,TitlePlacement,ShortDescription,Markdown,Html,ImageId,Image,ImagePlacement,ShowOnList,ShowFooter,Tags")] BlogEntryViewModel entry)
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

            List<string> tagStrs = (!String.IsNullOrWhiteSpace(entry.Tags)) ? entry.Tags.Split(',').Select(s => s.Trim().ToLower()).ToList() : new List<string>();
            List<Tag> tags = new List<Tag>();

            foreach (string tagStr in tagStrs)
            {
                Tag tag = await db.Tags.Where(t => t.Name == tagStr).FirstOrDefaultAsync();

                if (tag == null)
                {
                    tag = new Tag { Name = tagStr };
                    await db.Tags.AddAsync(tag);
                }

                tags.Add(tag);
            }

            BlogEntry newEntry = new BlogEntry
            {
                Title = entry.Title,
                ShortDescription = String.Join("\n",
                    WebUtility.HtmlEncode(entry.ShortDescription)
                        .Replace("\r", "")
                        .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => String.Format("<p>{0}</p>", s))),
                Markdown = entry.Markdown,
                Html = ShowcaseUtilities.SanitizeHtml(entry.Html),
                Image = image,
                ImagePlacement = entry.ImagePlacement,
                TitlePlacement = entry.TitlePlacement,
                ShowOnList = entry.ShowOnList,
                DateUploaded = DateTimeOffset.Now,
                Tags = tags
            };

            db.BlogEntries.Add(newEntry);

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

            BlogEntry entry = await db.BlogEntries.Include(b => b.Image).Include(b => b.Tags).Where(b => b.Id == id).FirstOrDefaultAsync();

            if (entry == null)
            {
                return NotFound("Blog Entry not found");
            }

            return View(new BlogEntryViewModel {
                Id = entry.Id,
                Title = entry.Title,
                TitlePlacement = entry.TitlePlacement,
                ShortDescription = entry.ShortDescription.Replace("<p>", "").Replace("</p>", ""),
                Markdown = entry.Markdown,
                Html = entry.Html,
                ImageId = entry.Image?.Id,
                Image = entry.Image,
                ImagePlacement = entry.ImagePlacement,
                ShowOnList = entry.ShowOnList,
                Tags = String.Join(",", entry.Tags.Select(t => t.Name))
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Title,TitlePlacement,ShortDescription,Markdown,Html,ImageId,Image,ImagePlacement,ShowOnList,ShowFooter,Tags")] BlogEntryViewModel entry)
        {
            if (entry.Id == null)
            {
                return BadRequest("\"id\" is null");
            }

            BlogEntry oldEntry = await db.BlogEntries
                .Where(b => b.Id == entry.Id)
                .Include(b => b.Image)
                .Include(b => b.Tags)
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

            List<string> tagStrs = (!String.IsNullOrWhiteSpace(entry.Tags)) ? entry.Tags.Split(',').Select(s => s.Trim().ToLower()).ToList() : new List<string>();
            List<Tag> tags = new List<Tag>();

            foreach (string tagStr in tagStrs)
            {
                Tag tag = await db.Tags.Where(t => t.Name == tagStr).FirstOrDefaultAsync();

                if (tag == null)
                {
                    tag = new Tag { Name = tagStr };
                    await db.Tags.AddAsync(tag);
                }

                tags.Add(tag);
            }

            oldEntry.Id = (int)entry.Id;
            oldEntry.Title = entry.Title;
            oldEntry.TitlePlacement = entry.TitlePlacement;
            oldEntry.ShortDescription = String.Join("\n",
                WebUtility.HtmlEncode(entry.ShortDescription)
                    .Replace("\r", "")
                    .Split("\n", StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => String.Format("<p>{0}</p>", s)));
            oldEntry.Image = image;
            oldEntry.ImagePlacement = entry.ImagePlacement;
            oldEntry.Markdown = entry.Markdown;
            oldEntry.Html = ShowcaseUtilities.SanitizeHtml(entry.Html);
            oldEntry.ShowOnList = entry.ShowOnList;
            oldEntry.ShowFooter = entry.ShowFooter;
            oldEntry.Tags = tags;

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

            BlogEntry entry = await db.BlogEntries.Include(e => e.Tags).Where(e => e.Id == id).FirstOrDefaultAsync();

            if (entry == null)
            {
                return NotFound("Portfolio Entry not found");
            }
            
            db.BlogEntries.Remove(entry);

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
