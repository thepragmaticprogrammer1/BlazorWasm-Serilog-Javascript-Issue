using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BlazorAppNet8.Client;
using Microsoft.JSInterop;
using Serilog;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

string InstanceId = Guid.NewGuid().ToString("n");
var logConfig = new LoggerConfiguration()
    // .Enrich.WithProperty("InstanceId", InstanceId)
    .WriteTo.BrowserConsole();
Log.Logger = logConfig.CreateLogger();

// Remove this line and Javascript import works first time as expected
Log.Information("App Instance Id:{InstanceId}", InstanceId);

// builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var host = builder.Build();

var js = host.Services.GetRequiredService<IJSRuntime>();

// This should work....
// var module = await js.InvokeAsync<IJSObjectReference>("import", "./localeHelper.js");
// var message = await module.InvokeAsync<string>("showPrompt", "hello");


// Workaround: Get the module in a loop - second try usually succeeds. Race condition?
int tries = 0;
IJSObjectReference? module = null;
while (module is null && tries < 5)
{
    Console.WriteLine($"Trying to import JS module");
    module = await js.InvokeAsync<IJSObjectReference>("import", "./localeHelper.js");
    tries++;
}

string message = "";
if (module is not null)
    message = await module.InvokeAsync<string>("showPrompt", "hello");

Console.WriteLine($"Message: {message}");

await host.RunAsync();