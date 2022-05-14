using ChatClient.Contracts;
using ChatClient.Models;
using ChatClient.Services;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace ChatClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            //injection server service
            services.AddSingleton<IChatClientService, ChatClientService>();

            //injection server config
            Parser.Default.ParseArguments<ChatClientOptions>(args).WithParsed(opt =>
            {
                services.AddSingleton(opt);
            });

            var serviceProvider = services.BuildServiceProvider();

            // Welcome and User Nickname

            var connection = serviceProvider.GetRequiredService<IChatClientService>();

            Console.Clear();
            Console.WriteLine("*** Welcome to our chat service!");
            Console.WriteLine("*** Please set your nickname:");

            var nickname = Console.ReadLine();

            connection.Connect(nickname);

            if (connection.UserAlreadyTaken)
            {
                Program.Main(new string[] { "User Already Taken" });
            }
            else
            {
                Console.Clear();
                Console.WriteLine("*** Welcome to the chat! ***");

                while (true)
                {
                    var message = Console.ReadLine();
                    if (!string.IsNullOrEmpty(message))
                    {
                        connection.SendMessage(message);
                    }
                }
            }

            Console.ReadKey();
        }
    }
}
