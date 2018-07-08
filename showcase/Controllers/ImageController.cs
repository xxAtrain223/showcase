using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using showcase.Data;
using showcase.Models;
using showcase.UtilityFunctions;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace showcase.Controllers
{
    public class ImageController : ControllerBase
    {
        private readonly ApplicationDbContext db;
        private readonly IHostingEnvironment env;

        public ImageController(ApplicationDbContext context, IHostingEnvironment environment, ApplicationSettings settings)
            : base(settings)
        {
            db = context;
            env = environment;
        }
        
        [HttpGet]
        [Route("Image/{id:int}")]
        public async Task<IActionResult> GetImage(int id = -1)
        {
            Image image = await db.Images.FindAsync(id);

            if (image != null)
            {
                return Redirect(Url.Content(image.Path)); //Physical File?
            }
            else
            {
                return NotFound("Image not found");
            }
        }

        public JsonResult GetImages(int pagesize = 10, int page = 1)
        {
            page = (page > 0) ? page - 1 : 0;

            return Json(new
            {
                pagesize,
                page = page + 1,
                numberOfPages = db.Images.Count() / pagesize,
                images = db.Images
                    .OrderByDescending(i => i.Id)
                    .Skip(page * pagesize)
                    .Take(pagesize)
                    .ToList()
                    .Select((i) => new { id = i.Id.ToString(), name = i.Name, src = Url.Content(i.Path), alt = i.AltText })
            });
        }
        
        [HttpPost]
        //[Authorize]
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public JsonResult UploadImage(string Name, string AltText, int CategoryId, IFormFile Image)
        {
            if (ShowcaseUtilities.IsImage(Image))
            {
                string virtualPath = String.Format("~/images/{0}{1}", Guid.NewGuid(), Path.GetExtension(Image.FileName));
                ShowcaseUtilities.SaveStreamToFile(Image.OpenReadStream(), virtualPath.Replace("~", env.WebRootPath));
                
                Image imageModel = new Image
                {
                    Name = Name,
                    AltText = AltText,
                    Path = virtualPath
                };
                
                db.Images.Add(imageModel);
                db.SaveChanges();
                
                return Json(new
                {
                    id = imageModel.Id,
                    name = imageModel.Name,
                    src = Url.Content(virtualPath),
                    alt = imageModel.AltText
                });
            }
            else
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { error = "File is not an image" });
            }
        }

        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public async Task<IActionResult> Manage(int pagesize = 10, int page = 1)
        {
            double count = await db.Images.CountAsync();

            ViewBag.currentPage = page;
            ViewBag.numberOfPages = (int)Math.Ceiling(count / pagesize);

            return View(await db.Images
                .Skip((page - 1) * pagesize)
                .Take(pagesize)
                .ToListAsync());
        }

        // GET: Image/Edit/5
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            var image = await db.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // POST: Image/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public async Task<IActionResult> Edit([Bind("Id,Name,AltText")] Image image)
        {
            Image currentImage = await db.Images.FindAsync(image.Id);

            if (currentImage == null)
            {
                return NotFound("currentImage not found");
            }

            currentImage.Name = image.Name;
            currentImage.AltText = image.AltText;

            if (ModelState.IsValid)
            {
                db.Update(currentImage);
                await db.SaveChangesAsync();

                return RedirectToAction(nameof(Manage));
            }

            return View(image);
        }

        // GET: Image/Delete/5
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest("\"id\" is null");
            }

            Image image = await db.Images.FindAsync(id);

            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Image image = await db.Images.FindAsync(id);

            string realPath = image.Path.Replace("~", env.WebRootPath);
            
            db.Images.Remove(image);
            await db.SaveChangesAsync();

            System.IO.File.Delete(realPath);

            return RedirectToAction(nameof(Manage));
        }

        [HttpGet]
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = Policies.ManagementIpWhitelist)]
        public async Task<IActionResult> Upload(string Name, string AltText, IFormFile FormFile)
        {
            if (ShowcaseUtilities.IsImage(FormFile))
            {
                string virtualPath = String.Format("~/images/{0}{1}", Guid.NewGuid(), Path.GetExtension(FormFile.FileName));
                await ShowcaseUtilities.SaveStreamToFileAsync(FormFile.OpenReadStream(), virtualPath.Replace("~", env.WebRootPath));

                Image imageModel = new Image
                {
                    Name = Name,
                    AltText = AltText,
                    Path = virtualPath
                };

                await db.Images.AddAsync(imageModel);
                await db.SaveChangesAsync();

                return RedirectToAction(nameof(Manage));
            }
            else
            {
                return BadRequest("File is not an image");
            }
        }
    }
}
