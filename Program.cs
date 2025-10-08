using cp5_d.Data;
using cp5_d.Models;
using cp5_d.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// EF Core SQL Server (usa a chave "SqlServer"; fallback para "DefaultConnection" se existir)
var csSqlServer = builder.Configuration.GetConnectionString("SqlServer")
    ?? builder.Configuration["ConnectionStrings:SqlServer"];
var csDefault = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];
// Fallback extras para Azure App Service quando o tipo da connection string é configurado errado
var csEnv =
    Environment.GetEnvironmentVariable("ConnectionStrings__SqlServer")
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
    ?? Environment.GetEnvironmentVariable("SQLCONNSTR_SqlServer")
    ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
    ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_SqlServer")
    ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
var connectionString = csSqlServer
    ?? csDefault
    ?? csEnv
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=DimDimDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sql =>
        {
            // Resiliência para intermitências no Azure SQL
            sql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
        }
    ));

builder.Services.AddScoped<IRepository<Loja>, EfRepository<Loja>>();
builder.Services.AddScoped<IRepository<Carro>, EfRepository<Carro>>();
builder.Services.AddScoped<IRepository<Vendedor>, EfRepository<Vendedor>>();

var app = builder.Build();

// Aplicar migrações em runtime com proteção para não derrubar o processo no App Service
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    // Log informativo sem expor segredo da connection string
    try
    {
        string keyUsed = csSqlServer is not null ? "SqlServer"
            : (csDefault is not null ? "DefaultConnection"
            : (csEnv is not null ? "EnvVar(Azure)" : "LocalDbFallback"));
        string Redact(string? cs) => cs is null ? "<null>" : Regex.Replace(cs, "(?i)(Password|Pwd)=[^;]*", "$1=***");
        logger.LogInformation("Connection string key usada: {Key}. Valor (redigido): {Conn}", keyUsed, Redact(connectionString));
    }
    catch { }
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Migrations aplicadas com sucesso no banco configurado.");

        // Opcional: semear dados iniciais para facilitar validação/CRUD
        try
        {
            var lojasRepo = scope.ServiceProvider.GetRequiredService<IRepository<Loja>>();
            var carrosRepo = scope.ServiceProvider.GetRequiredService<IRepository<Carro>>();
            var vendedoresRepo = scope.ServiceProvider.GetRequiredService<IRepository<Vendedor>>();
            SeedData.Seed(lojasRepo, carrosRepo, vendedoresRepo);
            logger.LogInformation("Seed de dados executado (se necessário).");
        }
        catch (Exception seedEx)
        {
            logger.LogWarning(seedEx, "Seed de dados falhou (não bloqueia o startup).");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Falha ao aplicar migrations no startup. Verifique a connection string 'SqlServer' e as regras de firewall do Azure SQL.");
        // Não relança para permitir que o site suba e possamos inspecionar logs (Log Stream / AI)
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Endpoint de diagnóstico simples para verificar conectividade com o banco
app.MapGet("/health/db", async (ApplicationDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "ok", source = "sql" })
                          : Results.Problem(title: "DB unreachable", statusCode: 500);
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "DB connection failed", detail: ex.Message, statusCode: 500);
    }
});

app.Run();
