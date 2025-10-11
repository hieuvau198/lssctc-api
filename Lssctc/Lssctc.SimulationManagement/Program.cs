using Lssctc.Share.Contexts;
using Lssctc.Share.Implements;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.Components.Mappings;
using Lssctc.SimulationManagement.Components.Services;
using Lssctc.SimulationManagement.Practices.Services;

using Lssctc.SimulationManagement.PracticeStepComponents.Services;
using Lssctc.SimulationManagement.PracticeSteps.Services;
using Lssctc.SimulationManagement.SectionPractice.Mappings;
using Lssctc.SimulationManagement.SectionPractice.Services;
using Lssctc.SimulationManagement.SimActions.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
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
builder.Services.AddAutoMapper(typeof(ComponentMappingProfile), typeof(SectionPracticeMapper));
builder.Services.AddScoped<IComponentService, ComponentService>();
builder.Services.AddScoped<IPracticeService, PracticeService>();
builder.Services.AddScoped<IPracticeStepService, PracticeStepService>();
builder.Services.AddScoped<IPracticeStepComponentService, PracticeStepComponentService>();
builder.Services.AddScoped<ISectionPracticeService, SectionPracticeService>();

builder.Services.AddScoped<ISimActionService, SimActionService>();
#endregion

#region Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients",
        policy => policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
    );
});
#endregion

var app = builder.Build();

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowClients");

app.UseAuthorization();

app.MapControllers();

app.Run();
