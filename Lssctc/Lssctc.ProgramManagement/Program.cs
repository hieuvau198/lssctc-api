using Lssctc.ProgramManagement.Accounts.Authens.Dtos;
using Lssctc.ProgramManagement.Accounts.Authens.Services;
using Lssctc.ProgramManagement.Accounts.Profiles.Services;
using Lssctc.ProgramManagement.Accounts.Users.Services;
using Lssctc.ProgramManagement.Activities.Services;
using Lssctc.ProgramManagement.BrandModel.Services;
using Lssctc.ProgramManagement.Certificates.Services;
using Lssctc.ProgramManagement.ClassManage.ActivityRecords.Services;
using Lssctc.ProgramManagement.ClassManage.Classes.Services;
using Lssctc.ProgramManagement.ClassManage.Enrollments.Services;
using Lssctc.ProgramManagement.ClassManage.FinalExams.Services;
using Lssctc.ProgramManagement.ClassManage.Helpers;
using Lssctc.ProgramManagement.ClassManage.PracticeAttempts.Services;
using Lssctc.ProgramManagement.ClassManage.Progresses.Services;
using Lssctc.ProgramManagement.ClassManage.QuizAttempts.Services;
using Lssctc.ProgramManagement.ClassManage.SectionRecords.Services;
using Lssctc.ProgramManagement.ClassManage.Timeslots.Services;
using Lssctc.ProgramManagement.Common.Services;
using Lssctc.ProgramManagement.Courses.Services;
using Lssctc.ProgramManagement.Dashboard.Services;
using Lssctc.ProgramManagement.Materials.Services;
using Lssctc.ProgramManagement.Practices.Services;
using Lssctc.ProgramManagement.Programs.Services;
using Lssctc.ProgramManagement.Quizzes.Services;
using Lssctc.ProgramManagement.Sections.Services;
using Lssctc.ProgramManagement.SimulationComponents.Services;
using Lssctc.Share.Contexts;
using Lssctc.Share.Implements;
using Lssctc.Share.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OfficeOpenXml;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// Configure EPPlus License for version 8.x
// EPPlus 8.x requires license to be set before first use
ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

#region DbContext

builder.Services.AddDbContext<LssctcDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("lssctcDb")));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}

    ).AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
            ValidAudience = builder.Configuration["JwtConfig:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"] ?? "JWT Key Not Found")),
            ClockSkew = TimeSpan.Zero
        };

        // check if token has been revoked (stored in IDistributedCache)
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                if (string.IsNullOrEmpty(jti))
                {
                    // fallback: compute key from raw token
                    var auth = context.Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
                    {
                        var token = auth[7..].Trim();
                        jti = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(token));
                    }
                }

                if (!string.IsNullOrEmpty(jti))
                {
                    var val = await cache.GetStringAsync($"revoked_jti:{jti}");
                    if (val != null)
                    {
                        context.Fail("Token revoked");
                    }
                }
            }
        };


    });
builder.Services.AddAuthorization();
#endregion

#region Controllers & Swagger

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

#endregion

#region Domain
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

#endregion

#region Application Services

builder.Services.AddScoped<IGoogleOAuthService, GoogleOAuthService>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IProgramsService, ProgramsService>();
builder.Services.AddScoped<IProgramCoursesService, ProgramCoursesService>();
builder.Services.AddScoped<ICoursesService, CoursesService>();

builder.Services.AddScoped<ISectionsService, SectionsService>();
builder.Services.AddScoped<IActivitiesService, ActivitiesService>();

builder.Services.AddScoped<IMaterialsService, MaterialsService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IQuizValidator, QuizValidator>();
builder.Services.AddScoped<IQuizExcelProcessor, QuizExcelProcessor>();
builder.Services.AddScoped<IPracticesService, PracticesService>();
builder.Services.AddScoped<ITraineePracticesService, TraineePracticesService>();
builder.Services.AddScoped<ITasksService, TasksService>();

builder.Services.AddScoped<IClassesService, ClassesService>();
builder.Services.AddScoped<IClassInstructorsService, ClassInstructorsService>();
builder.Services.AddScoped<IEnrollmentsService, EnrollmentsService>();
builder.Services.AddScoped<IProgressesService, ProgressesService>();

builder.Services.AddScoped<ISectionRecordsService, SectionRecordsService>();
builder.Services.AddScoped<IActivityRecordsService, ActivityRecordsService>();
builder.Services.AddScoped<IQuizAttemptsService, QuizAttemptsService>();
builder.Services.AddScoped<IPracticeAttemptsService, PracticeAttemptsService>();

builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IProfilesService, ProfilesService>();
builder.Services.AddScoped<IInstructorProfilesService, InstructorProfilesService>();
builder.Services.AddScoped<IAuthensService, AuthensService>();
builder.Services.AddScoped<IQuizQuestionService, QuizQuestionService>();
builder.Services.AddScoped<IQuizQuestionOptionsService, QuizQuestionOptionsService>();
builder.Services.AddScoped<IBrandModel, Lssctc.ProgramManagement.BrandModel.Services.BrandModel>();
builder.Services.AddScoped<ISimulationComponentService, SimulationComponentService>();
builder.Services.AddScoped<IProfilesService, ProfilesService>();
builder.Services.AddScoped<IOtpService, OtpService>();

builder.Services.AddScoped<ICertificatesService, CertificatesService>();
builder.Services.AddScoped<ITraineeCertificatesService, TraineeCertificatesService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IInstructorDashboardService, InstructorDashboardService>();
builder.Services.AddScoped<ISimulationManagerDashboardService, SimulationManagerDashboardService>();

builder.Services.AddScoped<IProgramImportService, ProgramImportService>();

builder.Services.AddScoped<ITimeslotService, TimeslotService>();
builder.Services.AddScoped<IActivitySessionService, ActivitySessionService>();
builder.Services.AddScoped<IFinalExamsService, FinalExamsService>();
builder.Services.AddScoped<IFinalExamPartialService, FinalExamPartialService>();
builder.Services.AddScoped<IFinalExamSeService, FinalExamSeService>();
builder.Services.AddScoped<IFETemplateService, FETemplateService>();

builder.Services.AddScoped<IClassCustomizeService, ClassCustomizeService>();
builder.Services.AddScoped<IClassCompleteService, ClassCompleteService>();
#endregion

#region CORS
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//    {
//        policy.AllowAnyOrigin()
//              .AllowAnyMethod()
//              .AllowAnyHeader();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        option =>
        {
            option.WithOrigins(
                "http://localhost:5173", 
                "http://20.2.88.115.nip.io", 
                "https://20.2.88.115.nip.io",
                "https://lssctc.site")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// 1. Bind configuration from appsettings to EmailSettings class
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// 2. Register MailService as Transient (creates new instance each time to ensure clean connection)
builder.Services.AddTransient<IMailService, MailService>();
#endregion

#region Firebase
builder.Services.AddSingleton<IFirebaseStorageService, FirebaseStorageService>();
#endregion

// add distributed cache so we can store revoked token JTIs without DB
builder.Services.AddDistributedMemoryCache();

// add memory cache for OTP storage
builder.Services.AddMemoryCache();

var app = builder.Build();

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigin");

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
