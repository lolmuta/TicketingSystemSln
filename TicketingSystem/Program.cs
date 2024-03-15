using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketingSystem.LoginUtil;
using TicketingSystem.Repo;
using TicketingSystem.Service;
using TicketingSystem.Utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<DbHelper>();
builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<ActService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<TicketsUUIDService>();
builder.Services.AddScoped<PaidsService>();
builder.Services.AddScoped<EmailHelper>();

//實作jwt 登入
var configuration = builder.Configuration;
string SecretKey = configuration.GetValue<string>("JwtAuth:SecretKey");
string Issuer = configuration.GetValue<string>("JwtAuth:Issuer");
string Audience = configuration.GetValue<string>("JwtAuth:Audience");
double ExpirationHours = configuration.GetValue<double>("JwtAuth:ExpirationHours");

// 添加身份驗證服務
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Issuer,
        ValidAudience = Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey))
    };
});



// Add services to the container.
builder.Services.AddControllersWithViews();

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


app.UseAuthentication();
app.UseAuthorization();

// 自訂錯誤處理中介軟體
app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode == 401)
    {
        // 如果是 401 錯誤（未授權），則導航到登入頁面
        context.Response.Redirect("/Login/Index");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
