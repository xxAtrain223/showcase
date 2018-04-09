﻿using System;
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

namespace showcase
{
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

                // User settings
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                // Cookie settings
                options.Cookie.HttpOnly = true;
                options.Cookie.Expiration = TimeSpan.FromDays(150);
                options.LoginPath = "/Account/Login"; // If the LoginPath is not set here, ASP.NET Core will default to /Account/Login
                options.LogoutPath = "/Account/Logout"; // If the LogoutPath is not set here, ASP.NET Core will default to /Account/Logout
                options.AccessDeniedPath = "/Account/AccessDenied"; // If the AccessDeniedPath is not set here, ASP.NET Core will default to /Account/AccessDenied
                options.SlidingExpiration = true;
            });

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();
            
            services.AddUrlHelper();
            
            services.AddMvc()
                .AddSessionStateTempDataProvider();

            services.AddSession();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseStaticFiles();

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
                    name: "default",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });   
        }
    }
}
