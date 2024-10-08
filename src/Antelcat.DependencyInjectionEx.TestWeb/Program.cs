using System.Diagnostics;
using Antelcat.DependencyInjectionEx;
using Antelcat.DependencyInjectionEx.TestWeb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddScoped<IA, A>()
    .AddScoped<IB, B>();

builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseAutowiredServiceProviderFactory();

var app = builder.Build();

var b = app.Services.GetRequiredService<IB>();

(app.Services as ServiceProviderEx)!.ServiceResolved += (p, type, controller, kind) =>
{
    if (controller is ControllerBase)
    {
        Debugger.Break();
    }
};

Debug.Assert(b is B { A : not null });
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
