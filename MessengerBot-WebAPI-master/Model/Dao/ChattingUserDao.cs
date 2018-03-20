using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class ChattingUserDao
    {
        MessengerBotDbContext db;
        public ChattingUserDao()
        {
            db = new MessengerBotDbContext();
        }

        public long GetOpponentID(long id)
        {
            return db.ChattingUsers.Find(id).OpponentID;
        }

        public void AddCouple(long id1, long id2)
        {
            ChattingUser cu1 = new ChattingUser();
            cu1.ID = id1;
            cu1.OpponentID = id2;

            ChattingUser cu2 = new ChattingUser();
            cu2.ID = id2;
            cu2.OpponentID = id1;

            try
            {
                if (!IsChatting(id1))
                {
                    db.ChattingUsers.Add(cu1);
                }
                if (!IsChatting(id2))
                {
                    db.ChattingUsers.Add(cu2);
                }
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }


        }

        public void RemoveCouple(long id1, long id2)
        {
            try
            {
                var cu1 = db.ChattingUsers.Find(id1);
                var cu2 = db.ChattingUsers.Find(id2);
                db.ChattingUsers.Remove(cu1);
                db.ChattingUsers.Remove(cu2);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }

        }

        public bool IsChatting(long id)
        {
            int countL = 0;
            try
            {
                countL = db.ChattingUsers.Count(x => x.ID == id);
            }
            catch (Exception ex)
            {
                string mess = ex.Message;
            }

            return (countL > 0);
        }

    }
}
