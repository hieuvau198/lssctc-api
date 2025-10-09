using Lssctc.ProgramManagement.Classes.Mappings;
using Lssctc.ProgramManagement.Classes.Services;
using Lssctc.ProgramManagement.Courses.Mappings;
using Lssctc.ProgramManagement.Courses.Services;
using Lssctc.ProgramManagement.LearningMaterials.Mappings;
using Lssctc.ProgramManagement.LearningMaterials.Services;
using Lssctc.ProgramManagement.LearningRecords.Mappings;
using Lssctc.ProgramManagement.LearningRecords.Services;
using Lssctc.ProgramManagement.Learnings.Services;
using Lssctc.ProgramManagement.Programs.Mappings;
using Lssctc.ProgramManagement.Programs.Services;
using Lssctc.ProgramManagement.Quizzes.Services;
using Lssctc.ProgramManagement.SectionMaterials.Services;
using Lssctc.ProgramManagement.SectionPartitions.Services;
using Lssctc.ProgramManagement.SectionQuizzes.Services;
using Lssctc.ProgramManagement.Sections.Mappings;
using Lssctc.ProgramManagement.Sections.Services;
using Lssctc.ProgramManagement.Syllabuses.Services;
using Lssctc.ProgramManagement.TrainingResults.Services;
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
builder.Services.AddAutoMapper(typeof(ClassMapper));
builder.Services.AddAutoMapper(typeof(LearningMaterialMapper).Assembly);
builder.Services.AddAutoMapper(typeof(SectionMapper).Assembly);
builder.Services.AddAutoMapper(typeof(LearningRecordMapper).Assembly);

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IProgramService, ProgramService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<ISyllabusService, SyllabusService>();

builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISectionService, SectionService>();
builder.Services.AddScoped<ISectionPartitionService, SectionPartitionService>();
builder.Services.AddScoped<ISectionMaterialService, SectionMaterialService>();
builder.Services.AddScoped<ILearningMaterialService, LearningMaterialService>();
builder.Services.AddScoped<ISectionQuizService, SectionQuizService>();
builder.Services.AddScoped<ITrainingResultService, TrainingResultService>();
builder.Services.AddScoped<ILearningRecordService, LearningRecordService>();

builder.Services.AddScoped<ILearningsClassService, LearningsClassService>();
builder.Services.AddScoped<ILearningsSectionService, LearningsSectionService>();
builder.Services.AddScoped<ILearningsSectionPartitionService, LearningsSectionPartitionService>();
builder.Services.AddScoped<ILearningsSectionMaterialService, LearningsSectionMaterialService>();
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
#endregion

var app = builder.Build();

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

app.Run();
