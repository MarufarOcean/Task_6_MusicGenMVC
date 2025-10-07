using MusicGenMVC.Services;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews().AddViewOptions(o => { });
builder.Services.AddHttpContextAccessor();


builder.Services.AddSingleton<LocaleStore>();
builder.Services.AddSingleton<SeededRandomFactory>();
builder.Services.AddScoped<SongGeneratorService>();


var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
name: "default",
pattern: "{controller=Songs}/{action=Index}/{id?}");


app.Run();