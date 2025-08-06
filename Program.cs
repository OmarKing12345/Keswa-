using Kesawa_Data_Access.Data;
using Kesawa_Data_Access.Repository;
using Kesawa_Data_Access.Repository.IRepository;
using Keswa_Entities.Mapping;
using Keswa_Entities.Models;
using Keswa_Project.Hubs;
using Keswa_Untilities;
using Keswa_Untilities.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Globalization;
using System.Text;

namespace keswa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ✅ CORS Policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                        "http://127.0.0.1:5500", // لو بتفتح من live server
                        "http://127.0.0.1:5501"  // أو بورت تاني
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // لو هتستخدم كوكيز أو login
                });
            });

            // ✅ Add services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSignalR(); // ✅ لازم تضيف دي
            //Radis
            builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost"));

            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();


            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            const string defaultCulture = "en";
            var supportedCultures = new[]
            {
                new CultureInfo(defaultCulture),
                new CultureInfo("ar")
            };
            builder.Services.Configure<RequestLocalizationOptions>(options => {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // ✅ Cookie Authentication for Identity
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None; // Important for cross-origin
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Important for HTTPS
                options.LoginPath = "/api/Account/Login";
                options.LogoutPath = "/api/Account/SignOut";
            });

            // ✅ Authentication (JWT + Google + Facebook)
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = "https://localhost:7061/",
                    ValidAudience = "http://localhost:4200/",
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("eqlkjrljlejrljljghhljflwqlfhuhfuhougoivsldjckklzlkjvlsajkvhjkqevkjeqkjfvukqlv")
                    ),
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGoogle("google", opt =>
            {
                var googleAuth = builder.Configuration.GetSection("Authentication:Google");
                opt.ClientId = googleAuth["ClientId"];
                opt.ClientSecret = googleAuth["ClientSecret"];
                opt.SignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddFacebook(options =>
            {
                options.AppId = builder.Configuration["Authentication:Facebook:AppId"];
                options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
            });

            // ✅ Repositories
            builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            builder.Services.AddScoped<IBrandRepository, BrandRepository>();
            builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
            builder.Services.AddScoped<IProductOrderRepository, ProductOrderRepository>();
            builder.Services.AddScoped<IProductCartRepository, ProductCartRepository>();
            builder.Services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
            builder.Services.AddScoped<ICartService, CartService>();

            builder.Services.AddAutoMapper(typeof(MappingProfile));
            var app = builder.Build();
            //stripe
            var stripeSettings = builder.Configuration.GetSection("Stripe");
            Stripe.StripeConfiguration.ApiKey = stripeSettings["SecretKey"];
            // ✅ Development tools
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseStaticFiles(); // 👈 Enable serving static files
            }

            app.UseHttpsRedirection();

            // ✅ Apply localization middleware
            var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            // ✅ Order of middleware is important
            app.UseRouting();
            app.UseCors("AllowFrontend");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapHub<SupportHub>("/supporthub"); // Use lowercase to match frontend
            app.MapControllers();
            app.Run();
        }
    }
}