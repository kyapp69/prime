﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Prime.KeysManager.Core;
using Prime.KeysManager.Core.Models;
using Prime.KeysManager.Messages;
using Prime.KeysManager.Transport;

namespace Prime.KeysManager
{
    public class KeysManagerApp
    {
        private readonly ITcpServer _tcpServer;
        private readonly IPrimeService _primeService;

        public short PortNumber { get; set; } = 19991;
        public IPAddress IpAddress { get; set; } = IPAddress.Parse("127.0.0.1");
        
        public KeysManagerApp(ITcpServer tcpServer, IPrimeService primeService)
        {
            _tcpServer = tcpServer;
            _primeService = primeService;
        }
        
        public void Run()
        {
            Subscribe();
            _tcpServer.ExceptionOccurred += TcpServerOnExceptionOccurred;

            // Main app cycle.
            _tcpServer.StartServer(IpAddress, PortNumber);     
        }

        private void Subscribe()
        {
            _tcpServer.Subscribe<ProvidersListMessage>(ProvidersListHandler);
            _tcpServer.Subscribe<PrivateProvidersListMessage>(PrivateProvidersListHandler);
            _tcpServer.Subscribe<ProviderDetailsMessage>(ProviderDetailsHandler);
            _tcpServer.Subscribe<ProviderKeysMessage>(ProviderKeysHandler);
            _tcpServer.Subscribe<DeleteProviderKeysMessage>(DeleteProviderKeysHandler);
            _tcpServer.Subscribe<TestPrivateApiMessage>(TestPrivateApiHandler);
        }

        private void TestPrivateApiHandler(TestPrivateApiMessage testPrivateApiMessage)
        {
            Console.WriteLine("Testing private API...");
            var success = true;

            try
            {
                success = _primeService.TestPrivateApi(testPrivateApiMessage.Id, testPrivateApiMessage.Key, testPrivateApiMessage.Secret, testPrivateApiMessage.Extra);
            }
            catch (Exception e)
            {
                success = false;
                Console.WriteLine($"Error while testing private API: {e.Message}");
            }

            _tcpServer.Send(new OperationResultModel() { Success = success });
        }

        private void DeleteProviderKeysHandler(DeleteProviderKeysMessage deleteProviderKeysMessage)
        {
            Console.WriteLine("Deleting keys...");
            var success = true;

            try
            {
                _primeService.DeleteKeys(deleteProviderKeysMessage.Id);
            }
            catch (Exception e)
            {
                success = false;
                Console.WriteLine($"Error while deleting keys: {e.Message}");
            }

            _tcpServer.Send(new OperationResultModel() { Success = success });
        }

        private void ProviderKeysHandler(ProviderKeysMessage providerKeysMessage)
        {
            Console.WriteLine("Saving keys...");
            var success = true;
            
            try
            {
                _primeService.SaveKeys(providerKeysMessage.Id, providerKeysMessage.Key, providerKeysMessage.Secret, providerKeysMessage.Extra);
            }
            catch (Exception e)
            {
                success = false;
                Console.WriteLine($"Error while saving keys: {e.Message}");
            }
            
            _tcpServer.Send(new OperationResultModel() { Success = success});
        }

        private void ProviderDetailsHandler(ProviderDetailsMessage providerDetailsMessage)
        {
            Console.WriteLine("Sending provider details...");

            var providerDetails = _primeService.GetNetworkDetails(providerDetailsMessage.Id);
            _tcpServer.Send(providerDetails);
        }

        private void PrivateProvidersListHandler(PrivateProvidersListMessage privateProvidersListMessage)
        {
            Console.WriteLine("Private providers list requested... Sending...");

            var providers = _primeService.GetPrivateNetworks();
            _tcpServer.Send(providers);
        }

        private void ProvidersListHandler(ProvidersListMessage providersListMessage)
        {
            Console.WriteLine("Providers list requested... Sending...");

            var providers = _primeService.GetNetworks();
            _tcpServer.Send(providers);
        }

        private void TcpServerOnExceptionOccurred(object sender, Exception exception)
        {
            Console.WriteLine($"Server error occured: {exception.Message}");
        }

        public void Exit()
        {
            _tcpServer.ShutdownServer();
        }
    }
}