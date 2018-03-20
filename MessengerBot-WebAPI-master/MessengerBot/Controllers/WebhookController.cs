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
using Model.Dao;
using Model;

namespace MessengerBot.Controllers
{
	public class WebhookController : ApiController
	{
        private string TextEnd = ConfigurationManager.AppSettings["TextEnd"];
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
                long id = long.Parse(item.sender.id);
				if (item.message == null && item.postback == null || id == 1059659984213576)
					continue;
				else
                {
                    bool chatting = new ChattingUserDao().IsChatting(id);
                    if(chatting)
                    {
                        await helper.Chatting(item);
                    }
                    else
                    {
                        bool IsExist = new QueueUserDao().IsExist(id);
                        if (IsExist == false)
                        {
                            new QueueUserDao().AddUser(id);
                            await helper.SendBotMessage(helper.GetBotMessage(string.Format("Chào mừng bạn đến với hồ câu cá :3 Bấm kí tự bất kì để thả thính, bấm {0} để kết thúc.", TextEnd), id));
                            return new HttpResponseMessage(HttpStatusCode.OK);
                        }
                        else
                        {
                            //new QueueUserDao().AddOrNotUser(long.Parse(item.sender.id));
                            new QueueUserDao().SetTrueStatus(id);
                            await helper.SendBotMessage(helper.GetBotMessage("Thả câu ...", "Đang tìm cá cho bạn thả thính...", id));
                            List<QueueUser> list = new QueueUserDao().GetAllUser(id);
                            if (list == null || list.Count <= 0)
                                return new HttpResponseMessage(HttpStatusCode.OK);
                            else
                            {
                                var IdOpp = list[new Random().Next(0, list.Count - 1)].ID;
                                var dao = new ChattingUserDao();
                                bool user1 = dao.IsChatting(id);
                                bool user2 = dao.IsChatting(IdOpp);
                                if (user1 == false && user2 == false)
                                {
                                    new QueueUserDao().RemoveCoupleUser(id, IdOpp);
                                    new ChattingUserDao().AddCouple(id, IdOpp);
                                    await helper.SendBotMessage(helper.GetBotMessage("Done!", string.Format("Cá đã cắn câu, hãy tâm sự cùng người lạ đi nào :3 Gõ {0} để kết thúc.", TextEnd), id));
                                    await helper.SendBotMessage(helper.GetBotMessage("Done!", string.Format("Cá đã cắn câu, hãy tâm sự cùng người lạ đi nào :3 Gõ {0} để kết thúc.", TextEnd), IdOpp));
                                }
                            }
                        }
                    }
                }
            }
			return new HttpResponseMessage(HttpStatusCode.OK);
		}
    }
}

