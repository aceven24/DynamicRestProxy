﻿using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Dynamic;

using RestSharp;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DynamicRestProxy.UnitTests
{
    [TestClass]
    public class GoogleTests
    {
        private static string _token;

        private async Task Authenticate()
        {
            if (!string.IsNullOrEmpty(_token))
                return;

            if (CredentialStore.Exists("google.auth.json"))
            {
                var access = CredentialStore.Retreive("google.auth.json");
                _token = access.access_token;
            }
            else
            {
                var access = await GetNewAccessToken();
                CredentialStore.Store("google.auth.json", access);
                _token = access.access_token;
            }
        }

        private async Task<dynamic> GetNewAccessToken()
        {
            dynamic key = CredentialStore.JsonKey("google").installed;

            var client = new RestClient("https://accounts.google.com");
            dynamic proxy = new RestProxy(client);
            var response = await proxy.o.oauth2.device.code.post(client_id: key.client_id, scope: "email profile https://www.googleapis.com/auth/calendar");
            Assert.IsNotNull(response);

            Debug.WriteLine((string)response.user_code);

            // use clip.exe to put the user code on the clipboard
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = string.Format("/c echo {0} | clip", response.user_code);
            p.Start();

            // this requires user permission - open a broswer - enter the user_code which is now in the clipboard
            Process.Start((string)response.verification_url);

            int expiration = response.expires_in;
            int interval = response.interval;
            int time = interval;

            dynamic tokenResonse = null;
            // we are using the device flow so enter the code in the browser - poll google for success
            while (time < expiration)
            {
                Thread.Sleep(interval * 1000);
                tokenResonse = await proxy.o.oauth2.token.post(client_id: key.client_id, client_secret: key.client_secret, code: response.device_code, grant_type: "http://oauth.net/grant_type/device/1.0");
                if (tokenResonse.access_token != null)
                    break;
                time += interval;
            }

            Assert.IsNotNull(tokenResonse);
            return tokenResonse;
        }

        [TestMethod]
        [TestCategory("online")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetUserProfile()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var api = new RestClient("https://www.googleapis.com");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic apiProxy = new RestProxy(api);
            var profile = await apiProxy.oauth2.v1.userinfo.get();

            Assert.IsNotNull(profile);
            Assert.AreEqual((string)profile.family_name, "Kackman");
        }

        [TestMethod]
        [TestCategory("online")]
        //  [Ignore] // - this test requires user interaction
        public async Task GetCalendarList()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var api = new RestClient("https://www.googleapis.com/calendar/v3");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic apiProxy = new RestProxy(api);
            var list = await apiProxy.users.me.calendarList.get();

            Assert.IsNotNull(list);
            Assert.AreEqual((string)list.kind, "calendar#calendarList");
        }

        [TestMethod]
        [TestCategory("ordered")]
        [TestCategory("online")]
        //  [Ignore] // - this test requires user interaction
        public async Task CreateCalendar()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var api = new RestClient("https://www.googleapis.com/calendar/v3");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic apiProxy = new RestProxy(api);

            dynamic calendar = new ExpandoObject();
            calendar.summary = "unit_testing";

            var list = await apiProxy.calendars.post(calendar);

            Assert.IsNotNull(list);
            Assert.AreEqual((string)list.summary, "unit_testing");
        }

        [TestMethod]
        [TestCategory("ordered")]
        [TestCategory("online")]
        //  [Ignore] // - this test requires user interaction
        public async Task UpdateCalendar()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var api = new RestClient("https://www.googleapis.com/calendar/v3");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic apiProxy = new RestProxy(api);
            var list = await apiProxy.users.me.calendarList.get();
            Assert.IsNotNull(list);

            string id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();
            Assert.IsFalse(string.IsNullOrEmpty(id));

            var guid = Guid.NewGuid().ToString();
            dynamic calendar = new ExpandoObject();
            calendar.summary = "unit_testing";
            calendar.description = guid;

            var result = await apiProxy.calendars.segment(id).put(calendar);
            Assert.IsNotNull(result);

            list = await apiProxy.users.me.calendarList.get();
            Assert.IsNotNull(list);
            string description = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.description).FirstOrDefault();

            Assert.AreEqual(guid, description);
        }

        [TestMethod]
        [TestCategory("ordered")]
        [TestCategory("online")]
        //  [Ignore] // - this test requires user interaction
        public async Task DeleteCalendar()
        {
            await Authenticate();
            Assert.IsNotNull(_token);

            var api = new RestClient("https://www.googleapis.com/calendar/v3");
            api.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(_token);
            dynamic apiProxy = new RestProxy(api);
            var list = await apiProxy.users.me.calendarList.get();
            Assert.IsNotNull(list);

            string id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();
            Assert.IsFalse(string.IsNullOrEmpty(id));

            var result = await apiProxy.calendars.segment(id).delete();
            Assert.IsNull(result);

            list = await apiProxy.users.me.calendarList.get();
            Assert.IsNotNull(list);
            id = ((IEnumerable<dynamic>)(list.items)).Where(cal => cal.summary == "unit_testing").Select(cal => (string)cal.id).FirstOrDefault();

            Assert.IsTrue(string.IsNullOrEmpty(id));
        }
    }
}
