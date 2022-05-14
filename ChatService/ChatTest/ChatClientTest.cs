using ChatClient.Contracts;
using ChatClient.Models;
using ChatClient.Services;
using ChatServer.Contracts;
using ChatServer.Models;
using ChatServer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using Xunit;

namespace ChatTest
{
    [Collection("ChatClient")]
    public class ChatClientTest
    {
        private readonly ChatClientOptions _clientOptions;
        private readonly ChatServerOptions _serverOptions;
        private readonly ServiceProvider _serviceProvider;
        private readonly IChatServerService _chatServerService;

        public ChatClientTest()
        {
            _clientOptions = new ChatClientOptions();
            _clientOptions.Host = "127.0.0.1";
            _clientOptions.Port = 2022;

            _serverOptions = new ChatServerOptions();
            _serverOptions.Host = "127.0.0.1";
            _serverOptions.Port = 2022;

            var services = new ServiceCollection();

            //injection server service
            services.AddSingleton<IChatServerService, ChatServerService>();
            services.AddSingleton(_serverOptions);

            //injection client service
            services.AddSingleton<IChatClientService, ChatClientService>();
            services.AddSingleton(_clientOptions);

            _serviceProvider = services.BuildServiceProvider();

            _chatServerService = _serviceProvider.GetService<IChatServerService>();
            _chatServerService.Start();
        }

        [Theory]
        [InlineData("ismail")]
        public void ServerConnect_SuccessTest(string username)
        {
            var clientService = _serviceProvider.GetService<IChatClientService>();

            clientService.Connect(username);

            Assert.True(true);
        }

        [Theory]
        [InlineData("ismail", "Send message to everyone")]
        public void SendMessage_SuccessTest(string username, string message)
        {
            var clientService = _serviceProvider.GetService<IChatClientService>();

            clientService.Connect(username);

            clientService.SendMessage(message);

            Assert.True(true);
        }
    }
}
