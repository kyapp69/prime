﻿using Prime.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prime.Utility;

namespace Prime.Ui.Wpf.ViewModel
{
    public class ServiceEditViewModel : VmBase, IDataErrorInfo
    {
        private readonly DebounceDispatcher _debounceDispatcher;

        public ServiceEditViewModel() : this(Networks.I.Providers.OfType<INetworkProviderPrivate>().Skip(1).FirstOrDefault())
        {
        }

        public ServiceEditViewModel(INetworkProviderPrivate provider)
        {
            _debounceDispatcher = new DebounceDispatcher();
            Service = provider;
            Configuration = Service.GetApiConfiguration;

            if (Configuration == null)
                throw new ArgumentException("No API Configuration object for " + Service.GetType());

            UserKey = UserContext.Current.GetApiKey(Service);

            if (UserKey != null)
            {
                _apiKey = UserKey.Key;
                _apiSecret = UserKey.Secret;
                _apiExtra1 = UserKey.Extra;
                _apiName = UserKey.Name;
                DecideTest(0);
            }
            else
                _apiName = Service.Title;

            this.PropertyChanged += ServiceEditViewModel_PropertyChanged;
        }

        private void ServiceEditViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var prop = e.PropertyName;
            if (prop != nameof(ApiKey) && prop != nameof(ApiSecret) && prop != nameof(ApiExtra1) && prop != nameof(ApiName))
                return;

            DecideTest();
        }

        private void DecideTest(int debounceInterval = 700)
        {
            StatusText = "";
            StatusResult = false;

            var allin = this[nameof(ApiName)] == null && this[nameof(ApiKey)] == null && this[nameof(ApiSecret)] == null &&
                        this[nameof(ApiExtra1)] == null;
            if (!allin)
                return;

            _debounceDispatcher.Debounce(debounceInterval, o => CheckKeys());
        }

        private void CheckKeys()
        {
            StatusText = "Checking the keys now...";

            var apikey = new ApiKey(_apiName, _apiKey, _apiSecret, _apiExtra1);
            var t = ApiCoordinator.TestApiAsync(Service, new ApiTestContext(apikey));
            t.ContinueWith(task => ApiKeyCheckResult(task, apikey));
        }

        private void ApiKeyCheckResult(Task<ApiResponse<bool>> x, ApiKey key)
        {
            var ok = x.Result.Response;
            StatusText = ok ? "Successfully connected, the keys have been saved." : "Unable to connect, check you've entered the information correctly.";
            StatusResult = ok;
            if (!ok)
            {
                return;
            }

            if (UserKey != null)
                ProviderData.ApiKeys.Remove(UserKey);

            UserKey = key;
            ProviderData.ApiKeys.Add(key);
            ProviderData.Save(UserContext.Current);
        }

        private string _apiName;
        private string _apiKey;
        private string _apiSecret;
        private string _apiExtra1;
        private string _statusText;
        private bool _statusResult;

        public INetworkProviderPrivate Service { get; private set; }

        public ApiKey UserKey { get; private set; }

        public ApiConfiguration Configuration { get; private set; }

        private ProviderData _providerData;
        public ProviderData ProviderData => _providerData ?? (_providerData = UserContext.Current.Data(Service));

        public string ApiName
        {
            get => this._apiName;
            set => Set(ref _apiName, value, x=> x.Trim());
        }

        public string ApiKey
        {
            get => this._apiKey;
            set => Set(ref _apiKey, value, x => x.Trim());
        }

        public string ApiSecret
        {
            get => this._apiSecret;
            set => Set(ref _apiSecret, value, x => x.Trim());
        }

        public string ApiExtra1
        {
            get => this._apiExtra1;
            set => Set(ref _apiExtra1, value, x => x.Trim());
        }

        public string StatusText
        {
            get => _statusText;
            private set => Set(ref _statusText, value);
        }

        public bool StatusResult
        {
            get => _statusResult;
            private set => Set(ref _statusResult, value);
        }

        public string Error => null;

        public string this[string propertyName]
        {
            get
            {
                switch (propertyName)
                {
                    case nameof(ApiName):
                        return IsNameSane(_apiName);
                    case nameof(ApiKey):
                        return !IsKeySane(_apiKey) ? "Required" : null;
                    case nameof(ApiSecret):
                        return Configuration.HasSecret && !IsKeySane(_apiSecret) ? "Required" : null;
                    case nameof(ApiExtra1):
                        return Configuration.HasExtra && !IsKeySane(_apiExtra1) ? "Required" : null;
                    default:
                        return null;
                }
            }
        }

        private string IsNameSane(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Required";

            name = name.Trim();
            var exists = ProviderData.ApiKeys.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && UserKey?.Id != x.Id);
            return exists ? "Name already exists" : null;
        }

        private bool IsKeySane(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;
            key = key.Trim();
            if (key.Contains(" "))
                return false;
            return true;
        }
    }
}
