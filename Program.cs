using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// הגדרת ה-DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    new MySqlServerVersion(new Version(8, 0, 21))));

// הוספת CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificOrigin",
//         builder => builder.WithOrigins("https://to-do-client1-p1w5.onrender.com")
//                           .AllowAnyMethod()
//                           .AllowAnyHeader());
// });

// הוספת Controllers
builder.Services.AddControllers();

// הוספת Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// JWT token
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

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// שימוש במדיניות CORS
app.UseCors("AllowSpecificOrigin");
app.UseCors("AllowAllOrigins");

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();

// הוספת Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // כדי לגשת ל-Swagger בכתובת הבית
});

// Routes
app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync())
    .Produces<List<Item>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

app.MapGet("/byId", async (ToDoDbContext db, int id) => 
{
    return await db.Items.Where(x => x.UserId == id).ToListAsync();
});

app.MapPost("/", async (ToDoDbContext db, string name, int id) =>
{
    var i = new Item() { Name = name, UserId = id };
    db.Items.Add(i);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{i.Id}", i); // השתנה ל-i.Id
});

app.MapPut("/{id}", async (int id, bool IsComplete, ToDoDbContext db) =>
{
    var todo = await db.Items.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.IsComplete = IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

// Route למחיקת פריט
app.MapDelete("/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// יצירת טוקן
object createJwt(User user)
{
    var claims = new List<Claim>()
    {
        new Claim("name", user.Name),
        new Claim("id", user.IdUsers.ToString()),
        new Claim("password", user.Password),
    };
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:key")));
    var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    var tokenOption = new JwtSecurityToken(
        issuer: builder.Configuration.GetValue<string>("Jwt:Issuer"),
        audience: builder.Configuration.GetValue<string>("Jwt:Audience"),
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: signingCredentials);
    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOption);
    return new { Token = tokenString };
}

// התחברות
app.MapPost("/login", async (ToDoDbContext db, User user) =>
{
    var myUser = await db.Users.Where(x => x.IdUsers == user.IdUsers).ToListAsync(); // השתנה ל-ToListAsync
    if (myUser.Count > 0 && myUser[0].Password == user.Password)
    {
        var jwt = createJwt(myUser[0]);
        return Results.Ok(new { jwt, myUser });
    }
    return Results.Unauthorized();
});

// הרשמה
app.MapPost("/addUser", async (ToDoDbContext db, User user) =>
{
    var myUser = await db.Users.Where(x => x.IdUsers == user.IdUsers).ToListAsync(); // השתנה ל-ToListAsync
    if (myUser.Count == 0)
    {
        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();
        var jwt = createJwt(user);
        return Results.Ok(jwt);
    }
    return Results.Unauthorized();
});

// שליפת המשתמשים
app.MapGet("/users", async (ToDoDbContext db) => await db.Users.ToListAsync()); // השתנה ל-ToListAsync

// מידע על האפליקציה
app.MapGet("/info", () => "פרויקט פרקטיקוד 3\nיוצר: מירי שטראוס ");
app.MapGet("/", () => "פרויקט פרקטיקוד 4\nיוצר: מירי שטראוס ");

app.Run();
