using System.Diagnostics;
using Antelcat.DependencyInjectionEx.TestWeb;

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
