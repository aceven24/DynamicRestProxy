﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using RestSharp;

using Newtonsoft.Json;

namespace DynamicRestProxy
{
    static class RestClientExtensions
    {
        public static async Task<dynamic> ExecuteDynamicTaskAsync(this IRestClient client, IRestRequest request, Method method)
        {
            request.Method = method;

            var response = await client.ExecuteTaskAsync(request);
            if (response == null)
                return null;

            if (response.ErrorException != null)
                throw response.ErrorException;

            return await response.Deserialize();
        }

        public static async Task<dynamic> Deserialize(this IRestResponse response)
        {
            if (!string.IsNullOrEmpty(response.Content))
            {
                return await Task.Factory.StartNew<dynamic>(() => JsonConvert.DeserializeObject<dynamic>(response.Content));
            }

            return await Task.Factory.StartNew<object>(() => { return null; });
        }

        public static void AddDictionary(this IRestRequest request, IDictionary<string, object> args)
        {
            foreach (var kvp in args.Where(kvp => kvp.Value != null))
            {
                request.AddParameter(kvp.Key, kvp.Value.ToString());
            }
        }
    }
}
