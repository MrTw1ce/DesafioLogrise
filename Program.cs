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



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddControllers();

var app = builder.Build();

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

app.MapGet("/users", (AppDbContext dbContext) => {
    var result = dbContext.Users.Select(r => new {r.Id, r.Email}).ToList();
    return Results.Ok(result);
});

app.MapGet("/user/{id:int}", (int id, AppDbContext dbContext) => {
    var result = dbContext.Users.Select(t => new {t.Id,t.Email}).ToList();
    var user = result.SingleOrDefault(t => id == t.Id);
    if (user is null){
        return Results.NotFound("The user doesn't exist!");
    }
    return Results.Ok(user);
});

app.MapPost("/register", [AllowAnonymous] (AppDbContext dbContext, UserRegistration data) => {
    var users = dbContext.Users.ToList();
    var newUser = new User{Id = users.Max(r => r.Id) + 1, Email = data.Credentials.Email, Password = data.Credentials.Password};
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

app.MapPost("/login", (AppDbContext dbContext, UserCredentials credentials) => {
    var users = dbContext.Users.ToList();
    var userAccount = users.SingleOrDefault(t => credentials.Email == t.Email);
    if (userAccount is not null && userAccount.Email == credentials.Email && userAccount.Password == credentials.Password){
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        
        return Results.Ok(userAccount);
    }
    return Results.Unauthorized();
});

app.MapPost("/logout", () => {
    return "Logout";
}).RequireAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

public record ProductOrder(int Id, string Product, float Price, int Quantity);

public record ProductReference(string Product, float Price);

public record Images(string thumbnail,string mobile, string tablet, string desktop);

public record Products(Images Image, string Name, string Category, float Price);

public record UserCredentials(string Email, string Password);
public record UserRegistration(UserCredentials Credentials, string Confirmation);
