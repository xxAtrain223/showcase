using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using showcase.Data;
using showcase.Models;
using showcase.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using AspNetCore.IServiceCollection.AddIUrlHelper;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using showcase;
using Microsoft.AspNetCore.Rewrite;
using PaulMiami.AspNetCore.Mvc.Recaptcha;
using Microsoft.AspNetCore.Authentication;
using showcase.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.DataAnnotations.Internal;
using showcase.Attributes;

namespace showcase
{
    public static class Policies
    {
        public const string ManagementIpWhitelist = "ManagementIpWhitelist";
        public const string ManagementIpWhitelistAllowAnonymous = "ManagementIpWhitelistAllowAnonymous";
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (Boolean.TryParse(Configuration["Installed"], out bool installed) && installed)
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

                services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                services.Configure<IdentityOptions>(options =>
                {
                    // Password settings
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequiredUniqueChars = 6;

                    // Lockout settings
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;
                });

                services.ConfigureApplicationCookie(options =>
                {
                    // Cookie settings
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Expiration = TimeSpan.FromDays(150);
                    options.LoginPath = "/Account/AccessDenied"; // "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/AccessDenied";
                    options.SlidingExpiration = true;
                });

                // Add application services.
                services.AddTransient<IEmailSender, EmailSender>();

                services.AddUrlHelper();

                services.AddMvc().AddSessionStateTempDataProvider();

                services.AddSession();

                services.AddRecaptcha(new RecaptchaOptions
                {
                    SiteKey = Configuration["Recaptcha:SiteKey"],
                    SecretKey = Configuration["Recaptcha:SecretKey"]
                });

                services.AddAuthentication().AddExternalAuthentications(Configuration.GetSection("Authentication"));
            }
            else
            {
                services.AddMvc();
            }

            services.AddSingleton<ApplicationSettings>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.ManagementIpWhitelist, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.AddRequirements(new IpRequirement(Configuration.GetSection("Management:IpWhitelist")));
                });

                options.AddPolicy(Policies.ManagementIpWhitelistAllowAnonymous, policy =>
                    policy.AddRequirements(new IpRequirement(Configuration.GetSection("Management:IpWhitelist"))));
            }).AddSingleton<IAuthorizationHandler, IpHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (Boolean.TryParse(Configuration["Installed"], out bool installed) && installed)
            {
                if (env.IsDevelopment())
                {
                    app.UseBrowserLink();
                    app.UseDeveloperExceptionPage();
                    app.UseDatabaseErrorPage();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }

                app.UseAuthentication();

                app.UseSession();

                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "Company",
                        template: "Resume/Company/{name}/{version}",
                        defaults: new { controller = "Resumes", action = "CompanyResumeLink", name = String.Empty, version = (int?)null });

                    routes.MapRoute(
                        name: "Category",
                        template: "Resume/Category/{name}/{version}",
                        defaults: new { controller = "Resumes", action = "CategoryResumeLink", name = String.Empty, version = (int?)null });

                    routes.MapRoute(
                        name: "PortfolioTitle",
                        template: "Portfolio/Show/{title:alpha}",
                        defaults: new { controller = "Portfolio", action = "Show", id = (int?)null, title = String.Empty });

                    routes.MapRoute(
                        name: "PortfolioId",
                        template: "Portfolio/Show/{id:int?}",
                        defaults: new { controller = "Portfolio", action = "Show", id = (int?)null, title = String.Empty });

                    routes.MapRoute(
                        name: "BlogTitle",
                        template: "Blog/Show/{title:alpha}",
                        defaults: new { controller = "Blog", action = "Show", id = (int?)null, title = String.Empty });

                    routes.MapRoute(
                        name: "BlogId",
                        template: "Blog/Show/{id:int?}",
                        defaults: new { controller = "Blog", action = "Show", id = (int?)null, title = String.Empty });

                    routes.MapRoute(
                        name: "Login",
                        template: "Login",
                        defaults: new { controller = "Account", action = "Login" });

                    routes.MapRoute(
                        name: "default",
                        template: "{controller}/{action}/{id?}",
                        defaults: new { controller = "Home", action = "Index" });
                });
            }
            else
            {
                app.UseMvc(routes =>
                {
                    routes.MapRoute(
                        name: "install",
                        template: "{controller}/{action}/{id?}",
                        defaults: new { controller = "Install", action = "Index" });
                });
            }

            app.UseStaticFiles();
        }
    }

    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddFacebook(this AuthenticationBuilder builder, IConfigurationSection section)
        {
            if (section.Exists())
            {
                builder.AddFacebook(options =>
                {
                    options.AppId = section["AppId"];
                    options.AppSecret = section["AppSecret"];
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddTwitter(this AuthenticationBuilder builder, IConfigurationSection section)
        {
            if (section.Exists())
            {
                builder.AddTwitter(options =>
                {
                    options.ConsumerKey = section["ConsumerKey"];
                    options.ConsumerSecret = section["ConsumerSecret"];
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddGoogle(this AuthenticationBuilder builder, IConfigurationSection section)
        {
            if (section.Exists())
            {
                builder.AddGoogle(options =>
                {
                    options.ClientId = section["ClientId"];
                    options.ClientSecret = section["ClientSecret"];
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddMicrosoftAccount(this AuthenticationBuilder builder, IConfigurationSection section)
        {
            if (section.Exists())
            {
                builder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = section["ClientId"];
                    options.ClientSecret = section["ClientSecret"];
                });
            }

            return builder;
        }

        public static AuthenticationBuilder AddExternalAuthentications(this AuthenticationBuilder builder, IConfigurationSection authSection)
        {
            if (authSection.Exists())
            {
                builder
                    .AddFacebook(authSection.GetSection("Facebook"))
                    .AddTwitter(authSection.GetSection("Twitter"))
                    .AddGoogle(authSection.GetSection("Google"))
                    .AddMicrosoftAccount(authSection.GetSection("Microsoft"));
            }

            return builder;
        }
    }
}
