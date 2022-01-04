using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LandonApiAuth.Filters;
using LandonApiAuth.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using LandonApiAuth.Services;
using LandonApiAuth.Infrastructure;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using AspNet.Security.OpenIdConnect.Primitives;
using OpenIddict.Validation;
using Newtonsoft.Json;

namespace LandonApiAuth
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

            services.Configure<HotelInfo>(Configuration.GetSection("Info"));
            services.Configure<HotelOptions>(Configuration);
            services.Configure<PagingOptions>(Configuration.GetSection("DefaultPagingOptions"));
            

            services.AddScoped<IRoomService, DefaultRoomService>();
            services.AddScoped<IOpeningService, DefaultOpeningService>();
            services.AddScoped<IBookingService, DefaultBookingService>();
            services.AddScoped<IDateLogicService, DefaultDateLogicService>();
            services.AddScoped<IUserService, DefaultUserService>();

            services.AddDbContext<HotelApiDbContext>(options => {
                var connectionString = Configuration.GetSection("ConnectionString").Value;
                options.UseSqlServer(connectionString);
                options.UseOpenIddict<Guid>();
            });

            /*
            services.AddDbContext<HotelApiDbContext>(
                options => options.UseInMemoryDatabase("landondb"));
            */

            // Add OpenIddict services
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<HotelApiDbContext>()
                        .ReplaceDefaultEntities<Guid>();
                })
                .AddServer(options =>
                {
                    options.UseMvc();

                    options.SetTokenEndpointUris("/token");//EnableTokenEndpoint("/token");

                    options.AllowPasswordFlow();
                    options.AcceptAnonymousClients();
                })
                .AddValidation();

            services.Configure<IdentityOptions>(options =>
            {
                
                options.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;
                
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });

            // Add ASP.NET Core Identity
            AddIdentityCoreServices(services);

            services.TryAddSingleton<ISystemClock, SystemClock>();

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddMvc(options =>
            {
                options.CacheProfiles.Add("Static", new CacheProfile
                {
                    Duration = 86400
                });
                options.Filters.Add<JsonExceptionFilter>();
                options.Filters.Add<RequireHttpsOrCloseAttribute>();
                options.Filters.Add<LinkRewritingFilter>();
                options.EnableEndpointRouting = false;
            });
                /*
                 * .AddJsonOptions(options =>
            {
                // These should be the defaults, but we can be explicit:
                options.JsonSerializerOptions.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;

            });
                */
            //services.AddSwaggerDocument();

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddSwaggerDocument(config =>
            {
                config.PostProcess = document =>
                {
                    document.Info.Version = "v1";
                    document.Info.Title = "ToDo API";
                    document.Info.Description = "A simple ASP.NET Core web API";
                    document.Info.TermsOfService = "None";
                    document.Info.Contact = new NSwag.OpenApiContact
                    {
                        Name = "Shayne Boyer",
                        Email = string.Empty,
                        Url = "https://twitter.com/spboyer"
                    };
                    document.Info.License = new NSwag.OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = "https://example.com/license"
                    };
                };
            });

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader
                    = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector
                     = new CurrentImplementationApiVersionSelector(options);
            });


            services.AddCors(options =>
            {
                options.AddPolicy("AllowMyApp",
                     policy => policy//.WithOrigins("https://example.com")
                         .AllowAnyOrigin()); // 
            });

            services.AddAutoMapper( options => options.AddProfile<MappingProfile>());

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errorResponse = new ApiError(context.ModelState);
                    return new BadRequestObjectResult(errorResponse);
                };
            });

            services.AddResponseCaching();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseOpenApi();
                app.UseSwaggerUi3();

                /*
                app.UseSwaggerUi3WithApiExplorer(options =>
                {
                    options.GeneratorSettings
                        .DefaultPropertyNameHandling
                    = NJsonSchema.PropertyNameHandling.CamelCase;
                });
                */
            }
            else
            {
                app.UseHsts();
            }

            app.UseAuthentication();

            app.UseResponseCaching();

            app.UseCors("AllowMyApp");

            app.UseMvc();

            app.UseRouting();

            /*
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            */
        }

        private static void AddIdentityCoreServices(IServiceCollection services)
        {
            var builder = services.AddIdentityCore<UserEntity>();
            builder = new IdentityBuilder(
                builder.UserType,
                typeof(UserRoleEntity),
                builder.Services);

            builder.AddRoles<UserRoleEntity>()
                .AddEntityFrameworkStores<HotelApiDbContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager<SignInManager<UserEntity>>();
        }
    }
}
