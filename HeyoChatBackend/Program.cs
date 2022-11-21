using HeyoChatBackend;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services.AddSignalR();

services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

services.AddSingleton<IDictionary<string, UserConnection>>(
    opts => new Dictionary<string, UserConnection>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseCors();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatSignalRHub>("/chat");
});

app.Run();