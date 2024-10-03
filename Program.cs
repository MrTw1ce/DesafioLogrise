using Desafio.Components;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.IO;
using System.Text;

using System; 
using System.Collections; 
using System.Collections.Generic; 

using Microsoft.EntityFrameworkCore;
using SQLModel.NETCoreWebAPI.ApplicationDbContext;
using SQLModel.Desafio.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => {
        options.Cookie.Name = "authCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers();

var app = builder.Build();

app.UseSession();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

var cartContent = new List<ProductOrder>();

app.MapGet("/products", (AppDbContext dbContext) => {
    var result = dbContext.Products.ToList();
    var products = new List<Products>();
    for(int i = 0;i<result.Count;i++){
        products.Add(new Products(JsonSerializer.Deserialize<Images>(result[i].Image),result[i].Name,result[i].Category,result[i].Price));
    }
    return Results.Ok(products);
});

app.MapGet("/cart", () => {
    return Results.Ok(cartContent);
});

app.MapPost("/cart", (ProductReference reference) => {
    if(cartContent.Count() == 0){
        cartContent.Add(new ProductOrder(1, reference.Product, reference.Price, 1));
    }
    else{
        cartContent.Add(new ProductOrder(cartContent.Max(r => r.Id) + 1, reference.Product, reference.Price, 1));
    }
    return Results.Ok(cartContent);
});

app.MapDelete("/cart/{id:int}", (int id) => {
    var targetOrder = cartContent.SingleOrDefault(t => id == t.Id);
    if (targetOrder is null){
        return Results.BadRequest("The item doesn't exist in the cart!");
    }
    cartContent.Remove(targetOrder);
    return Results.Ok(cartContent);
});
app.MapPut("/cart/{id:int}", (int id, ProductOrder order) => {
    var targetOrder = cartContent.SingleOrDefault(t => id == t.Id);
    if (targetOrder is null){
        return Results.BadRequest("The item doesn't exist in the cart!");
    }
    if ((order.Quantity < 1) || (order.Quantity > 99)){
        return Results.BadRequest("Invalid quantity!");
    }
    var orderIndex = cartContent.IndexOf(targetOrder);
    cartContent[orderIndex] = new ProductOrder(targetOrder.Id, targetOrder.Product, order.Price, order.Quantity);
    return Results.Ok(cartContent);
});

app.MapDelete("/cart", () => {
    cartContent.Clear();
    return Results.Ok(cartContent);
});

app.MapPost("/submit-order", (AppDbContext dbContext, HttpContext httpContext) => {
    if (cartContent.Count() == 0){
        return Results.BadRequest("The cart is empty!");
    }
    var result = dbContext.Orders.ToList();
    string currentUserEmail = httpContext.Session.GetString("email");
    if (currentUserEmail is null){
        currentUserEmail = "";
    }
    string orderString = JsonSerializer.Serialize(cartContent);
    if (result.Count() == 0){
        dbContext.Add(new Order{Id=1,UserEmail=currentUserEmail,OrderJson=orderString});
    }
    else {
        dbContext.Add(new Order{Id=result.Max(r => r.Id)+1,UserEmail=currentUserEmail,OrderJson=orderString});
    }
    dbContext.SaveChanges();
    return Results.Ok("Order submitted!");
});

app.MapGet("/orders", (AppDbContext dbContext, HttpContext httpContext) => {
    if (httpContext.Session.GetString("role") != "admin"){
        return Results.Unauthorized();
    }
    var result = dbContext.Orders.ToList();
    var orders = new List<UserOrder>();
    for(int i = 0; i<result.Count();i++){
        orders.Add(new UserOrder(result[i].Id,result[i].UserEmail,JsonSerializer.Deserialize(result[i].OrderJson, ProductOrderContext.Default.ListProductOrder)));
    }
    return Results.Ok(orders);
}).RequireAuthorization();

app.MapGet("/user-orders/{id:int}", (int id, AppDbContext dbContext, HttpContext httpContext) => {
    var accounts = dbContext.Users.ToList();
    var targetAccount = accounts.SingleOrDefault(r => id == r.Id);
    if ((targetAccount is null) || (id == 0)){
        return Results.NotFound("User doesn't exist!");
    }
    if ((httpContext.Session.GetInt32("id") == targetAccount.Id) || (httpContext.Session.GetString("role") == "admin")){
        var result = dbContext.Orders.ToList().FindAll(r => r.UserEmail == targetAccount.Email).ToList();
        var orders = new List<UserOrder>();
        for(int i = 0; i<result.Count();i++){
            orders.Add(new UserOrder(result[i].Id,result[i].UserEmail,JsonSerializer.Deserialize(result[i].OrderJson, ProductOrderContext.Default.ListProductOrder)));
        }
        return Results.Ok(orders);
    }
    return Results.Unauthorized();
}).RequireAuthorization();

app.MapGet("/users", (AppDbContext dbContext, HttpContext httpContext) => {
    if (httpContext.Session.GetString("role") != "admin"){
        return Results.Unauthorized();
    }
    var result = dbContext.Users.Select(r => new {r.Id, r.Email, r.Role}).ToList();
    return Results.Ok(result);
}).RequireAuthorization();

app.MapGet("/user/{id:int}", (int id, AppDbContext dbContext,HttpContext httpContext) => {
    var result = dbContext.Users.Select(t => new {t.Id,t.Email,t.Role}).ToList();
    var targetAccount = result.SingleOrDefault(t => id == t.Id);
    if ((targetAccount is null) || (id == 0)){
        return Results.NotFound("User doesn't exist!");
    }
    if (((httpContext.Session.GetInt32("id") == targetAccount.Id) || (httpContext.Session.GetString("role") == "admin")) && (targetAccount.Id != 0)){
        return Results.Ok(targetAccount);
    }
    return Results.Unauthorized();
}).RequireAuthorization();

app.MapPost("/register", (AppDbContext dbContext, UserRegistration data) => {
    var users = dbContext.Users.ToList();
    var newUser = new User{Id = users.Max(r => r.Id) + 1, Email = data.Credentials.Email, Password = data.Credentials.Password, Role = "client"};
    var existingUser = users.SingleOrDefault(t => data.Credentials.Email == t.Email);
    if (existingUser is null){
        if (data.Credentials.Password != data.Confirmation){
            return Results.BadRequest("Passwords do not match!");
        }
        dbContext.Users.Add(newUser);
        dbContext.SaveChanges();
        return Results.Ok(newUser);
    }
    return Results.BadRequest("Email is already being used!");
});

app.MapPost("/login", (AppDbContext dbContext, HttpContext httpContext, UserCredentials credentials) => {
    var users = dbContext.Users.ToList();
    var userAccount = users.SingleOrDefault(t => credentials.Email == t.Email);
    if (userAccount is not null && userAccount.Email == credentials.Email && userAccount.Password == credentials.Password){
        //var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        httpContext.Session.SetInt32("id", userAccount.Id);
        httpContext.Session.SetString("email", userAccount.Email);
        httpContext.Session.SetString("role",userAccount.Role);
        var claims = new List<Claim>{
            new(ClaimTypes.Email, userAccount.Email),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties();
        httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity),authProperties);
        return Results.Ok("Logged In");
    }
    return Results.Unauthorized();
});

app.MapPost("/logout", (HttpContext httpContext) => {
    httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    httpContext.Session.Clear();
    return Results.Ok("Logged Out");
}).RequireAuthorization();

app.MapGet("/current-user", (HttpContext httpContext) => {
    return Results.Ok(httpContext.Session.GetString("email"));
});

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

public record ProductOrder(int Id, string Product, float Price, int Quantity);
[JsonSerializable(typeof(List<ProductOrder>))]
internal partial class ProductOrderContext : JsonSerializerContext{}

public record UserOrder(int Id, string Email, List<ProductOrder> Order);

public record ProductReference(string Product, float Price);

public record Images(string thumbnail,string mobile, string tablet, string desktop);

public record Products(Images Image, string Name, string Category, float Price);

public record UserCredentials(string Email, string Password);
public record UserRegistration(UserCredentials Credentials, string Confirmation);


