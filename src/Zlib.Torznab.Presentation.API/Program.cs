using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using MonoTorrent.Connections.TrackerServer;
using Zlib.Torznab.Models.Queues;
using Zlib.Torznab.Models.Repositories;
using Zlib.Torznab.Models.Settings;
using Zlib.Torznab.Persistence;
using Zlib.Torznab.Persistence.Repositories;
using Zlib.Torznab.Presentation.API;
using Zlib.Torznab.Presentation.API.HostedServices;
using Zlib.Torznab.Presentation.API.Services;
using Zlib.Torznab.Services.Ipfs;
using Zlib.Torznab.Services.Torrents;
using Zlib.Torznab.Services.Torznab;

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

builder.Services.AddSingleton<IBackgroundJobPool>(new DefaultBackgroundJobPool(30));

builder.Services.AddHostedService<HostedTorrentService>();
builder.Services.AddHostedService<HostedTrackerService>();
builder.Services.AddHostedService<HostedBackgroundJobPoolService>();

builder.Services.AddSingleton<ITrackerListener, APITrackerListener>();
builder.Services.AddSingleton<APITrackerListener>();
builder.Services.AddSingleton<ITorrentService, TorrentService>();

builder.Services.AddHttpClient<IIpfsGateway, IpfsGateway>();

builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddScoped<ITorznabService, TorznabService>();

builder.Services.Configure<ApplicationSettings>(
    builder.Configuration.GetSection(ApplicationSettings.Key)
);

builder.Services.Configure<IpfsSettings>(builder.Configuration.GetSection(IpfsSettings.Key));

builder.Services.Configure<TorrentSettings>(builder.Configuration.GetSection(TorrentSettings.Key));

builder.Services.Configure<TorznabSettings>(builder.Configuration.GetSection(TorznabSettings.Key));

var connectionString = builder.Configuration.GetConnectionString("Archive");
var serverVersion = ServerVersion.AutoDetect(connectionString);
builder.Services.AddDbContext<ArchiveContext>(
    opts =>
        opts.UseMySql(
            connectionString,
            serverVersion,
            o => o.EnableStringComparisonTranslations().EnableIndexOptimizedBooleanColumns()
        )
// .LogTo(Console.WriteLine, LogLevel.Information)
// .EnableSensitiveDataLogging()
// .EnableDetailedErrors()
);

var app = builder.Build();

// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
app.UseSwagger();
app.UseSwaggerUI(x => x.DisplayRequestDuration());

// }

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
