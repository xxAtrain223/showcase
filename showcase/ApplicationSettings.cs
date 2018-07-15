using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace showcase
{
    public class ApplicationSettings
    {
        public IConfiguration Configuration { get; }
        public Dictionary<string, string> ConnectionStrings { get; set; }
        public Recaptcha Recaptcha { get; set; }
        public Contact Contact { get; set; }
        public Authentication Authentication { get; set; }
        public Management Management { get; set; }
        public Logging Logging { get; set; }
        public bool Installed { get; set; }
        
        public ApplicationSettings(IConfiguration configuration = null)
        {
            Configuration = configuration;
            Configuration?.Bind(this);
        }
    }

    public class Recaptcha
    {
        public string SiteKey { get; set; }
        public string SecretKey { get; set; }
    }

    public class Contact
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public SmtpServer SmtpServer { get; set; }
    }

    public class SmtpServer
    {
        public string Host { get; set; }
        public int? Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class Authentication
    {
        public FacebookAuth Facebook { get; set; }
        public TwitterAuth Twitter { get; set; }
        public GoogleAuth Google { get; set; }
        public MicrosoftAuth Microsoft { get; set; }
    }

    public class FacebookAuth
    {
        public string AppId { get; set; }
        public string AppSecret { get; set; }
    }

    public class TwitterAuth
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
    }

    public class GoogleAuth
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class MicrosoftAuth
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class Management
    {
        public List<string> IpWhitelist { get; set; }
    }

    public class Logging
    {
        public bool IncludeScopes { get; set; }
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
    }
}
