using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MidcoUsageChecker
{
    public class MidcoWebClient
    {
        static string RadiusLogin = "https://api.loginradius.com/identity/v2/auth/login?apiKey=0bec904c-2f03-4e5a-90ab-ee412a4d9787&loginUrl=&emailTemplate=&verificationUrl=https%3A%2F%2Fauth.midco.com%2Fauth.aspx%3Faction%3Dlogin%26return_url%3Dhttps%3A%2F%2Fwww.midco.com%2FMyAccount%2FDashboard%2FDefault.aspx&smsTemplate=";
        static string LoggedInDashboard(string authKey) => string.Format("https://www.midco.com/MyAccount/DashBoard/Default.aspx?token={0}", authKey);
        static string GetBandwidthUsageLandingPage = "https://www.midco.com/MyAccount/BandwidthUsage/default.aspx ";

        static readonly string HourlyButtonKey = "ctl00$ctl00$MainContent$MainContent$hourlyBtn";
        static readonly string DailyButtonKey = "ctl00$ctl00$MainContent$MainContent$dailyBtn";

        private string username;
        private string password;
        private HttpClient client;
        private string viewState;
        private string viewStateGenerator;
        private string eventValidation;

        private void SetupClient()
        {
            var cookies = new CookieContainer();
            var handler = new HttpClientHandler() { CookieContainer = cookies, UseCookies = true, AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate/*, Proxy = new WebProxy("127.0.0.1", 8888)*/ };
            client = new HttpClient(handler);
        }

        private void SetupHeaders()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 Edg/87.0.664.57");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Origin", "https://auth.midco.com");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Referer", "https://auth.midco.com/");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        }

        private void PopulateFormFields(string bwLandingPageString)
        {
            var h = new HtmlDocument();
            h.LoadHtml(bwLandingPageString);

            viewState = h.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATE']")?.Attributes["value"].Value;
            viewStateGenerator = h.DocumentNode.SelectSingleNode("//input[@id='__VIEWSTATEGENERATOR']")?.Attributes["value"].Value;
            eventValidation = h.DocumentNode.SelectSingleNode("//input[@id='__EVENTVALIDATION']")?.Attributes["value"].Value;
        }

        public async Task<bool> LogIn(string username, string password)
        {
            this.username = username;
            this.password = password;
            SetupClient();
            SetupHeaders();

            var obj = new { email = this.username, password = this.password };
            var str = JsonConvert.SerializeObject(obj);
            var sc = new StringContent(str, null, "application/json");

            var postResponse = await client.PostAsync(RadiusLogin, sc);
            var responseBody = await postResponse.Content.ReadAsStringAsync();
            var loginResponse = JsonConvert.DeserializeObject<SimpleLoginRadiusResponse>(responseBody);

            var dashboardResponse = await client.GetAsync(LoggedInDashboard(loginResponse.access_token));

            PopulateFormFields(await dashboardResponse.Content.ReadAsStringAsync());

            return true;
        }


        public Task<DataUsage> LoadHourlyUsage()
        {
            return LoadUsage(HourlyButtonKey);
        }

        public Task<DataUsage> LoadDailyUsage()
        {
            return LoadUsage(DailyButtonKey);
        }

        private async Task<DataUsage> LoadUsage(string requestButtonKey)
        {
            var bandwithPageLandingResponse = await client.GetAsync(GetBandwidthUsageLandingPage);
            var bwLandingPageString = await bandwithPageLandingResponse.Content.ReadAsStringAsync();

            PopulateFormFields(bwLandingPageString);

            var bandwidthRequest = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("__EVENTTARGET", requestButtonKey),
                    new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
                    new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                    new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                    new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
            });

            var bwPostResponse = await client.PostAsync(GetBandwidthUsageLandingPage, bandwidthRequest);

            if (bwPostResponse.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new NoLongerLoggedInException("No longer logged into service. Need to re-login.");
            }

            var bwResponseBody = await bwPostResponse.Content.ReadAsStringAsync();

            if (!bwResponseBody.Contains("window.onload"))
            {
                if (bwLandingPageString.Contains("Welcome to the Midco ID login page"))
                {
                    throw new NoLongerLoggedInException("No longer logged into service. Need to re-login.");
                }
                
                //TODO: Improve detecting and re-logging in if logged out
                //try
                //{
                //    //Became logged out, need to log-in again
                //    await LogIn(username, password);
                //    return await LoadUsage(requestButtonKey);
                //}
                //catch (KeyNotFoundException)
                //{
                //    //Bigger issue here, why again?
                //}
                ////TODO: Figure out what to do if this keeps failing, eventually and indefinite retry

                throw new KeyNotFoundException("Could not find entry line for data usage");
            }

            return GetDataUsagePackage(bwResponseBody);
        }

        private static DataUsage GetDataUsagePackage(string body)
        {
            var lines = body.Split('\n');
            var line = lines.Select(l => l.Trim()).FirstOrDefault(l => l.StartsWith("window.onload = function()"));

            var spot = line.NthIndexOfC('{', 2);
            line = line.Substring(spot);
            var spot2 = line.NthIndexOfC('}', -2);
            line = line.Substring(0, spot2 + 1);
            DataUsage myDeserializedClass = JsonConvert.DeserializeObject<DataUsage>(line);
            return myDeserializedClass;
        }
    }
}
