﻿using System.Threading.Tasks;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    static class RestClientExtensions
    {
        public static async Task<dynamic> ExecuteDynamicGetTaskAsync(this RestClient client, RestRequest request)
        {
            var response = await client.ExecuteGetTaskAsync(request);
            return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(response.Content));
        }

        public static async Task<dynamic> ExecuteDynamicPostTaskAsync(this RestClient client, RestRequest request)
        {
            var response = await client.ExecutePostTaskAsync(request);
            return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(response.Content));
        }
    }
}
