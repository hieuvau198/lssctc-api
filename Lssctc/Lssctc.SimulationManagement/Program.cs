using Lssctc.Share.Contexts;
using Lssctc.Share.Implements;
using Lssctc.Share.Interfaces;
using Lssctc.SimulationManagement.Components.Mappings;
using Lssctc.SimulationManagement.Components.Services;
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
builder.Services.AddAutoMapper(typeof(ComponentMappingProfile));
builder.Services.AddScoped<IComponentService, ComponentService>();

#endregion

#region Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClients",
        policy => policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // If use cookies/auth, else remove this line
    );
});
#endregion



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowClients");

app.UseAuthorization();

app.MapControllers();

app.Run();
