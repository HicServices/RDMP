using CommandLine;
using FAnsi.Implementation;
using FAnsi.Implementations.MicrosoftSQL;
using FAnsi.Implementations.MySql;
using FAnsi.Implementations.Oracle;
using FAnsi.Implementations.PostgreSql;
using Rdmp.Core.ReusableLibraryCode;
using ReactUI.Server;
using System.Drawing.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
UsefulStuff.GetParser().ParseArguments<RDMPBootstrapOptions>(args).MapResult(Setup, _ => -1);

static object Setup(RDMPBootstrapOptions args)
{
    ImplementationManager.Load<MicrosoftSQLImplementation>();
    ImplementationManager.Load<MySqlImplementation>();
    ImplementationManager.Load<OracleImplementation>();
    ImplementationManager.Load<PostgreSqlImplementation>();
    RDMPInitialiser.Init(args);
    return 0;
}

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
