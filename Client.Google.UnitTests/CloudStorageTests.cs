﻿using System.IO;
using System.Dynamic;
using System.Threading.Tasks;

using DynamicRestProxy.PortableHttpClient;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTestHelpers;

namespace Client.Google.UnitTests
{
    [TestClass]
    public class CloudStorageTests
    {
        private static string _token = null;

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadObject()
        {
            var auth = new GoogleOAuth2();
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = _token
            };
            dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);

            using (var stream = new StreamInfo(File.OpenRead(@"D:\temp\test.png"), "image/png"))
            {
                dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(stream, name: new PostUrlParam("test_object"), uploadType: new PostUrlParam("media"));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task MultiPartUploadObject()
        {
            var auth = new GoogleOAuth2();
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = _token
            };

            dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);

            using (var stream = new StreamInfo(File.OpenRead(@"D:\temp\test2.png"), "image/png"))
            {
                dynamic metaData = new ExpandoObject();
                metaData.name = "test2";
                dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(metaData, stream, uploadType: new PostUrlParam("multipart"));
                Assert.IsNotNull(result);
            }
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadString()
        {
            var auth = new GoogleOAuth2();
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = _token
            };

            dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);

            dynamic result = await google.upload.storage.v1.b.unit_tests.o.post("text", name: new PostUrlParam("string_object"), uploadType: new PostUrlParam("media"));
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [TestCategory("portable-client")]
        [TestCategory("integration")]
        [TestCategory("google")]
        public async Task UploadInt()
        {
            var auth = new GoogleOAuth2();
            _token = await auth.Authenticate(_token);
            Assert.IsNotNull(_token, "auth failed");

            var defaults = new DynamicRestClientDefaults()
            {
                AuthScheme = "OAuth",
                AuthToken = _token
            };

            dynamic google = new DynamicRestClient("https://www.googleapis.com/", defaults);

            dynamic result = await google.upload.storage.v1.b.unit_tests.o.post(42, name: new PostUrlParam("int_object"), uploadType: new PostUrlParam("media"));
            Assert.IsNotNull(result);
        }
    }
}
