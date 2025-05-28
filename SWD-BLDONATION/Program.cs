using Microsoft.EntityFrameworkCore;
<<<<<<< HEAD
using SWD_BLDONATION.Models;
=======
using SWD_BLDONATION.Models.Generated;
>>>>>>> b1f64584babe8ed46d30d51d12076773d51385d4

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Thêm cấu hình DbContext
builder.Services.AddDbContext<BloodDonationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Thêm CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // địa chỉ frontend
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bật CORS trước Authorization
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();
app.UseStaticFiles();  
app.UseAuthorization();
app.MapControllers();
app.Run();
