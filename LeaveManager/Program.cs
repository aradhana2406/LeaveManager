using LeaveManager.Data;
using LeaveManager.Features.EmployeeManagement.Commands.ApplyLeave;
using LeaveManager.Features.EmployeeManagement.Commands.UploadExistingEmployeesExcel;
using LeaveManager.Features.EmployeeManagement.Commands.UploadLeaveBalanceExcel;
using LeaveManager.Features.EmployeeManagement.Services;
using LeaveManager.Features.Leave.Commands.ApproveRejectLeave;
using LeaveManager.Infrastructure.Notifications;
using LeaveManager.Infrastructure.Token;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace LeaveManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var useInMemoryDatabase = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                if (useInMemoryDatabase)
                {
                    options.UseInMemoryDatabase("LeaveManagerDev");
                }
                else
                {
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"));
                }
            });


            builder.Services.AddScoped<ApplyLeaveHandler>();
            builder.Services.AddScoped<ApproveLeaveHandler>();
            builder.Services.AddScoped<UploadExistingEmployeesExcelHandler>();
            builder.Services.AddScoped<UploadLeaveBalanceExcelHandler>();
            builder.Services.AddScoped<IEmailService, GmailEmailService>();
            builder.Services.AddScoped<ILeaveAccrualService, LeaveAccrualService>();
            builder.Services.AddScoped<IExcelLeaveBalanceParser, ExcelLeaveBalanceParser>();
            builder.Services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
            builder.Services.AddScoped<IExistingEmployeeExcelParser, ExistingEmployeeExcelParser>();
            builder.Services.AddScoped<IOnboardingFileStorageService, OnboardingFileStorageService>();
            builder.Services.AddScoped<IApprovalTokenService, ApprovalTokenService>();

            var app = builder.Build();
            // Seed database on startup
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (dbContext.Database.IsRelational())
                {
                    dbContext.Database.Migrate();
                }
                else
                {
                    dbContext.Database.EnsureCreated();
                }
                EmployeeSeeder.Seed(dbContext);
            }


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
