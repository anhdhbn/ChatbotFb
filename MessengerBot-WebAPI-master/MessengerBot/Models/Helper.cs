using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MessengerBot.Models
{
    public class Helper
    {
        private string PageToken = ConfigurationManager.AppSettings["PageToken"];
        private string AppSecret = ConfigurationManager.AppSettings["AppSecret"];
        private string TextEnd = ConfigurationManager.AppSettings["TextEnd"];
        private bool debug = true;
        public Helper()
        {

        }

        public string RemoveNull(object o)
        {
            return JsonConvert.SerializeObject(o,
            new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
        }

        public bool VerifySignature(string signature, string body)
        {
            var hashString = new StringBuilder();
            using (var crypto = new HMACSHA1(Encoding.UTF8.GetBytes(AppSecret)))
            {
                var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(body));
                foreach (var item in hash)
                    hashString.Append(item.ToString("X2"));
            }

            return hashString.ToString().ToLower() == signature.ToLower();
        }

        public async Task SendMessage(JObject json)
        {
            string resStr;

            using (HttpClient client = new HttpClient())
            {
                string jsonstr = json.ToString();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={PageToken}", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
                resStr = res.ToString();
                //if(resStr.Contains("No matching user found"))
                //{

                //}
            }
        }


    }
}