
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;


// using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// הגדרת ה-DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
    new MySqlServerVersion(new Version(8, 0, 21))));

// הוספת CORS
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

// הוספת Controllers
builder.Services.AddControllers();

// הוספת Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

//JWT token
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
        ValidIssuer =builder.Configuration.GetValue<string>("Jwt:Issuer"),
        ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});


builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}



// שימוש במדיניות CORS
app.UseCors("AllowAll");

app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
// הוספת Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "To do ");
    c.RoutePrefix = string.Empty; // כדי לגשת ל-Swagger בכתובת הבית
});

// Routes
// [Route("api/[controller]")]
    // [ApiController]

// app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync());
app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync())
    .Produces<List<Item>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

//שליפה ע"פ מזהה של משתמש
app.MapGet("/byId",   async (ToDoDbContext db, int id) => 
{
    return await db.Items.Where(x => x.UserId == id).ToListAsync();
});

app.MapPost("/", async (ToDoDbContext db,string name,int id) =>
{
    var i=new Item(){Name=name,UserId=id};
    db.Items.Add(i);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{id}", i);
});

app.MapPut("/{id}", async (int id, bool IsComplete, ToDoDbContext db) =>
{
    var todo = await db.Items.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.IsComplete = IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
     });

// // Route למחיקת פריט
app.MapDelete("/{id}", async (int id, ToDoDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.NoContent();
});



//יצירת טוקן
   object createJwt(User user)
{
    var claims = new List<Claim>()
    {
        new Claim("name", user.Name),
                new Claim("id", user.IdUsers.ToString()),
                new Claim("password", user.Password),
    };
    var secretKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:key")));
    var siginicCredentails=new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256);
    var tokenOption=new JwtSecurityToken(
        issuer: builder.Configuration.GetValue<string>("Jwt:Issuer"),
        audience:builder.Configuration.GetValue<string>("Jwt:Audience"),
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: siginicCredentails);
        var tokenString=new JwtSecurityTokenHandler().WriteToken(tokenOption);
    return new {Token=tokenString};
}
//התחברות
app.MapPost("/login", async (ToDoDbContext db, User user) =>
{
 var myUser= db.Users.Where(x=>x.IdUsers == user.IdUsers).ToList();
 System.Console.WriteLine(myUser[0].IdUsers);
if (myUser.Count()>0 && myUser[0].Password == user.Password){
    var  jwt= createJwt(myUser[0]);
    return Results.Ok(new {jwt,myUser});
 }
 //--שגיאת 401-----
return Results.Unauthorized();
});
//ללא אטריביוט של טוקן
//הרשמה

app.MapPost("/addUser",async (ToDoDbContext db, User user) =>
{
var myUser= db.Users.Where(x=>x.IdUsers == user.IdUsers);
if (myUser.Count()==0){
        System.Console.WriteLine("-------------------");

    System.Console.WriteLine(user.Name);
    await db.Users.AddAsync(user);
    await db.SaveChangesAsync();
    var jwt= createJwt(user);
    return Results.Ok(jwt);
}
//--שגיאת 401-----
return Results.Unauthorized();
});



//שליפת המשתמשים
app.MapGet("/users", (ToDoDbContext db) => db.Users.ToListAsync());
//מידע על האפליקציה- אמור ליהות אמיתי
app.MapGet("/info", () => "פרויקט פרקטיקוד 3\nיוצר: מירי שטראוס ");



app.Run();

