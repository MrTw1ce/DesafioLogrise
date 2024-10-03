using Desafio.Components;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/*
 Imports para a base de dados.
 ==============================
 Imports for the database.
*/
using Microsoft.EntityFrameworkCore;
using SQLModel.NETCoreWebAPI.ApplicationDbContext;
using SQLModel.Desafio.Models;

/*
 Imports para a autentificação.
 ================================
 Imports for the authentication.
*/
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

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

/*
 Esta variável armazena o conteúdo do carrinho.
 ==============================================
 This variable stores the cart's content.
*/
var cartContent = new List<ProductOrder>();

/*
 Esta rota é utilizada para obter os produtos presentes no catálogo da aplicação.
 Para o efeito a mesma realiza uma query à tabela 'products' da base de dados.
 Os valores da coluna 'image' dessa tabela são depois convertidos de uma string para um registro do tipo Images.
 ===========================================================================================================================
 This route is used to retrieve the products present on the application's catalogue.
 In order to so, it does a query to the table 'products' from the database.
 The values of the column 'image' from that table are afterwards converted from a string to a record with the type Images.
*/
app.MapGet("/products", (AppDbContext dbContext) => {
    var result = dbContext.Products.ToList();
    var products = new List<Products>();
    for(int i = 0;i<result.Count;i++){
        products.Add(new Products(JsonSerializer.Deserialize<Images>(result[i].Image),result[i].Name,result[i].Category,result[i].Price));
    }
    return Results.Ok(products);
});

/*
 Esta rota é utlizada para obter o conteúdo atual do carrinho.
 ================================================================
 This route is used to retrieve the current content from the cart.
*/
app.MapGet("/cart", () => {
    return Results.Ok(cartContent);
});

/*
 Esta rota é utilizada para adicionar um produto ao carrinho.
 O ID que o produto adicionado vai assumir no carrinho será o valor do maior ID presente no carrinho incrementado por 1.
 Caso não haja produtos no carrinho o produto adicionado vai receber o ID 1.
 ========================================================================================================================
 This route is used to add a product to the cart.
 The ID the added product will get will be the value of the biggest ID in the cart incremented by 1.
 If there are no products in the cart the added product shall recieve the ID 1.
*/
app.MapPost("/cart", (ProductReference reference) => {
    if(cartContent.Count() == 0){
        cartContent.Add(new ProductOrder(1, reference.Product, reference.Price, 1));
    }
    else{
        cartContent.Add(new ProductOrder(cartContent.Max(r => r.Id) + 1, reference.Product, reference.Price, 1));
    }
    return Results.Ok(cartContent);
});

/*
 Esta rota é utilizada para remover um produto do carrinho por meio do ID deste.
 Antes de se remover o produto, verifica-se se existe um produto no carrinho com ID fornecido pelo utilizador.
 Se um produto com ID fornecido pelo utilizador não existir, é devolvida uma mensagem a dizer que o produto não existe no carrinho.
 Caso contrário, o produto é removido e o conteúdo do carrinho é devolvido.
 ===================================================================================================================================
 This route is used to remove a product from the cart, through its ID.
 Before removing the product, a verification occurs in order to check if the product with the given ID exists in the cart.
 If the product with the given ID doesn't exist, a message saying that the product doesn't exist in the cart is returned.
 Otherwise, the product is removed from the cart and its contents are returned.
*/
app.MapDelete("/cart/{id:int}", (int id) => {
    var targetOrder = cartContent.SingleOrDefault(t => id == t.Id);
    if (targetOrder is null){
        return Results.BadRequest("The product isn't in the cart!");
    }
    cartContent.Remove(targetOrder);
    return Results.Ok(cartContent);
});

/*
 Esta rota é utilizada para atualizar a quantidade de um produto presente no carrinho por meio do ID deste.
 À semelhança do que ocorre na rota anterior, nesta rota também é verificado se o produto com o ID fornecido está presente no carrinho.
 Se o produto não existir no carrinho, é devolvida uma mensagem a dizer que o produto não existe no carrinho.
 Caso contrário, é o produto no carrinho tem a sua quantidade alterada e o conteúdo do carrinho é devolvido.
 A nova quantidade do produto deve estar contida entre 1 e 99.
 =======================================================================================================================================
 This route is used to update the quantity of a product in the cart through its ID.
 Like in the previous route, this route also verifies if there is a product with the given ID in the cart.
 If the product doesn't exist in the cart, a message saying that the product doesn't exist in the cart is returned.
 Otherwise, the product's quantity in the cart is updated and the cart's contents are returned.
 The new quantity must be between 1 and 99.
*/
app.MapPut("/cart/{id:int}", (int id, ProductOrder order) => {
    var targetOrder = cartContent.SingleOrDefault(t => id == t.Id);
    if (targetOrder is null){
        return Results.BadRequest("The product isn't in the cart!");
    }
    if ((order.Quantity < 1) || (order.Quantity > 99)){
        return Results.BadRequest("Invalid quantity!");
    }
    var orderIndex = cartContent.IndexOf(targetOrder);
    cartContent[orderIndex] = new ProductOrder(targetOrder.Id, targetOrder.Product, order.Price, order.Quantity);
    return Results.Ok(cartContent);
});

