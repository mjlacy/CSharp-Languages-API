using System.Net.Mime;
using CSharp_Languages_API.Models;
using CSharp_Languages_API.Services;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

WebApplicationOptions webApplicationOptions = new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
};

WebApplicationBuilder builder = WebApplication.CreateBuilder(webApplicationOptions);

builder.Services.Configure<LanguagesDatabaseSettings>(
    builder.Configuration.GetSection("LanguagesDatabase"));

builder.Services.AddSingleton<LanguagesService>();

builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

WebApplication app = builder.Build();

// app.UseHttpsRedirection();

// app.UseAuthorization();

LanguagesDatabaseSettings? items;

using (StreamReader r = new StreamReader("appsettings.json"))
{
    string json = r.ReadToEnd();
    string newJson = json.Substring(json.IndexOf(':') + 2, json.IndexOf('}') - json.IndexOf(':') - 1);
    items = JsonConvert.DeserializeObject<LanguagesDatabaseSettings>(newJson);
}

try {
    bool _ = new LanguagesService(Options.Create<LanguagesDatabaseSettings>(items)).Ping();
} catch (Exception _) {
    Console.WriteLine("Failed to connect to database at launch.");
    return;
}

app.MapControllers();

app.Use(async (context, next) => {   
    await next();
    if (context.Response.StatusCode == 404 && context.Request.Path.Value != "/" && context.Request.Path.Value?.Length != 25) {
        context.Response.ContentType = MediaTypeNames.Text.Plain;
        await context.Response.WriteAsync("You have accessed an invalid URL");
    }
});

app.Run();
