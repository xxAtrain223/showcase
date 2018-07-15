using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using showcase.Models;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace showcase.Controllers
{
    public class InstallController : Controller
    {
        private readonly IHostingEnvironment env;
        private readonly IApplicationLifetime ApplicationLifetime;
        private readonly ApplicationSettings ApplicationSettings;

        public InstallController(IHostingEnvironment environment, IApplicationLifetime applicationLifetime, ApplicationSettings applicationSettings)
        {
            env = environment;
            ApplicationLifetime = applicationLifetime;
            ApplicationSettings = applicationSettings;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(InstallationViewModel.FromApplicationSettings(ApplicationSettings));
        }

        [HttpPost]
        public async Task<IActionResult> Index(InstallationViewModel installationViewModel)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(installationViewModel.ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(nameof(installationViewModel.ConnectionString), ex.Message);
            }

            if (!ModelState.IsValid)
            {
                return View(installationViewModel);
            }
            
            ApplicationSettings applicationSettings = installationViewModel.ToApplicationSettings();
            applicationSettings.Installed = true;

            string installJson = JsonConvert.SerializeObject(applicationSettings, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented });

            await System.IO.File.WriteAllTextAsync(String.Format("{0}/appsettings.json", env.ContentRootPath), installJson);

            ApplicationLifetime.StopApplication();

            return RedirectToAction("Index", "Home", null);
        }

        internal T RecursiveNullify<T>(T t) where T : class
        {
            bool propsAreNull = true;
            foreach (var property in t.GetType().GetProperties())
            {
                object propObj = property.GetValue(t);
                if (!property.PropertyType.Namespace.StartsWith("System") && !property.PropertyType.IsEnum)
                {
                    propObj = RecursiveNullify(propObj);
                    property.SetValue(t, propObj);
                }
                propsAreNull &= propObj == null;
            }

            if (propsAreNull)
            {
                return null;
            }
            return t;
        }

        [HttpPost]
        public async Task<ActionResult> TestConnectionString(string connectionString)
        {
            if (String.IsNullOrWhiteSpace(connectionString))
            {
                return BadRequest("Connection String is empty");
            }

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    await sqlConnection.OpenAsync();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("Connection String is valid");
        }
    }
}