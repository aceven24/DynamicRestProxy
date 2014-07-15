﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using System.Net;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.PortableHttpClient.UnitTests
{
    [TestClass]
    public class FlickrTests
    {
        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        public async Task FindUserByName()
        {
            var key = CredentialStore.RetrieveObject("flickr.key.json");
            var defaults = new DynamicRestClientDefaults();
            defaults.DefaultParameters.Add("format", "json");
            defaults.DefaultParameters.Add("api_key", key.Key);
            defaults.DefaultParameters.Add("nojsoncallback", "1");

            dynamic client = new DynamicRestClient("https://api.flickr.com/services/rest/", defaults);

            dynamic result = await client.get(method: "flickr.people.findByUsername", username: "dkackman");
            Assert.IsNotNull(result);
            Assert.AreEqual("9039518@N03", (string)result.user.id);
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [Ignore]
        public async Task UploadPhoto()
        {
            var key = CredentialStore.RetrieveObject("flickr.key.json");
            var defaults = new DynamicRestClientDefaults();
            defaults.DefaultParameters.Add("format", "json");
            defaults.DefaultParameters.Add("api_key", key.Key);
            defaults.DefaultParameters.Add("nojsoncallback", "1");

            dynamic client = new DynamicRestClient("https://up.flickr.com/services/", defaults);

            dynamic result = await client.upload.post(photo: File.OpenRead(@"D:\temp\test.png"));

            Assert.IsNotNull(result);

        }
    }
}
