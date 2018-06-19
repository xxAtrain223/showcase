using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace showcase.Controllers
{
    public class InstallController : Controller
    {
        private readonly IApplicationLifetime ApplicationLifetime;

        public InstallController(IApplicationLifetime applicationLifetime)
        {
            ApplicationLifetime = applicationLifetime;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}