using BlazorControlCenter.Components;
using BlazorControlCenter.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add our state service as a singleton (only one instance for the whole app).
builder.Services.AddSingleton<BlazorControlCenter.Services.ServerStateService>();

// Add our TCP server as a hosted service (it will be started and stopped with the web app).
builder.Services.AddHostedService<BlazorControlCenter.Services.TcpServerService>();

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
