using Microsoft.EntityFrameworkCore;
using CreditService.Infrastructure.Persistence;
using MediatR;
var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddDbContext<CreditDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMediatR(typeof(CreditService.Infrastructure.Handlers.CreateCreditHandler).Assembly);

builder.Services.AddScoped<global::AuditService.Api.Handlers.CreditEventHandler>();
builder.Services.AddHostedService<global::AuditService.Api.Services.ServiceBusSubscriber>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
