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
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

var bookmarksGroup = app.MapGroup("api/bookmarks").WithOpenApi();

bookmarksGroup.MapGet("/", GetBookmarks);
bookmarksGroup.MapPost("/", SynchronizeBookmarks);

app.Run();

#region Endpoints Implementation
async Task<IResult> GetBookmarks(IWebHostEnvironment environment)
{
    BookmarkTreeNode? savedBookmarks = null;
    if (File.Exists($"{environment.WebRootPath}{Path.DirectorySeparatorChar}{builder.Configuration.GetValue<string>("BookmarksFileName")}"))
    {
        using FileStream stream = File.OpenRead($"{environment.WebRootPath}{Path.DirectorySeparatorChar}{builder.Configuration.GetValue<string>("BookmarksFileName")}");
        savedBookmarks = await JsonSerializer.DeserializeAsync<BookmarkTreeNode>(stream);
    }
    return TypedResults.Ok(savedBookmarks);
}

async Task<IResult> SynchronizeBookmarks(HttpContext context, IWebHostEnvironment environment, BookmarkTreeNode browserBookmarks)
{
    using FileStream stream = File.Create($"{environment.WebRootPath}{Path.DirectorySeparatorChar}{builder.Configuration.GetValue<string>("BookmarksFileName")}");
    await JsonSerializer.SerializeAsync(stream, browserBookmarks, new JsonSerializerOptions { WriteIndented = true });
    await stream.DisposeAsync();
    return TypedResults.Created(context.Request.Path);
}

#endregion


