using CarAdvisor.API.Plugins;
using CarAdvisor.Business;
using CarAdvisor.Business.Abstract;
using CarAdvisor.Business.Concrete;
using CarAdvisor.Business.Mappings;
using CarAdvisor.DataAccess.Abstract;
using CarAdvisor.DataAccess.Concrete;
using CarAdvisor.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Dependency Injection Configuration
builder.Services.AddScoped<ICarDal, EfCarDal>();

builder.Services.AddScoped<ICarManager, CarManager>();

// Automapper configuration
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// 1. API AnahtarÃ„Â±nÃ„Â± ve Model AdÃ„Â±nÃ„Â± Belirliyoruz
// (GerÃƒÂ§ek projede bu key'i appsettings.json'da saklamak daha gÃƒÂ¼venlidir)
string geminiApiKey = builder.Configuration["Gemini:ApiKey"] ?? "";
string modelId = builder.Configuration["Gemini:ModelId"] ?? "gemini-flash-latest"; // Hem hızlı hem ekonomik model

// 2. Semantic Kernel'i Google Gemini ile Kuruyoruz
builder.Services.AddKernel()
       .AddGoogleAIGeminiChatCompletion(
           modelId: modelId,
           apiKey: geminiApiKey);

builder.Services.AddTransient<CarDatabasePlugin>();
builder.Services.AddDbContext<CarAdvisorContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        b => b.WithOrigins("http://localhost:5173") // React uygulamanÃ„Â±n adresi
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowAnyOrigin());
});
// 1. Hangfire'Ã„Â± ve SQL veritabanÃ„Â± baÃ„Å¸lantÃ„Â±sÃ„Â±nÃ„Â± kaydediyoruz
//builder.Services.AddHangfire(configuration => configuration
//    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
//    .UseSimpleAssemblyNameTypeSerializer()
//    .UseRecommendedSerializerSettings()
//    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"))); // Senin SQL baÃ„Å¸lantÃ„Â± dizesi adÃ„Â±n neyse onu yaz

//// 2. Hangfire Server'Ã„Â± (Ã„Â°Ã…Å¸ÃƒÂ§iyi) ÃƒÂ§alÃ„Â±Ã…Å¸tÃ„Â±rÃ„Â±yoruz
//builder.Services.AddHangfireServer(options =>
//{
//    options.WorkerCount = 1; // BilgisayarÃ„Â± ve hedef siteyi yormamak iÃƒÂ§in SADECE 1 Ã„Â°Ã…ÂÃƒâ€¡Ã„Â° ÃƒÂ§alÃ„Â±Ã…Å¸sÃ„Â±n!
//});
//// 3. Ã„Â°leriki adÃ„Â±mda yazacaÃ„Å¸Ã„Â±mÃ„Â±z Kafka Dinleyicisini arka plan servisi olarak ekliyoruz
//builder.Services.AddHostedService<KafkaConsumerService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// 1. CORS Ã„Â°zni TanÃ„Â±mlama

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthorization();

app.MapControllers();
// Hangfire Dashboard'u aktif ediyoruz (O efsanevi izleme paneli)
//app.UseHangfireDashboard("/hangfire");
app.Run();
