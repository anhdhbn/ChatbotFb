using Model.Dao;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using static MessengerBot.Models.FbBotDataClasses;

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

        public JObject GetBotMessage(string title, string content, long recipientId)
        {
            ArrayList ar = new ArrayList();
            ar.Add(new
            {
                title = title,
                subtitle = content
            });
            return JObject.FromObject(new
            {
                recipient = new
                {
                    id = recipientId.ToString()
                },
                message = new
                {
                    attachment = new
                    {
                        type = "template",
                        payload = new
                        {
                            template_type = "generic",
                            elements = ar
                        }
                    }
                }
            });
        }

        public JObject GetBotMessage(string content, long recipientId)
        {
            return JObject.FromObject(new
            {
                recipient = new
                {
                    id = recipientId.ToString()
                },
                message = new
                {
                    text = content
                }
            });
        }

        public async Task SendBotMessage(JObject json)
        {
            string pageToken = ConfigurationManager.AppSettings["pageToken"];
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage res = await client.PostAsync($"https://graph.facebook.com/v2.6/me/messages?access_token={pageToken}", new StringContent(json.ToString(), Encoding.UTF8, "application/json"));
            }
        }

        public async Task EndChat(long id, long IdOpponent)
        {
            await SendBotMessage(GetBotMessage("Bạn đã ngưng thả thính", "Gõ kí tự bất kì để thả thính", id));
            await SendBotMessage(GetBotMessage("Đối phương đã ngưng thả thính", "Gõ kí tự bất kì để thả thính", IdOpponent));
            new ChattingUserDao().RemoveCouple(id, IdOpponent);
            new QueueUserDao().AddCouple(id, IdOpponent);
            //new QueueUserDao().SetFalseStatus(id);
            //new QueueUserDao().SetFalseStatus(IdOpponent);
        }

        public async Task Chatting(BotMessageReceivedRequest item)
        {
            long IdOpponent = new ChattingUserDao().GetOpponentID(long.Parse(item.sender.id));
            if (item.message.text == ConfigurationManager.AppSettings["textEnd"])
            {
                await EndChat(long.Parse(item.sender.id), IdOpponent);
                return;
            }
            if (item.message.attachments == null)
            {
                var tempMess = item.message;
                tempMess.seq = null;
                tempMess.mid = null;
                var json = new
                {
                    message = tempMess,
                    recipient = new { id = IdOpponent }
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
                        recipient = new { id = IdOpponent }
                    };

                    var result = RemoveNull(json);
                    var jsonObj = JObject.Parse(result);
                    await SendMessage(jsonObj);
                }
            }
        }

    }
}