/*
 Esta rota é utilizada para remover todos os produtos do carrinho.
 Após a remoção de todos os produtos do carrinho, a mesma retorna o conteúdo do carrinho.
 ====================================================================================
 This route is used to remove every product from the cart.
 After removing all products from the cart, it returns the cart's contents.
*/
app.MapDelete("/cart", () => {
    cartContent.Clear();
    return Results.Ok(cartContent);
});

/*
 Esta rota é utilizada para armazenar o pedido de um cliente na tabela 'orders' da base de dados.
 O pedido será armazenado apenas se houver pelo menos um produto no carrinho.
 No registro do pedido irá constar o email do utilizador, se este estiver autenticado, e uma string JSON com os detalhes da ordem.
 Caso o utilizador não esteja autenticado, o email do emissor do pedido será armazenado como uma string vazia.
 ================================================================================================================================
 This route is used to store the order of a client in the table 'orders' from the database.
 The order will only be stored if there is at least 1 product in the cart.
 In orders record will contain the user's email, if they are authenticated, and a JSON string with the order's details.
 If the user is not authenticated, then email of the order's sender will be stored as an empty string.
*/
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

/*
 Esta rota é utilizada para obter as ordens realizadas pelos utilizadores, sendo acessível apenas por utilizadores autenticados e cujo a sessão tenha a role 'admin'.
 De forma a obter as ordens realizadas, é realizada uma query à tabela 'orders' da base de dados.
 Depois de se obterem os dados, os valores da coluna 'order' são convertidos de uma string JSON para uma lista de registros to tipo ProductOrder.
 Após o tratamento dos dados, uma lista com as ordens realizadas é retornada pela rota.
 ======================================================================================================================================================================
 This route is used to retrieve the orders made by the application's users, being only accessible by authenticated users whose session's role is 'admin'.
 In order to do so, a query is made to the table 'orders' from the database.
 After retrieving the data, the values of the column 'order' are converted from a JSON string to a list of records with the type ProductOrder.
 After treating the data , a list with orders made is returned by the route.
*/
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

/*
 Esta rota é utilizada para obter as ordens realizadas por uma conta específica, por meio do seu ID, sendo acessível apenas por utilizadores autenticados e cujo a sessão tenha a role 'admin'.
 Nesta rota, é verificado se existe uma conta com o ID fornecido.
 Se a conta não existir, é retornada uma mensagem a informar que a mesma não existe.
 Caso contrário ocorre uma segunda verificação, na qual se verifica se a sessão do utilizador tem a role 'admin' ou o ID da mesma corresponde o ID fornecido.
 Sendo que a rota irá apenas retornar os pedidos realizados por uma conta específica, se o utilizador cumprir com pelo menos uma das condições da segunda verificação.
 ====================================================================================================================================================================================================
 This route is used to retrieve the orders place by a single account, through its ID, being only accessible by authenticated users whose session's role is 'admin'.
 In this route, occurs a verification in which is checked if an account with the given ID exists.
 If the account doesn't exist, a message saying that it does not exist is returned.
 Otherwise a secound verification occurs to checked if the user's session either has the role 'admin' or the given ID.
 This route will only return the orders placed by a specific account, if the user meets at least one condition of the secound validation.
*/
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

/*
 Esta rota permite obter os dados de todas contas na base de dados, sendo apenas acessível por utilizadores autenticados e cujo a sessão tenha a role 'admin'.
 Para o efeito é realizada uma query à tabela 'users' da base de dados, da mesma são extraídas os dados de 3 colunas: 'id', 'email' e 'role'.
 ==============================================================================================================================================================
 This route allows to retrieve data of all accounts in the database, being only accessible by users authenticated whose session has the role 'admin'.
 In order to do so, a query is done to the table 'users' from the database, from which the data of 3 columns is extracted: 'id', 'email' and 'role'.
*/
app.MapGet("/users", (AppDbContext dbContext, HttpContext httpContext) => {
    if (httpContext.Session.GetString("role") != "admin"){
        return Results.Unauthorized();
    }
    var result = dbContext.Users.Select(r => new {r.Id, r.Email, r.Role}).ToList();
    result = result.FindAll(r => r.Id != 0);
    return Results.Ok(result);
}).RequireAuthorization();

/*
 Esta rota permite obter os dados de uma conta específica na base de dados por meio do ID desta, sendo apenas acessível por utilizadores autenticados.
 Na mesma, é verificado se existe uma conta com o ID fornecido.
 Se a conta não existir, é retornada uma mensagem a informar o utilizador que a conta não existe.
 Caso contrário, ocorre uma segunda validação na qual se verifica se o utilizador tem na sua sessão a role 'admin' ou o ID fornecido.
 Apenas se o utilizador cumprir com uma das condições da segunda validação, será retornado os dados da conta com o ID fornecido.
 Os dados da conta que vão ser retornados são: o ID, o email e a role desta.
 ==============================================================================================================================================================
 This route allows to retrieve the data from a specific account in the database through its ID, being only accessible to authenticated users.
 Within this route, occurs a verification to check if there is an account with the given ID.
 If that account doesn't exist, then a message informing that the account doesn't exist is returned.
 Otherwise, a secound validation shall occur in which will be checked if the user has in their session the role 'admin' or the given ID.
 Only if the user meets one of the conditions of the secound validation, the account's data will be returned.
 The data from the account that will be returned will be: its ID, its email and its role.
*/
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

