using ChatServer.Contracts;
using ChatServer.Models;
using ChatServer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Sockets;
using Xunit;

namespace ChatTest
{
    [Collection("ChatServer")]
    public class ChatServerTest
    {
        private readonly ChatServerOptions _serverOptions;
        private readonly ServiceProvider _serviceProvider;
        private readonly IChatServerService _chatServerService;

        public ChatServerTest()
        {
            _serverOptions = new ChatServerOptions();
            _serverOptions.Host = "127.0.0.1";
            _serverOptions.Port = 2022;

            var services = new ServiceCollection();

            //injection server service
            services.AddSingleton<IChatServerService, ChatServerService>();
            services.AddSingleton(_serverOptions);

            _serviceProvider = services.BuildServiceProvider();

            _chatServerService = _serviceProvider.GetRequiredService<IChatServerService>();

            _chatServerService.Start();
        }

        [Fact]
        public void ServerIsRunning_SuccessTest()
        {
            var tcpClient = new TcpClient();

            tcpClient.Connect(_serverOptions.Host, _serverOptions.Port);

            Assert.True(tcpClient.Connected);
        }

        [Theory]
        [InlineData("Ismail")]
        public void CreateUser_SuccessTest(string nickname)
        {
            var tcpClient = new TcpClient();

            tcpClient.Connect(_serverOptions.Host, _serverOptions.Port);

            _chatServerService.CreateUser(tcpClient, nickname);

            Assert.True(true);
        }

        [Theory]
        [InlineData("Ismail")]
        public void DeleteUser_SuccessTest(string nickname)
        {
            var tcpClient = new TcpClient();

            tcpClient.Connect(_serverOptions.Host, _serverOptions.Port);

            _chatServerService.CreateUser(tcpClient, nickname);

            _chatServerService.DeleteUser(tcpClient);

            Assert.True(true);
        }

        [Theory]
        [InlineData("Ismail", "Send my message")]
        public void SendMessage_SuccessTest(string nickname, string message)
        {
            var tcpClient = new TcpClient();

            tcpClient.Connect(_serverOptions.Host, _serverOptions.Port);

            _chatServerService.CreateUser(tcpClient, nickname);

            _chatServerService.SendMessage(nickname, message);

            Assert.True(true);
        }
    }
}
