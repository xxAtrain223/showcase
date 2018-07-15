using Microsoft.Extensions.Logging;
using showcase.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace showcase.Models
{
    public class InstallationViewModel
    {
        [Display(Name = "Connection String")]
        [Required]
        public string ConnectionString { get; set; }
        public LoggingViewModel Logging { get; set; }
        public RecaptchaViewModel Recaptcha { get; set; }
        public ContactViewModel Contact { get; set; }
        public AuthenticationViewModel Authentication { get; set; }
        [Display(Name = "Ip Whitelist")]
        [Required]
        public string IpWhitelist { get; set; }

        public ApplicationSettings ToApplicationSettings()
        {
            return new ApplicationSettings
            {
                ConnectionStrings = new Dictionary<string, string>() { { "DefaultConnection", ConnectionString } },
                Logging = Logging.ToLogging(),
                Recaptcha = Recaptcha.ToRecaptcha(),
                Contact = Contact.ToContact(),
                Authentication = Authentication.ToAuthentication(),
                Management = new Management { IpWhitelist = IpWhitelist.Split(",").Select(s => s.Trim()).ToList() }
            };
        }

        public static InstallationViewModel FromApplicationSettings(ApplicationSettings appSettings)
        {
            if (appSettings == null)
            {
                return null;
            }

            return new InstallationViewModel
            {
                ConnectionString = appSettings.ConnectionStrings["DefaultConnection"],
                Logging = LoggingViewModel.FromLogging(appSettings.Logging),
                IpWhitelist = String.Join(", ", appSettings.Management.IpWhitelist),
                Contact = ContactViewModel.FromContact(appSettings.Contact),
                Recaptcha = RecaptchaViewModel.FromRecaptcha(appSettings.Recaptcha),
                Authentication = AuthenticationViewModel.FromAuthentication(appSettings.Authentication)
            };
            /*
            return new InstallationViewModel
            {
                ConnectionString = appSettings.ConnectionStrings["DefaultConnection"],
                Logging = new LoggingViewModel { IncludeScopes = appSettings.Logging.IncludeScopes,
                    Default = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(appSettings.Logging.LogLevel.Default) },
                IpWhitelist = String.Join(", ", appSettings.Management.IpWhitelist),
                Recaptcha = new RecaptchaViewModel { SiteKey = appSettings.Recaptcha.SiteKey, SecretKey = appSettings.Recaptcha.SecretKey },
                Contact = new ContactViewModel { Name = appSettings.Contact.Name, Email = appSettings.Contact.Email,
                     SmtpServer = new SmtpServerViewModel
                     {
                         Host = appSettings.Contact.SmtpServer.Host,
                         Port = appSettings.Contact.SmtpServer.Port,
                         Username = appSettings.Contact.SmtpServer.Username,
                         Password = appSettings.Contact.SmtpServer.Password
                     }
                },
                Authentication = new AuthenticationViewModel
                {
                    Facebook = new FacebookViewModel
                    {
                        AppId = appSettings.Authentication.Facebook.AppId,
                        AppSecret = appSettings.Authentication.Facebook.AppSecret
                    },
                    Twitter = new TwitterViewModel
                    {
                        ConsumerKey = appSettings.Authentication.Twitter.ConsumerKey,
                        ConsumerSecret = appSettings.Authentication.Twitter.ConsumerSecret
                    },
                    Google = new GoogleViewModel
                    {
                        ClientId = appSettings.Authentication.Google.ClientId,
                        ClientSecret = appSettings.Authentication.Google.ClientSecret
                    },
                    Microsoft = new MicrosoftViewModel
                    {
                        ClientId = appSettings.Authentication.Microsoft.ClientId,
                        ClientSecret = appSettings.Authentication.Microsoft.ClientSecret
                    }
                }
            };
            */
        }
    }
    
    public class LoggingViewModel
    {
        [Display(Name = "Include Scopes")]
        [Required]
        public bool IncludeScopes { get; set; }
        [Display(Name = "Log Level")]
        [Required]
        public Microsoft.Extensions.Logging.LogLevel Default { get; set; }
        
        public Logging ToLogging()
        {
            return new Logging
            {
                IncludeScopes = IncludeScopes,
                LogLevel = new LogLevel { Default = Default.ToString() }
            };
        }

        public static LoggingViewModel FromLogging(Logging logging)
        {
            if (logging == null)
            {
                return null;
            }

            return new LoggingViewModel
            {
                IncludeScopes = logging.IncludeScopes,
                Default = Enum.Parse<Microsoft.Extensions.Logging.LogLevel>(logging.LogLevel?.Default ?? "Warning")
            };
        }
    }
    
    public class RecaptchaViewModel
    {
        [Display(Name = "Site Key")]
        [Required]
        public string SiteKey { get; set; }
        [Display(Name = "Secret Key")]
        [Required]
        public string SecretKey { get; set; }

        public Recaptcha ToRecaptcha()
        {
            return new Recaptcha
            {
                SiteKey = SiteKey,
                SecretKey = SecretKey
            };
        }

        public static RecaptchaViewModel FromRecaptcha(Recaptcha recaptcha)
        {
            if (recaptcha == null)
            {
                return null;
            }

            return new RecaptchaViewModel
            {
                SiteKey = recaptcha.SiteKey,
                SecretKey = recaptcha.SecretKey
            };
        }
    }

    public class ContactViewModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public SmtpServerViewModel SmtpServer { get; set; }

        public Contact ToContact()
        {
            return new Contact
            {
                Name = Name,
                Email = Email,
                SmtpServer = SmtpServer.ToSmtpServer()
            };
        }

        public static ContactViewModel FromContact(Contact contact)
        {
            return new ContactViewModel
            {
                Name = contact.Name,
                Email = contact.Email,
                SmtpServer = SmtpServerViewModel.FromSmtpServer(contact.SmtpServer)
            };
        }
    }

    public class SmtpServerViewModel
    {
        [RequiredIf("Port.HasValue || !String.IsNullOrWhiteSpace(Username) || !String.IsNullOrWhiteSpace(Password)", typeof(String))]
        public string Host { get; set; }
        [RequiredIf("!String.IsNullOrWhiteSpace(Host) || !String.IsNullOrWhiteSpace(Username) || !String.IsNullOrWhiteSpace(Password)", typeof(String))]
        public int? Port { get; set; }
        [RequiredIf("!String.IsNullOrWhiteSpace(Host) || Port.HasValue || !String.IsNullOrWhiteSpace(Password)", typeof(String))]
        public string Username { get; set; }
        [RequiredIf("!String.IsNullOrWhiteSpace(Host) || Port.HasValue || !String.IsNullOrWhiteSpace(Username)", typeof(String))]
        public string Password { get; set; }

        public SmtpServer ToSmtpServer()
        {
            if (Host != null || Port != null || Username != null || Password != null)
            {
                return new SmtpServer
                {
                    Host = Host,
                    Port = Port,
                    Username = Username,
                    Password = Password
                };
            }

            return null;
        }

        public static SmtpServerViewModel FromSmtpServer(SmtpServer smtpServer)
        {
            if (smtpServer == null)
            {
                return null;
            }

            return new SmtpServerViewModel
            {
                Host = smtpServer.Host,
                Port = smtpServer.Port,
                Username = smtpServer.Username,
                Password = smtpServer.Password
            };
        }
    }

    public class AuthenticationViewModel
    {
        public FacebookViewModel Facebook { get; set; }
        public TwitterViewModel Twitter { get; set; }
        public GoogleViewModel Google { get; set; }
        public MicrosoftViewModel Microsoft { get; set; }

        public Authentication ToAuthentication()
        {
            Authentication authentication = new Authentication
            {
                Facebook = Facebook.ToFacebook(),
                Twitter = Twitter.ToTwitter(),
                Google = Google.ToGoogle(),
                Microsoft = Microsoft.ToMicrosoft()
            };

            if (authentication.Facebook != null ||
                authentication.Twitter != null ||
                authentication.Google != null ||
                authentication.Microsoft != null)
            {
                return authentication;
            }

            return null;
        }

        public static AuthenticationViewModel FromAuthentication(Authentication authentication)
        {
            if (authentication == null)
            {
                return null;
            }

            return new AuthenticationViewModel
            {
                Facebook = FacebookViewModel.FromFacebook(authentication.Facebook),
                Twitter = TwitterViewModel.FromTwitter(authentication.Twitter),
                Google = GoogleViewModel.FromGoogle(authentication.Google),
                Microsoft = MicrosoftViewModel.FromMicrosoft(authentication.Microsoft)
            };
        }
    }

    public class FacebookViewModel
    {
        [Display(Name = "App Id")]
        [RequiredIf("!String.IsNullOrWhiteSpace(AppSecret)", typeof(String))]
        public string AppId { get; set; }
        [Display(Name = "App Secret")]
        [RequiredIf("!String.IsNullOrWhiteSpace(AppId)", typeof(String))]
        public string AppSecret { get; set; }
        
        public FacebookAuth ToFacebook()
        {
            if (AppId != null || AppSecret != null)
            {
                return new FacebookAuth
                {
                    AppId = AppId,
                    AppSecret = AppSecret
                };
            }

            return null;
        }

        public static FacebookViewModel FromFacebook(FacebookAuth facebook)
        {
            if (facebook == null)
            {
                return null;
            }

            return new FacebookViewModel
            {
                AppId = facebook.AppId,
                AppSecret = facebook.AppSecret
            };
        }
    }

    public class TwitterViewModel
    {
        [Display(Name = "Consumer Key")]
        [RequiredIf("!String.IsNullOrWhiteSpace(ConsumerSecret)", typeof(String))]
        public string ConsumerKey { get; set; }
        [Display(Name = "Consumer Secret")]
        [RequiredIf("!String.IsNullOrWhiteSpace(ConsumerKey)", typeof(String))]
        public string ConsumerSecret { get; set; }

        public TwitterAuth ToTwitter()
        {
            if (ConsumerKey != null || ConsumerSecret != null)
            {
                return new TwitterAuth
                {
                    ConsumerKey = ConsumerKey,
                    ConsumerSecret = ConsumerSecret
                };
            }

            return null;
        }

        public static TwitterViewModel FromTwitter(TwitterAuth twitter)
        {
            if (twitter == null)
            {
                return null;
            }

            return new TwitterViewModel
            {
                ConsumerKey = twitter.ConsumerKey,
                ConsumerSecret = twitter.ConsumerSecret
            };
        }
    }
    
    public class GoogleViewModel
    {
        [Display(Name = "Client Id")]
        [RequiredIf("!String.IsNullOrWhiteSpace(ClientSecret)", typeof(String))]
        public string ClientId { get; set; }
        [Display(Name = "Client Secret")]
        [RequiredIf("!String.IsNullOrWhiteSpace(ClientId)", typeof(String))]
        public string ClientSecret { get; set; }

        public GoogleAuth ToGoogle()
        {
            if (ClientId != null || ClientSecret != null)
            {
                return new GoogleAuth
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                };
            }

            return null;
        }

        public static GoogleViewModel FromGoogle(GoogleAuth google)
        {
            if (google == null)
            {
                return null;
            }

            return new GoogleViewModel
            {
                ClientId = google.ClientId,
                ClientSecret = google.ClientSecret
            };
        }
    }

    public class MicrosoftViewModel
    {
        [Display(Name = "Client Id")]
        [RequiredIf("!String.IsNullOrWhiteSpace(ClientSecret)", typeof(String))]
        public string ClientId { get; set; }
        [Display(Name = "Client Secret")]
        [RequiredIf("!String.IsNullOrWhiteSpace(ClientId)", typeof(String))]
        public string ClientSecret { get; set; }

        public MicrosoftAuth ToMicrosoft()
        {
            if (ClientId != null || ClientSecret != null)
            {
                return new MicrosoftAuth
                {
                    ClientId = ClientId,
                    ClientSecret = ClientSecret
                };
            }

            return null;
        }

        public static MicrosoftViewModel FromMicrosoft(MicrosoftAuth microsoft)
        {
            if (microsoft == null)
            {
                return null;
            }

            return new MicrosoftViewModel
            {
                ClientId = microsoft.ClientId,
                ClientSecret = microsoft.ClientSecret
            };
        }
    }
}
