# Product list with cart

## Table of contents

- [Resumo / Overview](#resumo--overview)
  - [Screenshot](#screenshot)
- [O meu processo / My process](#o-meu-processo--my-process)
  - [Construído com / Built with](#construído-com--built-with)
  - [O que eu aprendi / What I learned](#o-que-eu-aprendi--what-i-learned)
  - [Desenvolvimento contínuo / Continued development](#desenvolvimento-contínuo--continued-development)
  - [Recursos úteis / Useful resources](#recursos-úteis--useful-resources)
- [Autor / Author](#autor--author)

## Resumo / Overview

 Este projeto consiste no desenvolvimento do backend de uma aplicação web com C# ASP.NET.
 No mesmo foi criada uma API REST que exerce funções vistas em plataformas de e-commerce, nomeadamente:
 - Adicionar produtos a um carrinho;
 - Ajustar a quantidade de um produto no carrinho;
 - Obter o conteúdo do carrinho;
 - Remover todo o conteúdo do carrinho;
 - Obter os produtos da plataforma.

==========================================================================

 This project consists on the development of the backend of a web app with C# ASP.NET.
 A REST API was created within this project, this API executes functions seen in e-commerce platforms, mainly:
 - Add products to a cart;
 - Adjust a product's quantity on the cart;
 - Get the contents of the cart;
 - Remove all the content from the cart;
 - Get the products from the platform.

### Screenshot

![](./design/result.png)

![](./design/result-modal.png)

## O meu processo / My process

 O desenvolvimento deste projeto começou com uma fase de planeamento, na qual se determinou que se deveria dar prioridade à implementação das rotas da API associadas ao carrinho e aos produtos da plataforma.
 Após a fase de planeamento, implementaram-se as seguintes rotas:
 - GET    /products       -> Obtém os produtos da plataforma
 - GET    /cart           -> Obtém os produtos no carrinho
 - POST   /cart           -> Adiciona um produto ao carrinho
 - DELETE /cart/{id:int}  -> Remove um produto do carrinho
 - PUT    /cart/{id:int}  -> Ajusta a quantidade de um produto no carrinho
 - DELETE /cart           -> Remove todos os produtos do carrinho

 Uma vez implementadas estas rotas, iniciou-se a implementação do frontend.
 Para o efeito recorreu-se ao Blazor, porém de forma a que fosse possível implementar todas as funcionalidades do frontend dependeu-se mais do JavaScript.
 No frontend criaram-se métodos que realizam chamadas AJAX de forma a obter e atualizar o conteúdo do carrinho e do catálogo.

 Após a implementação do frontend, criou-se um base de dados em SQLite na qual seriam armazenados os dados da aplicação. Nesta base de dados constam as seguintes tabelas:
 - products
   - name      -> Nome do produto
   - image     -> String JSON com os caminhos para as imagens de um produto
   - category  -> Categoria do produto
   - price     -> Preço por unidade do produto

 - users
   - id        -> ID do utilizador
   - email     -> Email do utilizador
   - password  -> Password do utilizador
   - role      -> Role do utilizador

 - orders
   - id        -> ID da ordem
   - userEmail -> Email do utilizador que submeteu a ordem
   - orderJSON -> String JSON com os dados dos produtos associados à ordem

 Assim que se incluiu a base de dados no projeto, adicionaram-se as seguintes rotas para a gestão de utilizadores:
  - GET   /users          -> Obtém os dados de todos os utilizadores
  - GET   /user/{id:int}  -> Obtém os dados de um utilizador, com recurso ao ID do mesmo
  - POST  /register       -> Cria um novo utilizador com a role 'client'
  - POST  /login          -> Faz login de um utilizador
  - POST  /logout         -> Faz logout de um utilizador

 De forma a que fosse possível armazenar e consultar os pedidos realizados por um utilizador também se implementaram as seguintes rotas:
  - POST  /submit-order         -> Adiciona uma ordem à base de dados
  - GET   /orders               -> Obtém todas as ordens armazenadas na base de dados
  - GET   /user-orders/{id:int} -> Obtém todas as ordens realizadas por um utilizador com um determinado ID
 
 Após a implementação das rotas e funções referidas anteriormente, foram realizadas umas alterações ao código de forma garantir que o frontend se ajustava consoante o tamanho do ecrã e o bom funcionamento do projeto tanto no frontend como no backend.

==========================================================================

 The development of this project started with a planning phase, in which was decided that priority should be given to the implementation of the API routes that are associated to either the cart or the products of the platform.
 After the planning phase, the following routes were implemented:
 - GET    /products       -> Gets the products of the platform
 - GET    /cart           -> Gets the products in the cart
 - POST   /cart           -> Adds a product to the cart
 - DELETE /cart/{id:int}  -> Removes a product from the cart
 - PUT    /cart/{id:int}  -> Changes a product's quantity in the cart
 - DELETE /cart           -> Removes every product from the cart

 Once this routes were implemented, the implementation of the fronteend began.
 For this purpose Blazor was chosen; however, in order to implement every function needed in the frontend its implementation relied heavily on JavaScript.
 Within the frontend, methods which execute AJAX calls in order to get and update the contents of both the catalogue and the cart were implemented.

 Once the frontend was implemented, a SQLite database in which the app's data would be stored was created. Within this database the following tables can be found:
 - products
   - name      -> Product's name
   - image     -> JSON string with the paths to a product's images
   - category  -> Product's category
   - price     -> Product's price per unit

 - users
   - id        -> User's ID
   - email     -> User's email
   - password  -> User's password
   - role      -> User's role

 - orders
   - id        -> Order's ID
   - userEmail -> Email from the user who submitted the order
   - orderJSON -> JSON string with the data from the products associated with the order

 Once the database was included in the project, the following routes for user management were added:
  - GET   /users          -> Gets data from every user
  - GET   /user/{id:int}  -> Gets data from a user through its ID
  - POST  /register       -> Creates a new user with the 'client' role
  - POST  /login          -> Logs in an user
  - POST  /logout         -> Logs out an user

 In order to store and see the order made by the app's users the following routes were implemented:
  - POST  /submit-order         -> Adds an order to the database
  - GET   /orders               -> Gets every order stored in the database
  - GET   /user-orders/{id:int} -> Gets every order made by user with a given ID
 
 After implementing every route and function referred previously, the code suffered slight alterations in order to ensure that the frontend adapts to the screen's size and that the project works as intended in both the frontend and backend.

### Construído com / Built with

- Semantic HTML5 markup
- CSS
- JavaScript
- [Bootstrap](https://getbootstrap.com/)
- [JQuery](https://jquery.com/)
- [ASP.NET Core](https://dotnet.microsoft.com/en-us/apps/aspnet)
- [Blazor](https://dotnet.microsoft.com/pt-br/apps/aspnet/web-apps/blazor)
- [Entity Framework Core](https://github.com/dotnet/efcore)
- [SQLite](https://www.postgresql.org/)

### O que eu aprendi / What I learned

 Devido ao facto deste ser um dos primeiros projetos desenvolvidos por mim com recurso a C# ASP.NET, todo o desenvolvimento do mesmo foi um enorme processo de aprendizagem. Durante a realização deste projeto obtive experiência a:
 - Criar uma API mínima em C# ASP.NET e conectá-la a uma base de dados;
 - Obter e armazenar dados numa base de dados conectada a uma API;
 - Criar e manipular sessões numa API;
 - Implementar autorizações nas rotas da API.

 O código que se segue mostra duas rotas da API, `/submit-order` e `/orders`. Estas rotas têm o intuito de armazenar a ordem de um utilizador na base de dados e de obter todas as ordens realizadas pelos utilizadores, respetivamente. Para que a segunda rota devolva as ordens realizadas é necessário que o utilizador esteja autenticado e que a sessão do mesmo tenha a role 'admin'.

 ```cs
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
 ```

 Durante a realização deste projeto também adquiri experiência em garantir que o conteúdo de uma página web se adapte ao tamanho do ecrã com recurso a CSS. O código CSS que se segue foi produzido de forma a garantir que alguns elementos da página web fossem exibidos devidamente em ecrãs com uma largura específica.
 
 ```css
  @media (max-width:376px){
      .catalogue-title{
          padding-left: 10%;
      }

      .modal-body{
          overflow-y: hidden !important;
          max-height: 300px !important;
      }

      .order-products-modal{
          overflow-y: auto;
          overflow-x: hidden;
          max-height: 150px !important;
          padding: 5px 0px 0px 5px !important;
      }
  }

  @media (max-width:1445px){
      .confirm.btn{
          width: 18rem !important;
      }

      .modal-body{
          overflow-y: auto;
          max-height: 300px;
      }

      .delivery-note{
          width: 20em;
      }

      .modal-body{
          overflow-y: hidden !important;
          max-height: 650px;
      }

      .order-products-modal{
          overflow-y: auto;
          overflow-x: hidden;
          max-height: 450px;
          padding: 15px 0px 0px 15px;
      }
  }

  @media (min-width:1850px){
      .catalogue-title{
          padding-left: 3.5% !important;
      }
  }
 ```

==========================================================================

 Due to this being on of the first projects developed by me in C# ASP.NET, its development process was a whole learning processo. During the development of this project I've gained experience in:
 - Creating a minimal API with C# ASP.NET and connecting it to a database;
 - Getting and storing data in a database connected to the API;
 - Creating and manipulating sessions on an API;
 - Implementing authorizations in the API routes.

 The following code shows the API routes `/submit-order` e `/orders`. These routes have the objective of storing an user's order and getting every order made by the users, respectively. In order for the secound route to return the orders made by the users, the user must be authenticated and his session have the role 'admin'.

 ```cs
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
 ```

 While developing this project I've also adquired experience in ensuring that the contents of a web page adapt to the size of the screen through CSS. The following CSS code was created in order to ensure that some elements of the web page were displayed properly according to the screen's width.
 
 ```css
  @media (max-width:376px){
      .catalogue-title{
          padding-left: 10%;
      }

      .modal-body{
          overflow-y: hidden !important;
          max-height: 300px !important;
      }

      .order-products-modal{
          overflow-y: auto;
          overflow-x: hidden;
          max-height: 150px !important;
          padding: 5px 0px 0px 5px !important;
      }
  }

  @media (max-width:1445px){
      .confirm.btn{
          width: 18rem !important;
      }

      .modal-body{
          overflow-y: auto;
          max-height: 300px;
      }

      .delivery-note{
          width: 20em;
      }

      .modal-body{
          overflow-y: hidden !important;
          max-height: 650px;
      }

      .order-products-modal{
          overflow-y: auto;
          overflow-x: hidden;
          max-height: 450px;
          padding: 15px 0px 0px 15px;
      }
  }

  @media (min-width:1850px){
      .catalogue-title{
          padding-left: 3.5% !important;
      }
  }
 ```

### Desenvolvimento contínuo / Continued development

 Em projetos futuros, gostaria de expandir os meus conhecimentos no desenvolvimento do backend, com o intuito de:
  - Desenvolver aplicações mais complexas;
  - Entender melhor os conceitos de sessão, autentificação e de autorização e a implementação destes.

==========================================================================

 In future projects, I'd like to expand my knowledge in backend development, with the objective of:
 - Developing more complex applications;
 - Improving my understanding the concepts of session, authentication and authorization and their implementation.

### Recursos úteis / Useful resources

- [Playlist com Tutoriais de C# ASP.NET](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oWunQnm3WnZxJrseIw2zSAk)
- [Playlist com Tutoriais de Blazor](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oXNZX1Q8rB-5xgTSKR8qA5k)
- [Documentação C# ASP.NET sobre Sessões](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-8.0)
- [Tutorial sobre Autorização com Recurso a Sessões](https://medium.com/@KumarHalder/session-based-authorization-in-asp-net-core-95eed1d3dded)

==========================================================================

- [Playlist with C# ASP.NET Tutorials](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oWunQnm3WnZxJrseIw2zSAk)
- [Playlist with Blazor Tutorials](https://www.youtube.com/playlist?list=PLdo4fOcmZ0oXNZX1Q8rB-5xgTSKR8qA5k)
- [C# ASP.NET Sessions Docummentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-8.0)
- [Session Based Authorization Tutorial](https://medium.com/@KumarHalder/session-based-authorization-in-asp-net-core-95eed1d3dded)

## Autor / Author

- GitHub - [MrTw1ce](https://github.com/MrTw1ce)
- LinkIn - [Lucas Martins](https://www.linkedin.com/in/lucas-martins-657aa8325/)
