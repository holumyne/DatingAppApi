using DatingAppApi.Data;
using DatingAppApi.Entities;
using DatingAppApi.Extensions;
using DatingAppApi.Middleware;
using DatingAppApi.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddDbContext<DataContext>(opt =>
//{
//    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});
//builder.Services.AddCors();
//builder.Services.AddScoped<ITokenService, TokenService>();

builder.Services.AddApplicationSevices(builder.Configuration);

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenKey"])),
//            ValidateIssuer = false,
//            ValidateAudience = false,
//        };
//    });

builder.Services.AddIdentityServices(builder.Configuration);

var connString = "";
if (builder.Environment.IsDevelopment())
    connString = builder.Configuration.GetConnectionString("DefaultConnection");
else
{
    // Use connection string provided at runtime by fly io.
    var connUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

    // Parse connection URL to connection string for Npgsql
    connUrl = connUrl.Replace("postgres://", string.Empty);

    var pgUserPass = connUrl.Split("@")[0];
    var pgHostPortDb = connUrl.Split("@")[1];
    var pgHostPort = pgHostPortDb.Split("/")[0];
    var pgDb = pgHostPortDb.Split("/")[1];
    var pgUser = pgUserPass.Split(":")[0];
    var pgPass = pgUserPass.Split(":")[1];
    var pgHost = pgHostPort.Split(":")[0];
    var pgPort = pgHostPort.Split(":")[1];

    connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
}
builder.Services.AddDbContext<DataContext>(opt =>
{
    opt.UseNpgsql(connString);
});


//builder.Services.AddDbContext<DataContext>(options =>
//{
//    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

//    string connStr;

//    if (env == "Development")
//    {
//        connStr = builder.Configuration.GetConnectionString("DefaultConnection"); // DefaultConnection string must match appsettings.Development.json
//    }
//    else
//    {
//        var connUrl = Environment.GetEnvironmentVariable("HEROKU_POSTGRESQL_SILVER_URL");

//        connUrl = connUrl.Replace("postgres://", string.Empty);

//        var pgUserPass = connUrl.Split("@")[0];
//        var pgHostPortDb = connUrl.Split("@")[1];
//        var pgHostPort = pgHostPortDb.Split("/")[0];
//        var pgDb = pgHostPortDb.Split("/")[1];
//        var pgUser = pgUserPass.Split(":")[0];
//        var pgPass = pgUserPass.Split(":")[1];
//        var pgHost = pgHostPort.Split(":")[0];
//        var pgPort = pgHostPort.Split(":")[1];

//        connString = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};sslmode=Prefer;Trust Server Certificate=true";
//        connStr = $"Server={pgHost};Port={pgPort};User Id={pgUser};Password={pgPass};Database={pgDb};";
//    }

//    options.UseNpgsql(connStr);
//});


builder.Services.AddHealthChecks();
var app = builder.Build();
app.MapHealthChecks("/health");

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder
.AllowAnyHeader()
.AllowAnyMethod()
.AllowCredentials()  //this is included becos of signalR
.WithOrigins("https://localhost:4200"));

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//used to serve static files
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
    await context.Database.MigrateAsync();
    // this will clear(TRUNCATE) our Connections table when we restart our app
    // await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [Connections]");
    // await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Connections\""); // done like this becos of postgres db.The db does not accept ] 
    await Seed.ClearConnections(context); //this is more professional than the above one
    await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = services.GetService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();
