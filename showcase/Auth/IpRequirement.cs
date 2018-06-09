using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace showcase.Auth
{
    public class IpRequirement : IAuthorizationRequirement
    {
        public List<string> IpAddresses { get; private set; }

        public IpRequirement(List<string> ipAddresses)
        {
            IpAddresses = ipAddresses ?? new List<string>();
        }

        public IpRequirement(IConfigurationSection section)
        {
            IpAddresses = section.Get<List<string>>() ?? new List<string>();
        }
    }

    public class IpHandler : AuthorizationHandler<IpRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, IpRequirement requirement)
        {
            if (context.Resource is AuthorizationFilterContext mvcContext)
            {
                var filters = mvcContext.Filters.OfType<AuthorizeFilter>();

                var remoteIpAddress = mvcContext.HttpContext.Request.HttpContext
                    .Connection.RemoteIpAddress.MapToIPv4().ToString();

                foreach (string whitelistIp in requirement.IpAddresses)
                {
                    if (Regex.IsMatch(remoteIpAddress, whitelistIp.Replace(".", @"\.").Replace("*", ".*")))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                }
            }
            
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
