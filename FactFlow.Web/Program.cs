using FactFlow.Application.CatFacts.Commands.FetchCatFact;
using FactFlow.Application.CatFacts.Commands.AddManualFact;
using FactFlow.Application.CatFacts.Commands.DeleteFact;
using FactFlow.Application.CatFacts.Commands.UpdateFact;
using FactFlow.Application.CatFacts.Queries.GetFactById;
using FactFlow.Application.CatFacts.Queries.GetFacts;
using FactFlow.Application.CatFacts.Queries.GetDashboard;
using FactFlow.Application.CatFacts.Queries.GetFactHistory;
using FactFlow.Application.CatFacts.Queries.GetFactJournal;
using FactFlow.Application.Common.Messaging;
using FactFlow.Infrastructure;
using FactFlow.Infrastructure.Data;
using FactFlow.Web.Authentication;
using FactFlow.Web.Workspaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var configuredKeyPath = builder.Configuration["DataProtection:Path"] ?? "App_Data/DataProtectionKeys";
var homePath = Environment.GetEnvironmentVariable("HOME") ?? builder.Environment.ContentRootPath;
var expandedKeyPath = Environment.ExpandEnvironmentVariables(configuredKeyPath)
    .Replace("{HOME}", homePath, StringComparison.OrdinalIgnoreCase);
var keyPath = Path.IsPathRooted(expandedKeyPath)
    ? Path.GetFullPath(expandedKeyPath)
    : Path.GetFullPath(expandedKeyPath, builder.Environment.ContentRootPath);

builder.Services.AddControllersWithViews();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
    .SetApplicationName("FactFlow");
builder.Services.AddTransient<ICommandHandler<FetchCatFactCommand, FetchCatFactResult>, FetchCatFactCommandHandler>();
builder.Services.AddTransient<ICommandHandler<AddManualFactCommand, FactFlow.Domain.CatFacts.FactRecord>, AddManualFactCommandHandler>();
builder.Services.AddTransient<ICommandHandler<UpdateFactCommand, bool>, UpdateFactCommandHandler>();
builder.Services.AddTransient<ICommandHandler<DeleteFactCommand, bool>, DeleteFactCommandHandler>();
builder.Services.AddTransient<IQueryHandler<GetDashboardQuery, DashboardSnapshot>, GetDashboardQueryHandler>();
builder.Services.AddTransient<IQueryHandler<GetFactsQuery, IReadOnlyList<FactListItem>>, GetFactsQueryHandler>();
builder.Services.AddTransient<IQueryHandler<GetFactByIdQuery, FactDetails?>, GetFactByIdQueryHandler>();
builder.Services.AddTransient<IQueryHandler<GetFactHistoryQuery, FactHistorySnapshot>, GetFactHistoryQueryHandler>();
builder.Services.AddTransient<IQueryHandler<GetFactJournalQuery, FactJournalFile?>, GetFactJournalQueryHandler>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<DemoAuthOptions>(builder.Configuration.GetSection(DemoAuthOptions.SectionName));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "FactFlow.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromHours(8);
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IWorkspaceTabService, SessionWorkspaceTabService>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.Cookie.Name = "FactFlow.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapHealthChecks("/health");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    await app.Services.InitializeDatabaseAsync();
}

app.Run();

public partial class Program;
