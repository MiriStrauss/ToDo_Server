
using Microsoft.EntityFrameworkCore;
using TodoApi;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;


// using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// // הגדרת ה-DbContext
// builder.Services.AddDbContext<ToDoDbContext>(options =>
//     options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), 
//     new MySqlServerVersion(new Version(8, 0, 21))));
// builder.Services.AddOpenApi();

// // הוספת CORS
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAllOrigins",
//         builder =>
//         {
//             builder.AllowAnyOrigin()
//                    .AllowAnyMethod()
//                    .AllowAnyHeader();
//         });
// });

// // הוספת Controllers
// builder.Services.AddControllers();

// // הוספת Swagger
// builder.Services.AddSwaggerGen(c =>
// {
//     c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
// });

// //JWT token
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//  })
//  .AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new TokenValidationParameters
//     {
//         ValidateIssuer = true,
//         ValidateAudience = true,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         ValidIssuer =builder.Configuration.GetValue<string>("Jwt:Issuer"),
//         ValidAudience = builder.Configuration.GetValue<string>("Jwt:Audience"),
//         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//     };
// });


// builder.Services.AddEndpointsApiExplorer();
// // builder.Services.AddSwaggerGen();

// var app = builder.Build();

// if (app.Environment.IsDevelopment())
// {
//     app.UseDeveloperExceptionPage();
// }



// // שימוש במדיניות CORS
// app.UseCors("AllowAll");

// app.UseRouting();
// app.UseAuthentication(); 
// app.UseAuthorization();
// app.MapControllers();
// // הוספת Swagger
// app.UseSwagger();
// // app.UseSwaggerUI();
// app.UseSwaggerUI(c =>
// {
//     c.SwaggerEndpoint("/swagger/v1/swagger.json", "To do ");
//     c.RoutePrefix = string.Empty; // כדי לגשת ל-Swagger בכתובת הבית
// });

// // Routes
// // [Route("api/[controller]")]
//     // [ApiController]

// // app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync());
// app.MapGet("/items", async (ToDoDbContext db) => await db.Items.ToListAsync())
//     .Produces<List<Item>>(StatusCodes.Status200OK)
//     .Produces(StatusCodes.Status404NotFound);

// //שליפה ע"פ מזהה של משתמש
// app.MapGet("/byId",   async (ToDoDbContext db, int id) => 
// {
//     return await db.Items.Where(x => x.UserId == id).ToListAsync();
// });

// app.MapPost("/", async (ToDoDbContext db,string name,int id) =>
// {
//     var i=new Item(){Name=name,UserId=id};
//     db.Items.Add(i);
//     await db.SaveChangesAsync();
//     return Results.Created($"/items/{id}", i);
// });

// app.MapPut("/{id}", async (int id, bool IsComplete, ToDoDbContext db) =>
// {
//     var todo = await db.Items.FindAsync(id);

//     if (todo is null) return Results.NotFound();

//     todo.IsComplete = IsComplete;

//     await db.SaveChangesAsync();

//     return Results.NoContent();
//      });

// // // Route למחיקת פריט
// app.MapDelete("/{id}", async (int id, ToDoDbContext db) =>
// {
//     var item = await db.Items.FindAsync(id);
//     if (item is null) return Results.NotFound();

//     db.Items.Remove(item);
//     await db.SaveChangesAsync();
//     return Results.NoContent();
// });



// //יצירת טוקן
//    object createJwt(User user)
// {
//     var claims = new List<Claim>()
//     {
//         new Claim("name", user.Name),
//                 new Claim("id", user.IdUsers.ToString()),
//                 new Claim("password", user.Password),
//     };
//     var secretKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("Jwt:key")));
//     var siginicCredentails=new SigningCredentials(secretKey,SecurityAlgorithms.HmacSha256);
//     var tokenOption=new JwtSecurityToken(
//         issuer: builder.Configuration.GetValue<string>("Jwt:Issuer"),
//         audience:builder.Configuration.GetValue<string>("Jwt:Audience"),
//         claims: claims,
//         expires: DateTime.Now.AddMinutes(30),
//         signingCredentials: siginicCredentails);
//         var tokenString=new JwtSecurityTokenHandler().WriteToken(tokenOption);
//     return new {Token=tokenString};
// }
// //התחברות
// app.MapPost("/login", async (ToDoDbContext db, User user) =>
// {
//  var myUser= db.Users.Where(x=>x.IdUsers == user.IdUsers).ToList();
//  System.Console.WriteLine(myUser[0].IdUsers);
// if (myUser.Count()>0 && myUser[0].Password == user.Password){
//     var  jwt= createJwt(myUser[0]);
//     return Results.Ok(new {jwt,myUser});
//  }
//  //--שגיאת 401-----
// return Results.Unauthorized();
// });
// //ללא אטריביוט של טוקן
// //הרשמה

