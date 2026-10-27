using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.EndPoints;

public static class GamesEndpoints
{   
    const string GetGameEndpointName = "GetGame";
    // private static readonly List<GameSummaryDto> games = [
    //     new(
    //         1,
    //         "Breath of the Wild",
    //         "Nintendo Switch",
    //         19.9M,
    //         new DateOnly(2017, 7, 15)
    //     ),
    //     new(
    //         2,
    //         "The Legend of Zelda",
    //         "Nintendo Switch",
    //         19.3M,
    //         new DateOnly(2017, 7, 15)
    //     ),
    // ];
    public static void MapGamesEndpoints(this WebApplication app)
    {   
        var group = app.MapGroup("/games");

        group.MapGet("/", async (GameStoreContext dbcontext) => 
            await dbcontext.Games.Include(game => game.Genre)
            .Select(
                game => new GameSummaryDto(game.Id, game.Name, game.Genre!.Name, game.Price, game.ReleaseDate)
            ).AsNoTracking().ToListAsync());

        group.MapGet("/{id}", async (int id, GameStoreContext dbcontext) =>
        {
            var game = await dbcontext.Games.FindAsync(id);
            return game is null ? Results.NotFound() : Results.Ok(
                new GameDetailsDto(
                    game.Id,
                    game.Name,
                    game.GenreId,
                    game.Price,
                    game.ReleaseDate
                )
            );
        })
        .WithName(GetGameEndpointName);

        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbcontext) =>
        {   
            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbcontext.Games.Add(game);
            await dbcontext.SaveChangesAsync();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = gameDto.Id }, gameDto);
        });

        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbcontext) =>
        {
            var existingGame = await dbcontext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name = updatedGame.Name;
            existingGame.GenreId = updatedGame.GenreId;
            existingGame.Price = updatedGame.Price;
            existingGame.ReleaseDate = updatedGame.ReleaseDate;

            await dbcontext.SaveChangesAsync();

            return Results.NoContent();
        });


        group.MapDelete("/{id}", async (int id, GameStoreContext dbcontext) =>
        {
            await dbcontext.Games.Where(game => game.Id == id).ExecuteDeleteAsync();
            return Results.NoContent();
        });
    }
}