/*
 Esta rota permite criar novas contas com a role 'client'.
 Para o efeito, é necessário fornecer um email, uma password e a confirmação desta.
 Antes de se criar uma conta, é verificado se já existe alguma conta com o email fornecido.
 Se não existir, ocorre uma segunda validação na qual se verifica se a password coincide com a sua confirmação.
 Se a password coincidir com a sua confirmação uma nova conta com a role 'client' será criada.
 Caso os dados colocados no registro não passem numa das validações, será devolvida uma mensagem a esclarecer o utilizador sobre o ocorrido.
 ==================================================================================================================================================
 This route allows to create new accounts with the role 'client'.
 In order to do so the user, must provide an email, a password and its confirmation.
 Before the account is created, it is verified if there is already an account using the given email.
 If there is no account with the given email, a secound validation occurs in which is verified if the given password is equal to its confirmation.
 If the given password is equal to its confirmation a new account with the role 'client' is created.
 If the given data fails any validation, a message informing about the problem will be returned.
*/
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
        return Results.Ok("Account created!");
    }
    return Results.BadRequest("Email is already being used!");
});

/*
 Esta rota permite ao utilizador realizar login na aplicação.
 Para o efeito o utilizador deve fornecer um email e uma password.
 Se existir uma conta que tenha o email e a password fornecidos, então os dados da conta (id, email e role) vão ser armazenados na sessão e o utilizador vai ser autenticado.
 =============================================================================================================================================================================
 This route allows the user to log in to the application.
 In order to do so, the user must provide an email and a password.
 If there is an account with the given email and password, that account's data (id, email and role) will be stored in the session and the user will be authenticated.
*/
app.MapPost("/login", (AppDbContext dbContext, HttpContext httpContext, UserCredentials credentials) => {
    if (httpContext.Session.GetString("email") != null){
        return Results.BadRequest("You are already logged in!");
    }
    var users = dbContext.Users.ToList();
    var userAccount = users.SingleOrDefault(t => credentials.Email == t.Email);
    if (userAccount is not null && userAccount.Email == credentials.Email && userAccount.Password == credentials.Password && credentials.Email != "" && credentials.Password != ""){
        httpContext.Session.SetInt32("id", userAccount.Id);
        httpContext.Session.SetString("email", userAccount.Email);
        httpContext.Session.SetString("role",userAccount.Role);
        var claims = new List<Claim>{
            new(ClaimTypes.Email, userAccount.Email),
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties();
        httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(claimsIdentity),authProperties);
        return Results.Ok("Logged in");
    }
    return Results.Unauthorized();
});

/*
 Esta rota permite ao utilizador fazer logout da aplicação, sendo acessível apenas a utilizadores autenticados.
 Ao fazer logout, os dados da conta do utilizador são removidos da sessão.
 ================================================================================================================
 This route allows the user to log out of the application and is only accessible to authenticated users.
 Upon logging out, the user's account data will be removed from the session.
*/
app.MapPost("/logout", (HttpContext httpContext) => {
    httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    httpContext.Session.Clear();
    return Results.Ok("Logged out");
}).RequireAuthorization();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

/*
 Representa um produto no carrinho.
 ===================================
 Represents a product in the cart.
*/
public record ProductOrder(int Id, string Product, float Price, int Quantity);
[JsonSerializable(typeof(List<ProductOrder>))]
internal partial class ProductOrderContext : JsonSerializerContext{}

/*
 Representa a ordem feita por um utilizador.
 ============================================
 Represents an order made by a user.
*/
public record UserOrder(int Id, string Email, List<ProductOrder> Order);

/*
 Representa um produto antes deste ser adicionado ao carrinho.
 ==============================================================
 Represents a product before it is added to the cart.
*/
public record ProductReference(string Product, float Price);

/*
 Representa as imagens associadas a um produto no catálogo.
 ==================================================================
 Represents the images associated with a product in the catalogue.
*/
public record Images(string thumbnail,string mobile, string tablet, string desktop);

/*
 Representa os produtos no catálogo da aplicação.
 =================================================
 Represents the products in the application's catalogue.
*/
public record Products(Images Image, string Name, string Category, float Price);

/*
 Representa as credenciais de login de um utilizador.
 ======================================================
 Represents a user's login credentials.
*/
public record UserCredentials(string Email, string Password);

/*
 Representa as credenciais que um utilizador pretende utilizar para criar uma conta e a confirmação da sua password.
 ====================================================================================================================
 Represents the credentials that a user desires to use to create an account and the confirmation for their password.
*/
public record UserRegistration(UserCredentials Credentials, string Confirmation);