// app.MapPost("/addUser",async (ToDoDbContext db, User user) =>
// {
// var myUser= db.Users.Where(x=>x.IdUsers == user.IdUsers);
// if (myUser.Count()==0){
//         System.Console.WriteLine("-------------------");

//     System.Console.WriteLine(user.Name);
//     await db.Users.AddAsync(user);
//     await db.SaveChangesAsync();
//     var jwt= createJwt(user);
//     return Results.Ok(jwt);
// }
// //--שגיאת 401-----
// return Results.Unauthorized();
// });



// //שליפת המשתמשים
// // app.MapGet("/users", (ToDoDbContext db) => db.Users.ToListAsync());
// app.MapGet("/users", async (ToDoDbContext db) => await db.Users.ToListAsync())
//     .Produces<List<User>>(StatusCodes.Status200OK)
//     .Produces(StatusCodes.Status404NotFound);
// //מידע על האפליקציה- אמור ליהות אמיתי
// app.MapGet("/info", () => "פרויקט פרקטיקוד 3\nיוצר: מירי שטראוס ");
// app.MapGet("/", () => "פרויקט פרקטיקוד 4\nיוצר: מירי שטראוס ");



// app.Run();



builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
    ServerVersion.Parse("8.0-mysql")));
//swagger
builder.Services.AddSwaggerGen();
//cors
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowSpecificOrigin",
//         builder => builder.WithOrigins("http://localhost:3000")
//                           .AllowAnyMethod()
//                           .AllowAnyHeader());
// });
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

//הוספת אופצית הזדהות בסווגר
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
        Reference = new OpenApiReference
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new List<string>()
        }
    });
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
builder.Services.AddControllers();
var app = builder.Build();
app.UseCors("AllowAllOrigins");
// app.UseCors("AllowSpecificOrigin");
// if (app.Environment.IsDevelopment())
// // {
//     app.UseSwagger();
//     app.UseSwaggerUI();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // כדי לגשת ל-Swagger בכתובת הבית
});

// }
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
//jwt
app.UseRouting();
app.UseAuthentication(); 
app.UseAuthorization();
app.MapControllers();
//שליפת כל המשימות 
app.MapGet("/", (ToDoDbContext db) => db.Items.ToListAsync());
//שליפה ע"פ מזהה של משתמש
app.MapGet("/byId", [Authorize] async (ToDoDbContext db, int id) => 
{
    return await db.Items.Where(x => x.UserId == id).ToListAsync();
});
//הוספת משימה למשתמש הנוכחי
app.MapPost("/", (ToDoDbContext db, string? name,int? usId) =>
{
    Item newItem = new Item(){Name=name,UserId=usId};
    db.Items.Add(newItem);
    var i = db.SaveChangesAsync();
    return i;
});
//עדכון משימה
app.MapPut("/",async (ToDoDbContext db, int id) =>
{
    var item = db.Items.FirstOrDefault(i => i.Id == id);
    if (item == null)
        return null;
    if(item.IsComplete!=true)
    item.IsComplete =true;
    else
        item.IsComplete =false;

    var i =await db.SaveChangesAsync();
    return item.IsComplete!;
});
//מחיקת משימה
app.MapDelete("/",async  (ToDoDbContext db, int id) =>
{
    var item = db.Items.FirstOrDefault(i => i.Id == id);
    if (item != null)
    {
        db.Items.Remove(item);
        int i =await db.SaveChangesAsync();
        return i;
    }
    return -1;
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
//ללא אטריביוט של טוקן
app.MapGet("/info", () => "פרויקט פרקטיקוד 3\nיוצר: אסתי קרליבך ");
app.Run();