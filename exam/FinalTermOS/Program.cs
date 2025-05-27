using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FinalTermOS.Services; // 確保引入你的 Service 命名空間

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// === 在這裡添加你的 Service 註冊 ===
// 由於 SleepingTASimulationService 負責管理模擬的狀態（執行緒、號誌等），
// 應該讓整個應用程式共用同一個實例，所以註冊為 Singleton。
builder.Services.AddSingleton<SleepingTASimulationService>();
builder.Services.AddSingleton<DiningPhilosophersSimulationService>();
builder.Services.AddSingleton<BankersAlgorithmService>(); 
builder.Services.AddSingleton<VirtualMemorySimulationService>();
// =================================

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

app.Run();