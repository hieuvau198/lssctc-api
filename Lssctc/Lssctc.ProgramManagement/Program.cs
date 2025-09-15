using Lssctc.Share.Contexts;
using Lssctc.Share.Implements;
using Lssctc.Share.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


#region DbContext

builder.Services.AddDbContext<LssctcDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("lssctcDb")));
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


#region Domain
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
#endregion

#region Application Services
//builder.Services.AddAutoMapper(typeof(CoursesMappingProfile));
//builder.Services.AddScoped<ICoursesService, CoursesService>();
#endregion

var app = builder.Build();

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
