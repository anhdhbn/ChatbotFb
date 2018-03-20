using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using MessengerBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static MessengerBot.Models.FbBotDataClasses;
using System.Web.Script.Serialization;

namespace MessengerBot.Controllers
{
	public class WebhookController : ApiController
	{
		string pageToken = "EAADA0WnUOS4BAPsoOPbTAGtG3nLYHGXvEuiSaVyYa0l3RgXuOSnJTECv9AmcgVYTWZBaJ0ZBCLH1hZAYv5NyVHCCWuLpmzGjKOIhwWbQZCPDKl8WoBZC8nFQn8e3Y689d1bzQmgWyiuDJWxEkMaor4qcAyFdakT9eSGH5ttIVK7AnrmqQjajG";
		string appSecret = "88d3dcdbc6e31b1b4bcbf245b6dd6bbf";

		public HttpResponseMessage Get()
		{
			var querystrings = Request.GetQueryNameValuePairs().ToDictionary(x => x.Key, x => x.Value);
			if (querystrings["hub.verify_token"] == "yp1chatbot")
			{
				return new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent(querystrings["hub.challenge"], Encoding.UTF8, "text/plain")
				};
			}
			return new HttpResponseMessage(HttpStatusCode.Unauthorized);
		}

		[HttpPost]
		public async Task<HttpResponseMessage> Post()
		{
            var body = await Request.Content.ReadAsStringAsync();
            //var signature = Request.Headers.GetValues("X-Hub-Signature").FirstOrDefault().Replace("sha1=", "");			
            //if (!VerifySignature(signature, body))
            //	return new HttpResponseMessage(HttpStatusCode.BadRequest);
            Console.WriteLine(body);
			var value = JsonConvert.DeserializeObject<BotRequest>(body);
			if (value.@object != "page")
				return new HttpResponseMessage(HttpStatusCode.OK);

			foreach (var item in value.entry[0].messaging)
			{
				if (item.message == null && item.postback == null)
					continue;
				else
                {
                    // check database user xem có chưa 
                    // check database queue xem có chưa
                    // có thì thêm
                    // bắt kí tự bất kì
                    string mess = "Gõ kí tự bất kì để thả thính ^^";
                    string title = "Đối phương đã ngưng thả thính!";

                    //BotMessageReceivedRequest temp = new BotMessageReceivedRequest();
                    ////temp.sender.id = item.sender.id;
                    //temp.recipient.id = item.sender.id;
                    //temp.message.text = item.message.text;

                    //temp.message.quick_reply.content_type = mess;
                    //temp.message.quick_reply.title = title;

                    //string jsonString = JsonConvert.SerializeObject(
                    //temp,
                    //Formatting.None,
                    //new JsonSerializerSettings()
                    //{
                    //    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    //});

                    //await SendMessage(GetMessageTemplate(item.message.text, item.sender.id));

                    
                    
                    if(item.message.attachments == null)
                    {
                        var tempMess = item.message;
                        tempMess.seq = null;
                        tempMess.mid = null;
                        var json = new
                        {
                            message = tempMess,
                            recipient = new { id = item.sender.id }
                        };

                        var result = RemoveNull(json);
                        var jsonObj = JObject.Parse(result);
                        await SendMessage(jsonObj);
                    }
                    else
                    {
                        foreach (var att in item.message.attachments)
                        {
                            var tempMess = item.message;
                            tempMess.seq = null;
                            tempMess.mid = null;
                            tempMess.attachment = att;
                            tempMess.attachments = null;
                            if (tempMess.attachment.type == "fallback")
                            {
                                tempMess.attachment = null;
                            }

                            var json = new
                            {
                                message = tempMess,
                                recipient = new { id = item.sender.id }
                            };

                            var result = RemoveNull(json);
                            var jsonObj = JObject.Parse(result);
                            await SendMessage(jsonObj);
                        }
                    }

                   

                    //, quick_reply = new { content_type = mess, title = title }
                }
            }

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

        private string RemoveNull(object o)
        {
            return JsonConvert.SerializeObject(o,
            new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });
        }

		private bool VerifySignature(string signature, string body)
		{
			var hashString = new StringBuilder();
			using (var crypto = new HMACSHA1(Encoding.UTF8.GetBytes(appSecret)))
			{
				var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(body));
				foreach (var item in hash)
					hashString.Append(item.ToString("X2"));
			}

			return hashString.ToString().ToLower() == signature.ToLower();
		}

		/// <summary>
		/// get text message template
		/// </summary>
		/// <param name="text">text</param>
		/// <param name="sender">sender id</param>
		/// <returns>json</returns>
		private JObject GetMessageTemplate(string text, string sender)
		{
			return JObject.FromObject(new
			{
				recipient = new { id = sender },
				message = new { text = text }

                //sender_action = new { sender_action = "typing_on" }
            });
		}

		/// <summary>
		/// send message
		/// </summary>
		/// <param name="json">json</param>
		private async Task SendMessage(JObject json)
		{
			using (HttpClient client = new HttpClient())
			{
                string jsonstr = json.ToString();
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				HttpResponseMessage res = await client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={pageToken}", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
                string resStr = res.ToString();
                string temp = "";
            }
		}

        private async Task SendMessage(string jsonStr)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={pageToken}", new StringContent(jsonStr, Encoding.UTF8, "application/json"));
                string resStr = res.ToString();
                string temp = "";
            }
        }
    }
}

