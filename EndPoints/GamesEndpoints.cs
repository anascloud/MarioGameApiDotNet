using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;

namespace GameStore.Api.EndPoints;

public static class GamesEndpoints
{   
    const string GetGameEndpointName = "GetGame";
    private static readonly List<GameDto> games = [
        new(
            1,
            "Breath of the Wild",
            "Nintendo Switch",
            19.9M,
            new DateOnly(2017, 7, 15)
        ),
        new(
            2,
            "The Legend of Zelda",
            "Nintendo Switch",
            19.3M,
            new DateOnly(2017, 7, 15)
        ),
    ];
    public static void MapGamesEndpoints(this WebApplication app)
    {   
        var group = app.MapGroup("/games");

        group.MapGet("/", () => games);

        group.MapGet("/{id}", (int id) =>
        {
            var game = games.Find(game => game.Id == id);
            return game is null ? Results.NotFound() : Results.Ok(game);
        })
        .WithName(GetGameEndpointName);

        group.MapPost("/", (CreateGameDto newGame, GameStoreContext dbcontext) =>
        {   
            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbcontext.Games.Add(game);
            dbcontext.SaveChanges();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = gameDto.Id }, gameDto);
        });

        group.MapPut("/{id}", (int id, UpdateGameDto updatedGame) =>
        {
            var index = games.FindIndex(game => game.Id == id);

            if (index == -1)
            {
                return Results.NotFound();
            }

            games[index] = new GameDto(
                id,
                updatedGame.Name,
                updatedGame.Genre,
                updatedGame.Price,
                updatedGame.ReleaseDate
            );

            return Results.NoContent();
        });


        group.MapDelete("/{id}", (int id) =>
        {
            games.RemoveAll(game => game.Id == id);
            return Results.NoContent();
        });
    }
}
