using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using QLTours.Data;
using QLTours.Models;
using QLTours.Services;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddTransient<IEmailSender, EmailSender>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
	options.Cookie.Name = "YourCookieName";
	options.Cookie.HttpOnly = true;
	options.LoginPath = "/home/Login";
	options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian hết hạn của cookie
})
.AddGoogle(googleOptions =>
{
	googleOptions.ClientId = builder.Configuration["Google:ClientId"];
	googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"];
	googleOptions.Scope.Add("email");
});

// Add services to the container.

builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddAuthorization();
builder.Services.AddSession();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<ImageTourService>();
builder.Services.AddScoped<TourDAO>();
builder.Services.AddScoped<UserDAO>();
builder.Services.AddScoped<BookingDAO>();
builder.Services.AddScoped<ContactDAO>();
//Dependency Injection
builder.Services.AddDbContext<QuanLyTourContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("QLTour"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseStaticFiles();

app.UseRouting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/{action=Index}/{id?}",
	defaults: new { controller = "Home", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Home/{action=Login}/{id?}",
	defaults: new { controller = "Home", action = "Login" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Manage/{action=Index}/{id?}",
	defaults: new { controller = "Manage", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Bookings/{action=Index}/{id?}",
	defaults: new { controller = "Bookings", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Contact/{action=Index}/{id?}",
	defaults: new { controller = "Contact", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Users/{action=Index}/{id?}",
	defaults: new { controller = "Users", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Promotions/{action=Index}/{id?}",
	defaults: new { controller = "Promotions", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Admin",
	pattern: "Admin/Tours2/{action=Index}/{id?}",
	defaults: new { controller = "Tours2", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/{action=Index}/{id?}",
	defaults: new { controller = "Home", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/Tours/{action=Index}/{id?}",
	defaults: new { controller = "Tours", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/Bookings/{action=Index}/{id?}",
	defaults: new { controller = "Bookings", action = "Index" });

app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/Categories/{action=Index}/{id?}",
	defaults: new { controller = "Categories", action = "Index" });

app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/Vehicles/{action=Index}/{id?}",
	defaults: new { controller = "Vehicles", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/Hotels/{action=Index}/{id?}",
	defaults: new { controller = "Hotels", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/Itineraries/{action=Index}/{id?}",
	defaults: new { controller = "Itineraries", action = "Index" });
app.MapAreaControllerRoute(
	name: "EmployeeItineraryImages",
	areaName: "Employee",
	pattern: "Employee/ItineraryImages/{action=Index}/{id?}",
	defaults: new { controller = "ItineraryImages", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/DetailItineraries/{action=Index}/{id?}",
	defaults: new { controller = "DetailItineraries", action = "Index" });
app.MapAreaControllerRoute(
	name: "MyAreas",
	areaName: "Employee",
	pattern: "Employee/TourDetails/{action=Index}/{id?}",
	defaults: new { controller = "TourDetails", action = "Index" });

app.MapControllerRoute(
	name: "home",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
	name: "booking",
	pattern: "{controller=Home}/{action=BookingDetails}");
app.MapControllerRoute(
	name: "login",
	pattern: "{controller=Home}/{action=Login}");
app.MapControllerRoute(
	name: "register",
	pattern: "{controller=Home}/{action=Register}");
app.MapControllerRoute(
	name: "review",
	pattern: "{controller=Home}/{action=AddReview}");
app.MapControllerRoute(
	name: "SendMessage",
	pattern: "{controller=Home}/{action=SendMessage}");
app.MapControllerRoute(
	name: "PaymentCallback",
	pattern: "{controller=Home}/{action=PaymentCallback}");
app.MapControllerRoute(
	name: "CreatePaymentUrl",
	pattern: "{controller=Home}/{action=CreatePaymentUrl}");
app.MapControllerRoute(
	name: "PaymentSuccess",
	pattern: "{controller=Home}/{action=PaymentSuccess}");
app.MapControllerRoute(
	name: "PaymentFailed",
	pattern: "{controller=Home}/{action=PaymentFailed}");
app.MapControllerRoute(
	name: "PaymentFailed",
	pattern: "{controller=Acc}/{action=PaymentFailed}");
app.MapControllerRoute(
	name: "ForgotPassword",
	pattern: "{controller=Account}/{action=ForgotPassword}");
app.MapControllerRoute(
	name: "MyProfile",
	pattern: "{controller=Account}/{action=MyProfile}");
app.MapControllerRoute(
	name: "ChangePassword",
	pattern: "{controller=Account}/{action=ChangePassword}");
app.MapControllerRoute(
	name: "ResetPassword",
	pattern: "{controller=Account}/{action=ResetPassword}/{resetCode}/{email}");


app.Run();
