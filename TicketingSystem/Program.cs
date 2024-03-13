using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketingSystem.LoginUtil;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<JwtService>();
//實作jwt 登入
//todo appsetting
string _secretKey = "yourSecretKeyaaaaaaaaaaaaaaaaaaaaaaa";
string issuer = "yourIssuer";
string audience = "yourAudience";
double expirationHours = 1;
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
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey))
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
