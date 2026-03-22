using Microsoft.EntityFrameworkCore;
using OnomasticsDataSystem.Core.Interfaces;
using OnomasticsDataSystem.Infrastructure.Data;
using OnomasticsDataSystem.Infrastructure.Repositories;
using OnomasticsDataSystem.Infrastructure.Services;
using OnomasticsDataSystem.Web.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

//pridane
//pretože viac komponentov používa ten istý DbContext paralelne.
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

builder.Services.AddScoped<IStatisticsService, StatisticsService>();

builder.Services.AddScoped<ISourceRepository, SourceRepository>();

builder.Services.AddMudServices();

//pridane

// Add services to the container.
builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

var app = builder.Build();

var supportedCultures = new[] { "sk-SK" };
var localizationOptions = new RequestLocalizationOptions()
	.SetDefaultCulture(supportedCultures[0])
	.AddSupportedCultures(supportedCultures)
	.AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

app.Run();
