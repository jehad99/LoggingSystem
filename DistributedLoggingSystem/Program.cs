using Amazon.Runtime;
using Amazon.S3;
using DistributedLoggingSystem.Models;
using DistributedLoggingSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// needs to be singleton
builder.Services.AddDbContext<LoggingDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
}, ServiceLifetime.Singleton);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var config = new AmazonS3Config
    {
        ServiceURL = builder.Configuration["AWS:ServiceURL"], // MinIO endpoint
        ForcePathStyle = true // Ensure compatibility with MinIO
    };

    var credentials = new BasicAWSCredentials(
        builder.Configuration["AWS:AccessKeyId"], // Access key
        builder.Configuration["AWS:SecretAccessKey"] // Secret key
    );

    return new AmazonS3Client(credentials, config);
});

builder.Services.AddSingleton<BatchLogService>(); 
var app = builder.Build();

// Ensure the logs bucket exists on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var batchLogService = services.GetRequiredService<BatchLogService>();

    await batchLogService.EnsureBucketExistsAsync();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseCors(builder =>
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
        
app.UseAuthorization();

app.MapControllers();

app.Run();
