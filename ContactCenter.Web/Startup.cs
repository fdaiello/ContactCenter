using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Hosting;
using ContactCenter.Data;
using ContactCenter.Core;
using ContactCenter.Infrastructure.Clients.GsWhatsApp;
using ContactCenter.Infrastructure.Clients.Wassenger;
using ContactCenter.Infrastructure.Clients.MayTapi;
using ContactCenter.Infrastructure.Clients.Speech;
using ContactCenter.Infrastructure.Clients.MpSms;
using ContactCenter.Helpers;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using System.IO;
using System.Linq;
using ContactCenter.Infrastructure.Clients.Wpush;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using ContactCenter.Core.Models;
using ContactCenter.Data.Interfaces;

namespace ContactCenter
{
    public class Startup
    {
        // Configuration
        public IConfiguration Configuration { get; }

        // Logger
        private readonly ILogger _logger;

        public Startup(IWebHostEnvironment env, ILogger<Startup> logger)
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            _logger = logger;

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // CORS - needed for api calls testing
            services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowMyOrigins",
                                  builder =>
                                  {
                                      builder.WithOrigins("https://contact-center.azurewebsites.net/",
                                                          "https://contact-center-stage.azurewebsites.net/",
                                                          "http://localhost:5000",
                                                          "http://localhost:4200");
                                  });
            });

            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Authentication
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<IAccountManager, AccountManager>();

            // DB Creation and Seeding
            services.AddTransient<IDatabaseInitializer, DatabaseInitializer>();

            // MVC Configuration
            services.AddControllersWithViews();

            //  Session Managment
            services.AddSession(options =>
            {
                options.Cookie.Name = ".ContactCenter.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.IsEssential = true;
            });

            // Avoid circular reference errors when serializing at Controllers
            services.AddControllers().AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );

            // DbOptionBuilder - currently used by MayTapi and Wassenger - Import function works on different lifecycle, needs to instantiate context
            var dbOptionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            dbOptionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"),
                    opts => opts.EnableRetryOnFailure().CommandTimeout((int)TimeSpan.FromSeconds(20).TotalSeconds));
            services.AddSingleton(dbOptionsBuilder);

            // Get Gs ( GupShupAPI ) Settings & API
            IConfigurationSection gsSettings = Configuration.GetSection("GsSettings");
            services.Configure<GsWhatsAppSettings>(gsSettings);
            services.AddSingleton<GsWhatsAppClient>();

            // Wassenger ( WhatsApp paralel API ) Settings & API
            IConfigurationSection waSettings = Configuration.GetSection("Wassenger");
            services.Configure<WassengerSettings>(waSettings);
            services.AddSingleton<WassengerClient>();

            // MayTapi ( WhatsApp paralel API ) Settings & API
            IConfigurationSection mtSettings = Configuration.GetSection("MayTapi");
            services.Configure<MayTapiSettings>(mtSettings);
            services.AddSingleton<MayTapiClient>();

            // WebPush Client
            IConfigurationSection wpushsettings = Configuration.GetSection("WebPushR");
            services.Configure<WpushSettings>(wpushsettings);
            services.AddSingleton<WpushClient>();

            // NotifyApi
            services.AddScoped<Notify>();

			// Data Protection - needed for Identity(logins) work on different web app slots
			services.AddDataProtection()
				.SetApplicationName(Configuration["DataProtection:ApplicationName"])
				.PersistKeysToAzureBlobStorage(Configuration.GetValue<string>($"BlobStorageConnStr"), Configuration.GetValue<string>($"DataProtection:KeyStoreContainerName"), Configuration.GetValue<string>($"DataProtection:KeyStoreBlobName"));

			// Create the storage client used to upload media files
			BlobContainerClient blobContainerClient = new BlobContainerClient(Configuration.GetValue<string>($"BlobStorageConnStr"), Configuration.GetValue<string>($"FileContainer"));
            services.AddSingleton(blobContainerClient);

            // Mister Postman SMS - Settings & API
            services.Configure<MpSmsSettings>(options => Configuration.GetSection("MpSms").Bind(options));
            services.AddSingleton<MpSmsClient>();

            // ADD JWT Authentication
            var key = Encoding.ASCII.GetBytes(Configuration.GetValue<string>($"JwtSecret"));
            services.AddAuthentication()
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });


            // Confere se o diretorio bin foi adicionado ao PATH do ambiente: neceesario pro IIS achar o libmp3lame.32.dll
            CheckAddDllPath();

            // Add Static Files - so we will have a route to Angular built 
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });

            // Memory cache will be used to improve Dashboard performance
            services.AddMemoryCache();

            // Speech Client
            IConfigurationSection speechSettings = Configuration.GetSection("Speech");
            services.Configure<SpeechSettings>(speechSettings);
            services.AddSingleton<SpeechClient>();
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Chat/Error");
                app.UseSpaStaticFiles();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Chat}/{action=Index}/{id?}");
            });
            app.UseCors("AllowMyOrigins");
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
        // Confere se o diretorio bin foi adicionado ao PATH do ambiente: neceesario pro IIS achar o libmp3lame.32.dll
        public static void CheckAddDllPath()
        {
            // find path to base directory
            var dllPath = AppDomain.CurrentDomain.BaseDirectory;
            // get current search path from environment
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";

            // add 'bin' folder to search path if not already present
            if (!path.Split(Path.PathSeparator).Contains(dllPath, StringComparer.CurrentCultureIgnoreCase))
            {
                path = string.Join(Path.PathSeparator.ToString(new CultureInfo("en-US")), new string[] { path, dllPath });
                Environment.SetEnvironmentVariable("PATH", path);
            }
        }
    }
}
