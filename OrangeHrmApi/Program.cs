using Microsoft.EntityFrameworkCore;
using OrangeHrmApi.Configuration;
using OrangeHrmApi.Data;
using OrangeHrmApi.Services;
using Serilog;

namespace OrangeHrmApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<EmployeeContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.Configure<OrangeHrmSettings>(builder.Configuration.GetSection("OrangeHRM"));
            builder.Services.AddScoped<IOrangeHrmService, OrangeHrmService>();
            builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<EmployeeContext>();
                context.Database.EnsureCreated();
            }

            app.Run();

        }
    }
}
