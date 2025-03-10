using MongoDB.Driver;
using Scandium.Services;
using Scandium.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR().AddAzureSignalR("Endpoint=https://scandium.service.signalr.net;AccessKey=sOYaYx+wk14PkSjVuwbxytrOImTn/fh7OCtPwnkXddo=;Version=1.0;");

builder.Host.ConfigureServices(services =>
{
    // MongoDb connection
    services.AddSingleton<IMongoClient>(
        new MongoClient(MongoClientSettings
            .FromConnectionString("mongodb+srv://thorium-web:Tt3NLI1dWVitYuZh@cluster0.7mtes.mongodb.net/thorium-data?retryWrites=true&w=majority")
        )
    );
    
    services.AddScoped<MongoDbContext>(s => 
        new MongoDbContext(
            s.GetRequiredService<IMongoClient>(),
            builder.Configuration.GetConnectionString("DatabaseName")
        )
    );

    services.AddScoped<GeneratorHelperService>();
    services.AddScoped<JobService>();
    services.AddTransient<ProgressReporterFactory>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<LoadingBarHub>("/loading-bar-progress");

app.Run();