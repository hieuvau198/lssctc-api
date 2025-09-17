using LearnerService.Application.Interfaces;
using LearnerService.Application.Mappings;
using LearnerService.Application.Services;
using LearnerService.Domain.Contexts;
using LearnerService.Domain.Implements;
using LearnerService.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region DbContext

builder.Services.AddDbContext<LearnerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("lssctcDb")));
#endregion

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Application Services

builder.Services.AddAutoMapper(typeof(LearnersMappingProfile));
builder.Services.AddScoped<ILearnersService, LearnersService>();

#endregion

#region Repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

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
