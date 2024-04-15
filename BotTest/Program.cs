using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TextCommandFramework.Services;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace TextCommandFramework;

// This is a minimal example of using Discord.Net's command
// framework - by no means does it show everything the framework
// is capable of.
//
// You can find samples of using the command framework:
// - Here, under the 02_commands_framework sample
// - https://github.com/foxbot/DiscordBotBase - a bare-bones bot template
// - https://github.com/foxbot/patek - a more feature-filled bot, utilizing more aspects of the library
class Program
{
    // There is no need to implement IDisposable like before as we are
    // using dependency injection, which handles calling Dispose for us.
    public static async Task Main(string[] args)
    {
        // You should dispose a service provider created using ASP.NET
        // when you are finished using it, at the end of your app's lifetime.
        // If you use another dependency injection framework, you should inspect
        // its documentation for the best way to do this.
        await using var services = ConfigureServices();
        var client = services.GetRequiredService<DiscordSocketClient>();

        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        client.Log += LogAsync;
        services.GetRequiredService<CommandService>().Log += LogAsync;


        // Tokens should be considered secret data and never hard-coded.
        // We can read from the environment variable to avoid hard coding.
        await client.LoginAsync(TokenType.Bot, config["DiscordKey"]);
        await client.StartAsync();

        // Here we initialize the logic required to register our commands.
        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

        await Task.Delay(Timeout.Infinite);
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());

        return Task.CompletedTask;
    }

    private static ServiceProvider ConfigureServices()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        path = System.IO.Path.Join(path, "bot.db");

        var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            .AddSingleton<HttpClient>();

        services.AddDbContext<BotContext>(
            options => options.UseSqlite($"Data Source={path}"));


        return services.BuildServiceProvider();
    }
}
