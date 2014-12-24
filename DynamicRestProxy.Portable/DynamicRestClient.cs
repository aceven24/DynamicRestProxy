﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Newtonsoft.Json;

namespace DynamicRestProxy.PortableHttpClient
{
    /// <summary>
    /// A rest client that uses dynamic objects for invocation and result values
    /// </summary>
    public class DynamicRestClient : RestProxy
    {
        private readonly Uri _baseUrl;
        private readonly DynamicRestClientDefaults _defaults;
        private readonly Func<HttpRequestMessage, CancellationToken, Task> _configureRequest;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseUrl">The root url for all requests</param>
        /// <param name="defaults">Default values to add to all requests</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(string baseUrl, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(new Uri(baseUrl, UriKind.Absolute), defaults, configure)
        {
        }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="baseUrl">The root url for all requests</param>
        /// <param name="defaults">Default values to add to all requests</param>
        /// <param name="configure">A callback function that will be called just before any request is sent</param>
        public DynamicRestClient(Uri baseUrl, DynamicRestClientDefaults defaults = null, Func<HttpRequestMessage, CancellationToken, Task> configure = null)
            : this(baseUrl, null, "", defaults, configure)
        {
        }

        internal DynamicRestClient(Uri baseUrl, RestProxy parent, string name, DynamicRestClientDefaults defaults, Func<HttpRequestMessage, CancellationToken, Task> configure)
            : base(parent, name)
        {
            _baseUrl = baseUrl;
            _defaults = defaults ?? new DynamicRestClientDefaults();
            _configureRequest = configure;
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.BaseUrl"/>
        /// </summary>
        protected override Uri BaseUrl
        {
            get { return _baseUrl; }
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.CreateProxyNode(RestProxy, string)"/>
        /// </summary>
        protected override RestProxy CreateProxyNode(RestProxy parent, string name)
        {
            return new DynamicRestClient(_baseUrl, parent, name, _defaults, _configureRequest);
        }

        /// <summary>
        /// <see cref="DynamicRestProxy.RestProxy.CreateVerbAsyncTask(string, IEnumerable{object}, IDictionary{string, object})"/>
        /// </summary>
        protected async override Task<T> CreateVerbAsyncTask<T>(string verb, IEnumerable<object> unnamedArgs, IDictionary<string, object> namedArgs, CancellationToken cancelToken, JsonSerializerSettings serializationSettings)
        {
            var builder = new RequestBuilder(this, _defaults);

            // filter any CancellationTokens and JsonSerializerSettings out of the unnamed args as those are not intended as content
            using (var request = builder.CreateRequest(verb, unnamedArgs, namedArgs))
            {
                // give the user code a chance to setup any other request details
                // this is especially useful for setting oauth tokens when they have different lifetimes than the rest client
                if (_configureRequest != null)
                {
                    await _configureRequest(request, cancelToken);
                }

                using (var client = CreateClient())
                using (var response = await client.SendAsync(request, cancelToken))
                {
                    response.EnsureSuccessStatusCode();

                    // forward the JsonSerializationSettings on if passed
                    return await response.Deserialize<T>(serializationSettings);
                }
            }
        }

        private HttpClient CreateClient()
        {
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }

            var client = new HttpClient(handler, true);

            client.BaseAddress = _baseUrl;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/x-json"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/javascript"));

            if (handler.SupportsTransferEncodingChunked())
            {
                client.DefaultRequestHeaders.TransferEncodingChunked = true;
            }

            if (_defaults != null)
            {
                ProductInfoHeaderValue productHeader = null;
                if (!string.IsNullOrEmpty(_defaults.UserAgent) && ProductInfoHeaderValue.TryParse(_defaults.UserAgent, out productHeader))
                {
                    client.DefaultRequestHeaders.UserAgent.Clear();
                    client.DefaultRequestHeaders.UserAgent.Add(productHeader);
                }

                foreach (var kvp in _defaults.DefaultHeaders)
                {
                    client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
                }

                if (!string.IsNullOrEmpty(_defaults.AuthToken) && !string.IsNullOrEmpty(_defaults.AuthScheme))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_defaults.AuthScheme, _defaults.AuthToken);
                }
            }

            return client;
        }
    }
}