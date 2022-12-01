using System.Reflection;
using System.Text.Json;
using GoldenGuillaume.BookmarkApi.Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

var bookmarksGroup = app.MapGroup("api/bookmarks").WithOpenApi();

bookmarksGroup.MapGet("/", GetBookmarks);
bookmarksGroup.MapPost("/", SynchronizeBookmarks);

app.Run();

async Task<IResult> GetBookmarks(IWebHostEnvironment environment)
{
    var savedBookmarks = await LoadSavedBookmarks(environment);
    return savedBookmarks != null ? TypedResults.Ok(savedBookmarks) : TypedResults.Ok();
}

async Task<IResult> SynchronizeBookmarks(HttpContext context, IWebHostEnvironment environment, BookmarkTreeNode browserBookmarks)
{
    var savedBookmarks = await LoadSavedBookmarks(environment);
    if (savedBookmarks != null && JsonSerializer.Serialize(savedBookmarks) != JsonSerializer.Serialize(browserBookmarks))
    {

    }

    string filePath = $"{environment.WebRootPath}{Path.DirectorySeparatorChar}{builder.Configuration.GetValue<string>("BookmarksFileName")}";
    using FileStream stream = File.Create(filePath);
    await JsonSerializer.SerializeAsync(stream, browserBookmarks, new JsonSerializerOptions { WriteIndented = true });
    await stream.DisposeAsync();

    return savedBookmarks == null ? TypedResults.Created(context.Request.Path) : TypedResults.Ok(browserBookmarks);
}

async Task<BookmarkTreeNode?> LoadSavedBookmarks(IWebHostEnvironment environment)
{
    if (File.Exists($"{environment.WebRootPath}{Path.DirectorySeparatorChar}{builder.Configuration.GetValue<string>("BookmarksFileName")}"))
    {
        using FileStream stream = File.OpenRead($"{environment.WebRootPath}{Path.DirectorySeparatorChar}{builder.Configuration.GetValue<string>("BookmarksFileName")}");
        return await JsonSerializer.DeserializeAsync<BookmarkTreeNode>(stream);
    }
    return await Task.FromResult<BookmarkTreeNode?>(null);
}
