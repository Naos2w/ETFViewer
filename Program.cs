var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} 

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
  .WithStaticAssets();

app.MapGet("/api/etf", async () =>
{
  using var client = new HttpClient();
  var json = await client.GetStringAsync("https://websys.fsit.com.tw/FubonETF/TWSE/FubonRealtime.aspx?type=A0010");
  return Results.Content(json, "application/json");
});
app.Run();
