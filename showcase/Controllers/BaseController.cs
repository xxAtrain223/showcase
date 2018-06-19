using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace showcase.Controllers
{
    public class ControllerBase : Controller
    {
        protected readonly ApplicationSettings ApplicationSettings;

        public ControllerBase(ApplicationSettings applicationSettings)
        {
            ApplicationSettings = applicationSettings;
        }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!ApplicationSettings.Installed)
            {
                if (!(context.Controller is InstallController))
                {
                    context.Result = LocalRedirect(Url.Action("Index", "Install"));
                }
            }

            base.OnActionExecuting(context);
        }
    }
}
