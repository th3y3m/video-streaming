using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using StreamingService.Services;

namespace StreamingService.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.EnableAnnotations();
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 1000 * 1024 * 1024;
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 1000 * 1024 * 1024;
            });

            builder.Services.AddScoped<VimeoService>();
            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10L * 1024 * 1024 * 1024;  // 10GB
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024;  // 10GB
            });
            builder.Services.AddScoped<CloudinaryService>();
            builder.Services.AddScoped<YoutubeService>();
            builder.Services.AddScoped<VirusTotalService>();
            builder.Services.AddScoped<ClamAVService>();
            builder.Services.AddScoped<MuxService>();
            builder.Services.AddScoped<HybridAnalysisService>();
            builder.Services.AddScoped<WindowsDefenderService>();

            builder.Services.AddHttpClient<MuxService>()
    .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.mux.com/"));


            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    policy =>
                    {
                        policy.WithOrigins(
                            "https://localhost:53427"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                    });
            });

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseCors("AllowSpecificOrigin");

            app.MapControllers();

            app.MapFallbackToFile("/index.html");

            app.Run();
        }
    }
}
