﻿using System;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json;
using Prime.Base.Messaging.Manager;
using Prime.Core;
using Prime.Core.Testing;
using Prime.Manager.Core;
using Prime.Manager.Messages;

namespace Prime.Manager
{
    public class ManagerService
    {
        private readonly ServerContext _context;
        private readonly IApiKeyService _apiKeyService;

        public readonly ILogger L;
        public readonly IMessenger M;
        
        public ManagerService(ServerContext context)
        {
            _context = context;
            _apiKeyService = new ApiKeyService(context);
            L = _context.L;
            M = _context.M;
        }

        private void Log(string text)
        {
            L.Log($"({typeof(ManagerService).Name}) : {text}");
        }
        
        public void Init()
        {
            M.RegisterAsync<ProvidersListRequestMessage>(this, ProvidersListHandler);
            M.RegisterAsync<PrivateProvidersListRequestMessage>(this, PrivateProvidersListHandler);
            M.RegisterAsync<ProviderDetailsRequestMessage>(this, ProviderDetailsHandler);
            M.RegisterAsync<ProviderSaveKeysRequestMessage>(this, ProviderKeysHandler);
            M.RegisterAsync<DeleteProviderKeysRequestMessage>(this, DeleteProviderKeysHandler);
            M.RegisterAsync<TestPrivateApiRequestMessage>(this, TestPrivateApiHandler);
            M.RegisterAsync<GenerateGuidRequestMessage>(this, FakeClientGuidHandler);
            M.RegisterAsync<ProviderHasKeysRequestMessage>(this, ProviderHasKeysHandler);
        }

        private void ProviderHasKeysHandler(ProviderHasKeysRequestMessage request)
        {
            Log("Checking if provider has keys...");
            var details = _apiKeyService.GetNetworkDetails(request.Id);
            var hasKeys = !string.IsNullOrWhiteSpace(details.Key) && !string.IsNullOrWhiteSpace(details.Secret);

            M.SendAsync(new ProviderHasKeysResponsetMessage(request, hasKeys));
        }

        private void FakeClientGuidHandler(GenerateGuidRequestMessage request)
        {
            Log("Generating client GUID...");

            var guid = Guid.NewGuid();
            M.SendAsync(new GenerateGuidResponseMessage(request, guid));
        }

        private void TestPrivateApiHandler(TestPrivateApiRequestMessage request)
        {
            Log("Testing private API...");
            TestPrivateApiResponseMessage msg;

            try
            {
                var success = _apiKeyService.TestPrivateApi(request.Id, request.Key, request.Secret, request.Extra);
                msg = new TestPrivateApiResponseMessage(request, success);
            }
            catch (Exception e)
            {
                Log($"Error while testing private API: {e.Message}");
                msg = new TestPrivateApiResponseMessage(request, false, e.Message);
            }

            M.SendAsync(msg);
        }

        private void DeleteProviderKeysHandler(DeleteProviderKeysRequestMessage request)
        {
            Log("Deleting keys...");
            DeleteProviderKeysResponseMessage msg;

            try
            {
                _apiKeyService.DeleteKeys(request.Id);

                msg = new DeleteProviderKeysResponseMessage(request, true);
            }
            catch (Exception e)
            {
                Log($"Error while deleting keys: {e.Message}");

                msg = new DeleteProviderKeysResponseMessage(request, false, e.Message);
            }

            M.SendAsync(msg);
        }

        private void ProviderKeysHandler(ProviderSaveKeysRequestMessage request)
        {
            Log("Saving keys...");
            ProviderSaveKeysResponseMessage msg;
            
            try
            {
                _apiKeyService.SaveKeys(request.Id, request.Key, request.Secret, request.Extra);

                msg = new ProviderSaveKeysResponseMessage(request, true);
            }
            catch (Exception e)
            {
                Log($"Error while saving keys: {e.Message}");

                msg = new ProviderSaveKeysResponseMessage(request, false, e.Message);
            }

            M.SendAsync(msg);
        }

        private void ProviderDetailsHandler(ProviderDetailsRequestMessage request)
        {
            Log("Sending provider details...");

            var response = _apiKeyService.GetNetworkDetails(request.Id);
            M.SendAsync(new ProviderDetailsResponseMessage(request, response));
        }

        private void PrivateProvidersListHandler(PrivateProvidersListRequestMessage request)
        {
            Log("Private providers list requested... Sending...");

            var response = _apiKeyService.GetPrivateNetworks().AsList();
            M.SendAsync(new PrivateProvidersListResponseMessage(request, response));
        }

        private void ProvidersListHandler(ProvidersListRequestMessage request)
        {
            Log("Providers list requested... Sending...");

            var response = _apiKeyService.GetNetworks().AsList();
            M.SendAsync(new ProvidersListResponseMessage(request, response));
        }

        public void Stop()
        {
            M.UnregisterAsync(this);
        }
    }
}