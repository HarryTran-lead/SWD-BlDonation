using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SWD_BLDONATION.MappingProfiles;
using SWD_BLDONATION.Models.Generated;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Thêm cấu hình DbContext
builder.Services.AddDbContext<BloodDonationContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Đăng ký AutoMapper (bắt buộc để inject IMapper)
builder.Services.AddAutoMapper(typeof(Program));

// Đăng ký AutoMapper cho toàn bộ profile

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());


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
