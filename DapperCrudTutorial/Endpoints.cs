namespace DapperCrudTutorial;

public static class Endpoints
{
    private const string BaseApiUrl = @"api/superheroes";
    private const string defaultConnectionStringName = "default";

    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(BaseApiUrl, GetAllSuperHeroes);
        app.MapGet($"{BaseApiUrl}/{{id}}", GetSuperHero);
        app.MapPost(BaseApiUrl, PostSuperHero);
        app.MapPut($"{BaseApiUrl}/{{id}}", PutSuperHero);
        app.MapDelete($"{BaseApiUrl}/{{id}}", DeleteSuperHero);

        return app;
    }

    private static async Task<IEnumerable<SuperHero>> SelectAllSuperHeroes(SqlConnection connection) =>
        await connection.QueryAsync<SuperHero>("SELECT * FROM superheroes");

    private static async Task<IResult> GetAllSuperHeroes([FromServices] IConfiguration config)
    {
        using var connection = new SqlConnection(config.GetConnectionString(defaultConnectionStringName));
        var heroes = await SelectAllSuperHeroes(connection);
        return Results.Ok(heroes);
    }

    private static async Task<IResult> GetSuperHero([FromServices] IConfiguration config, Guid id)
    {
        using var connection = new SqlConnection(config.GetConnectionString(defaultConnectionStringName));
        var hero = await connection.QueryFirstAsync<SuperHero>(
            "SELECT * FROM superheroes WHERE id = @Id",
            new { Id = id });
        return Results.Ok(hero);
    }

    private static async Task<IResult> PostSuperHero([FromServices] IConfiguration config, SuperHero newSuperHero)
    {
        using var connection = new SqlConnection(config.GetConnectionString(defaultConnectionStringName));
        await connection.ExecuteAsync(
            "INSERT INTO superheroes (name, firstname, lastname, place) VALUES (@Name, @FirstName, @LastName, @Place)",
            newSuperHero);
        return Results.Ok(await SelectAllSuperHeroes(connection));
    }

    private static async Task<IResult> PutSuperHero([FromServices] IConfiguration config, Guid id, SuperHero superHero)
    {
        if (id != superHero.Id)
        {
            return Results.NotFound();
        }
        using var connection = new SqlConnection(config.GetConnectionString(defaultConnectionStringName));
        await connection.ExecuteAsync(
            "UPDATE superheroes SET name = @Name, firstname = @FirstName, lastname = @LastName, place = @Place WHERE id = @Id",
            superHero);
        return Results.Ok(await SelectAllSuperHeroes(connection));
    }

    private static async Task<IResult> DeleteSuperHero([FromServices] IConfiguration config, Guid id)
    {
        using var connection = new SqlConnection(config.GetConnectionString(defaultConnectionStringName));
        await connection.ExecuteAsync(
            "DELETE FROM superheroes WHERE id = @Id",
            new { Id = id });
        return Results.Ok(await SelectAllSuperHeroes(connection));
    }
}
