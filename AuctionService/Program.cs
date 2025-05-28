
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using System.Threading.RateLimiting;
using AuctionService.DataContextDb;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using AuctionService.Validators;
using AuctionService.DIs.Services;
using AuctionService.DIs.Interfaces;
using Microsoft.Extensions.DependencyInjection;


namespace AuctionService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });

                // Tell Swagger this is an ApiKey in the Header,
                // and update the description so users know NOT to include "Bearer "
                var jwtScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "apiKey",
                    Description = "Enter your JWT token *without* the “Bearer ” prefix",
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                };

                c.AddSecurityDefinition("Bearer", jwtScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    [jwtScheme] = Array.Empty<string>()
                });
            });

            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                //როდესაც დაიჰოსტება True
                opt.RequireHttpsMetadata = false;

                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = config["Jwt:issuer"],
                    ValidAudience = config["Jwt:audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:SecretKey"])),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true
                };

                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });


            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("allowAll", builder =>
                {
                    builder
                        .SetIsOriginAllowedToAllowWildcardSubdomains()
                        .WithOrigins(
                            config["Origins:Client"],
                            config["Origins:Admin"],
                            config["Origins:LiveServerLocalhost"],
                            config["Origins:LiveServer127"]
                        )
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });
            });


            builder.Services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20, // 20 მოთხოვნა წამში
                            Window = TimeSpan.FromSeconds(1), // 1 წამი
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 5 // 5 მოთხოვნა რიგში
                        }));
            });

            // Add services to the container.
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("default"));
            });

            builder.Services.AddSingleton<IEmailSenderService>(new EmailSenderService(config["Smtp:Sender"],config["Smtp:Url"],587,config["Smtp:AppKey"]));

            builder.Services.AddAutoMapper(typeof(Program).Assembly);

            builder.Services.AddValidatorsFromAssemblyContaining<UserValidator>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IItemService, ItemService>();
            builder.Services.AddScoped<IAuctionServices, AuctionServices>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IBidService, BidService>();
            builder.Services.AddHostedService<AuctionLifecycleService>();
            builder.Services.AddScoped<IAuctionNotificationService, AuctionNotificationService>();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateAuctionDtoValidator>();
            builder.Services.AddValidatorsFromAssemblyContaining<BidCreateDtoValidator>();



            var app = builder.Build();


            app.UseCors("allowAll");
            app.UseRateLimiter();
          

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
