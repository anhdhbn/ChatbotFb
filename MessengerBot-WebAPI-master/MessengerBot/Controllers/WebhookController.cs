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
using System.Configuration;

namespace MessengerBot.Controllers
{
	public class WebhookController : ApiController
	{
        

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
            Helper helper = new Helper();
            var body = await Request.Content.ReadAsStringAsync();
            var signature = Request.Headers.GetValues("X-Hub-Signature").FirstOrDefault().Replace("sha1=", "");
            if (!helper.VerifySignature(signature, body))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            var value = JsonConvert.DeserializeObject<BotRequest>(body);
			if (value.@object != "page")
            {
                return new HttpResponseMessage(HttpStatusCode.OK);
            }

			foreach (var item in value.entry[0].messaging)
			{
				if (item.message == null && item.postback == null)
					continue;
				else
                {                    
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

                        var result = helper.RemoveNull(json);
                        var jsonObj = JObject.Parse(result);
                        await helper.SendMessage(jsonObj);
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

                            var result = helper.RemoveNull(json);
                            var jsonObj = JObject.Parse(result);
                            await helper.SendMessage(jsonObj);
                        }
                    }
                }
            }

			return new HttpResponseMessage(HttpStatusCode.OK);
		}
    }
}

