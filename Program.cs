using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    new MySqlServerVersion(new Version(8, 0, 21))));

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// JWT Authentication
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
        ValidIssuer = builder.Configuration.GetValue<string>("Jwt:Issuer"),
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Add endpoints API explorer (for Swagger)
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Use Developer Exception Page in Development mode
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Use CORS policy
app.UseCors("AllowAll");

// Use Routing Middleware
app.UseRouting();

// Use Authentication and Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Add Swagger Middleware
app.UseSwagger();

// Use Swagger UI
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "To Do API v1");
    c.RoutePrefix = "swagger";  // Access Swagger UI at /swagger instead of root
});

// Routes
// Route to get all items (only accessible for authorized users)
app.MapGet("/items", [Authorize] async (ToDoDbContext db) => await db.Items.ToListAsync())
    .Produces<List<Item>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// Route to get items by user ID
app.MapGet("/byId", async (ToDoDbContext db, [FromQuery] int id) =>
{
    var items = await db.Items.Where(x => x.UserId == id).ToListAsync();
    if (items.Count == 0) return Results.NotFound();
    return Results.Ok(items);
});

// Route to create a new item
app.MapPost("/items", async (ToDoDbContext db, string name, int id) =>
{
    var item = new Item() { Name = name, UserId = id };
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Id}", item);
});

// Route to update an existing item
app.MapPut("/items/{id}", async (int id, bool IsComplete, ToDoDbContext db) =>
{
    var todo = await db.Items.FindAsync(id);
    if (todo == null) return Results.NotFound();
    todo.IsComplete = IsComplete;
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Route to delete an item
app.MapDelete("/items/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item == null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Route to create a JWT token
object CreateJwt(User user)
{
    var claims = new List<Claim>
    {
        new Claim("name", user.Name),
        new Claim("id", user.IdUsers.ToString())
    };

    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:key")));
    var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

    var tokenOption = new JwtSecurityToken(
        issuer: builder.Configuration.GetValue<string>("Jwt:Issuer"),
        audience: builder.Configuration.GetValue<string>("Jwt:Audience"),
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: signingCredentials);

    return new { Token = new JwtSecurityTokenHandler().WriteToken(tokenOption) };
}

// Login route
app.MapPost("/login", async (ToDoDbContext db, User user) =>
{
    var myUser = db.Users.Where(x => x.IdUsers == user.IdUsers).ToList();
    if (myUser.Count() > 0 && myUser[0].Password == user.Password)
    {
        var jwt = CreateJwt(myUser[0]);
        return Results.Ok(new { jwt, myUser });
    }
    return Results.Unauthorized();
});

// Add User route
app.MapPost("/addUser", async (ToDoDbContext db, User user) =>
{
    var myUser = db.Users.Where(x => x.IdUsers == user.IdUsers);
    if (!myUser.Any())
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        var jwt = CreateJwt(user);
        return Results.Ok(jwt);
    }
    return Results.Unauthorized();
});

// Get all users route
app.MapGet("/users", async (ToDoDbContext db) => await db.Users.ToListAsync());

// Info route
app.MapGet("/info", () => "פרויקט פרקטיקוד 3\nיוצר: מירי שטראוס ");
app.MapGet("/", () => "פרויקט פרקטיקוד 4\nיוצר: מירי שטראוס ");

app.Run();
