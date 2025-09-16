using Lssctc.ProgramManagement.Courses.Mappings;
using Lssctc.ProgramManagement.Courses.Services;
using Lssctc.ProgramManagement.Programs.Mappings;
using Lssctc.ProgramManagement.Programs.Services;
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
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});


#region Domain
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
#endregion

#region Application Services
//builder.Services.AddAutoMapper(typeof(CoursesMappingProfile));
//builder.Services.AddScoped<ICoursesService, CoursesService>();
builder.Services.AddAutoMapper(typeof(CourseMapper));
builder.Services.AddAutoMapper(typeof(ProgramMapper));
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IProgramService, ProgramService>();

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
