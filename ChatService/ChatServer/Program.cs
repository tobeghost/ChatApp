using ChatServer.Contracts;
using ChatServer.Models;
using ChatServer.Services;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChatServer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            //injection server service
            services.AddSingleton<IChatServerService, ChatServerService>();

            //injection server config
            Parser.Default.ParseArguments<ChatServerOptions>(args).WithParsed(opt =>
            {
                services.AddSingleton(opt);
            });

            var serviceProvider = services.BuildServiceProvider();

            var serverService = serviceProvider.GetRequiredService<IChatServerService>();

            serverService.Start();
        }
    }
}
