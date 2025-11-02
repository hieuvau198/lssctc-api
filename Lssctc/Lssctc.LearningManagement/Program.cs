
using Lssctc.Share.Contexts;
using Lssctc.Share.Implements;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region DbContext
builder.Services.AddDbContext<LssctcDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("lssctcDb")));
#endregion

// AutoMapper

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Domain
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

#endregion

#region Application Services

#endregion

// ================== ADD CORS ==================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()   // Cho phép tất cả origin (FE có thể đổi thành cụ thể)
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
// ==============================================

var app = builder.Build();

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ================== USE CORS ==================
app.UseCors("AllowAll");
// ==============================================

app.UseAuthorization();

app.MapControllers();

app.Run();
