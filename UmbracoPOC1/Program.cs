WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Enable Razor Runtime Compilation for Development
#if DEBUG
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
#endif

// Explicitly add MVC services to ensure Surface Controllers are discovered
builder.Services.AddControllersWithViews();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseHttpsRedirection();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
