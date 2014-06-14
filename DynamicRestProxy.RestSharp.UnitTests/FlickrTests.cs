﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using System.Diagnostics;

using RestSharp;

using DynamicRestProxy.RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace DynamicRestProxy.RestSharp.UnitTests
{
    [TestClass]
    public class FlickrTests
    {
        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        public async Task FindUserByName()
        {
            var key = CredentialStore.JsonKey("flickr");

            var client = new RestClient("https://api.flickr.com/services/rest/");
            client.AddDefaultParameter("format", "json");
            client.AddDefaultParameter("api_key", (string)key.Key);
            client.AddDefaultParameter("nojsoncallback", "1");

            dynamic proxy = new RestSharpProxy(client);

            dynamic result = await proxy.get(method: "flickr.people.findByUsername", username: "dkackman");
            Assert.IsNotNull(result);
            Assert.AreEqual("9039518@N03", (string)result.user.id);
        }

        [TestMethod]
        [TestCategory("RestSharp")]
        [TestCategory("integration")]
        [Ignore]
        public async Task UploadPhoto()
        {
            var key = CredentialStore.JsonKey("flickr");

            var client = new RestClient("https://up.flickr.com/services/");
            client.AddDefaultParameter("format", "json");
            client.AddDefaultParameter("api_key", (string)key.Key);
            client.AddDefaultParameter("nojsoncallback", "1");

            var file = new FileInfo(@"D:\temp\test.png");

            dynamic proxy = new RestSharpProxy(client);

            dynamic result = await proxy.upload.post(photo: file);

            Assert.IsNotNull(result);
            
        }
    }
}
