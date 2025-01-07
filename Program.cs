using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using pknow_backend.Controllers;
using System.Text;

namespace pknow_backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var builder = WebApplication.CreateBuilder(args); builder.WebHost.ConfigureKestrel(serverOptions =>
            //{
            //    serverOptions.Limits.MaxRequestBodySize = 2147483648; // Maksimum 2 GB
            //});


            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 2147483648; // Maksimum 2 GB
            });

            builder.Services.AddDistributedMemoryCache(); // Dibutuhkan untuk session
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Session berlaku selama 30 menit
                options.Cookie.HttpOnly = true; // Cookie hanya dapat diakses melalui HTTP
                options.Cookie.IsEssential = true; // Penting untuk GDPR
            });




            //builder.Services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.IsEssential = true;
            //});

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policyBuilder =>
                    {
                        policyBuilder.WithOrigins("http://localhost:5173") // URL frontend
                                     .AllowAnyHeader()
                                     .AllowAnyMethod()
                                     .AllowCredentials(); // Memungkinkan pengiriman cookie
                    });
            });


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Key:jwtIssuer"],
                        ValidAudience = builder.Configuration["Key:jwtAudience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Key:jwtKey"]!))
                    };
                });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<CaptchaService>();



            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            //app.Use(async (context, next) =>
            //{
            //    var maxRequestSizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            //    if (maxRequestSizeFeature != null && context.Request.ContentLength > 262_144_000) // 250 MB
            //    {
            //        maxRequestSizeFeature.MaxRequestBodySize = 262_144_000; // 250 MB
            //    }
            //    await next();
            //});

            app.Use(async (context, next) =>
            {
                var maxRequestSizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
                if (maxRequestSizeFeature != null && context.Request.ContentLength > 262_144_000) // 250 MB
                {
                    maxRequestSizeFeature.MaxRequestBodySize = 262_144_000; // 250 MB
                }
                await next();
            });

            app.UseCors("AllowSpecificOrigin");
           
            app.UseRouting();
            Console.WriteLine("Session middleware activated...");
            app.UseSession();
            Console.WriteLine("Session middleware completed...");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
