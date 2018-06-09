using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using showcase.Data;
using showcase.Models;
using PaulMiami.AspNetCore.Mvc.Recaptcha;
using showcase.Models.HomeViewModels;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Configuration;
using Microsoft.AspNetCore.Authorization;

namespace showcase.Controllers
{
    public class Contact
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public SmtpServer SmtpServer { get; set; }
    }

    public class SmtpServer
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IConfiguration config;

        public HomeController(ApplicationDbContext context, IConfiguration configuration)
        {
            db = context;
            config = configuration;
        }
        
        public async Task<IActionResult> Index()
        {
            return View(await db.BlogEntries
                .Include(e => e.Image)
                .Include(e => e.Tags)
                .Where(e => e.Tags.Any(t => t.Name.ToLower() == "home"))
                .FirstOrDefaultAsync());
        }
        
        public IActionResult Contact()
        {
            ViewData["SmtpServerAvailable"] = config.GetSection("Contact:SmtpServer").GetChildren().Count() > 0;
            return View();
        }

        [HttpPost]
        [ValidateRecaptcha]
        public async Task<IActionResult> Contact([Bind("Name,EmailAddress,Message,SendCopy")] ContactViewModel contactViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(contactViewModel);
            }

            Contact contact = new Contact();
            config.GetSection("Contact").Bind(contact);
            
            SmtpClient client = new SmtpClient
            {
                UseDefaultCredentials = false,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Host = contact.SmtpServer.Host,
                Port = contact.SmtpServer.Port,
                EnableSsl = true,
                Credentials = new NetworkCredential(contact.SmtpServer.Username, contact.SmtpServer.Password)
            };

            MailMessage message = new MailMessage();
            message.From = new MailAddress(contact.Email, contact.Name);
            message.ReplyToList.Add(new MailAddress(contactViewModel.EmailAddress, contactViewModel.Name));
            message.To.Add(new MailAddress(contact.Email, contact.Name));
            message.Subject = "Showcase Contact Form Response from " + contactViewModel.Name;
            message.Body = contactViewModel.Message;

            await client.SendMailAsync(message);

            if (contactViewModel.SendCopy)
            {
                MailMessage messageCopy = new MailMessage();
                messageCopy.From = new MailAddress(contact.Email, contact.Name);
                messageCopy.ReplyToList.Add(new MailAddress(contact.Email, contact.Name));
                messageCopy.To.Add(new MailAddress(contactViewModel.EmailAddress, contactViewModel.Name));
                messageCopy.Subject = "Showcase Contact Form Response from " + contactViewModel.Name;
                messageCopy.Body = contactViewModel.Message;

                await client.SendMailAsync(messageCopy);
            }

            HttpContext.Session.Set("ContactEmail", contactViewModel.EmailAddress);

            return RedirectToActionPreserveMethod(nameof(MessageSent));
        }

        [HttpPost]
        public IActionResult MessageSent(string email)
        {
            ViewData["email"] = email;

            return View();
        }

        [HttpGet]
        public IActionResult ShowEmail()
        {
            return View();
        }

        [HttpPost]
        [ValidateRecaptcha]
        [ActionName("ShowEmail")]
        public IActionResult ShowEmailPost()
        {
            ViewData["EmailDisplay"] = String.Format("{0} <{1}>", config["Contact:Name"], config["Contact:Email"]);
            ViewData["EmailAddress"] = config["Contact:Email"];

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            BlogEntry entry = await db.BlogEntries
                .Include(e => e.Tags)
                .Where(e => e.Tags.Any(t => t.Name.ToLower() == "home"))
                .FirstOrDefaultAsync();

            ViewData["HomeBlogId"] = entry?.Id;

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
