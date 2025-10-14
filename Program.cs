using BlazorControlCenter.Components;
using BlazorControlCenter.Services;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JFaF5cXGRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdmWH9fc3RcQ2RdUE1yX0VWYEg=");

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddSyncfusionBlazor();

// Add our state service as a singleton (only one instance for the whole app).
builder.Services.AddSingleton<BlazorControlCenter.Services.ServerStateService>();

builder.Services.AddMudServices();

// Add our TCP server as a hosted service (it will be started and stopped with the web app).
builder.Services.AddHostedService<BlazorControlCenter.Services.TcpServerService>();

//builder.Services.AddDbContext<MyDbContext>(options =>
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
