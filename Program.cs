using GameStore.Api.EndPoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();

var app = builder.Build();

app.MapGamesEndpoints();
app.Run();
