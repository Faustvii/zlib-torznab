using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Persistence;
using Zlib.Torznab.Persistence.Repositories;
using Zlib.Torznab.Presentation.API.Core;
using Zlib.Torznab.Services;
using Zlib.Torznab.Presentation.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.Configure<JsonOptions>(opts =>
{
    opts.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services
    .AddControllers(x =>
    {
        x.FormatterMappings.SetMediaTypeMappingForFormat(
            "xml",
            MediaTypeHeaderValue.Parse("application/xml")
        );
        x.OutputFormatters.Add(new XmlSerializerOutputFormatterTorznabNamespace());
        x.OutputFormatters.Add(new StringOutputFormatter());
    })
    .AddXmlSerializerFormatters();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();

builder.Services
    .AddApplicationSettings(builder.Configuration)
    .AddCore()
    .AddHostedServices()
    .AddServiceLayer(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("Archive");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<ArchiveContext>(
    opts =>
        opts.UseMySql(
            connectionString,
            serverVersion,
            o => o.EnableStringComparisonTranslations().EnableIndexOptimizedBooleanColumns()
        )
);

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(x => x.DisplayRequestDuration());

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
