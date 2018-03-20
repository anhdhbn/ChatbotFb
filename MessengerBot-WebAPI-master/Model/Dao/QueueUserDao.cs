using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Dao
{
    public class QueueUserDao
    {
        MessengerBotDbContext db;

        public QueueUserDao()
        {
            db = new MessengerBotDbContext();
        }

        public List<QueueUser> GetAllUser(long id)
        {
            var list = db.QueueUsers.Where(x => x.ID != id && x.Status == true);
            return list.ToList();
        }

        public bool GetStatus(long id)
        {
            return db.QueueUsers.Find(id).Status;
        }

        public void SetTrueStatus(long id)
        {
            var enti = db.QueueUsers.Find(id);
            enti.Status = true;
            db.SaveChanges();
        }

        public void SetFalseStatus(long id)
        {
            var enti = db.QueueUsers.Find(id);
            enti.Status = false;
            db.SaveChanges();
        }

        public void ChangeStatus(long id)
        {
            try
            {
                var ef = db.QueueUsers.Find(id);
                ef.Status = !ef.Status;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }
        }

        public void AddUser(long id)
        {
            try
            {
                var qu = new QueueUser();
                qu.ID = id;
                db.QueueUsers.Add(qu);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }

        }

        public void AddOrNotUser(long id)
        {

            try
            {
                if (db.QueueUsers.Count(x => x.ID == id) == 0)
                {
                    var qu = new QueueUser();
                    qu.ID = id;
                    db.QueueUsers.Add(qu);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }
        }

        public bool IsExist(long id)
        {
            return (db.QueueUsers.Count(x => x.ID == id) > 0);
        }


        public void AddCouple(long id1, long id2)
        {


            try
            {
                var qu1 = new QueueUser();
                qu1.ID = id1;
                var qu2 = new QueueUser();
                qu2.ID = id2;
                db.QueueUsers.Add(qu1);
                db.QueueUsers.Add(qu2);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }
        }

        public void RemoveUser(long id)
        {


            try
            {
                var qu = db.QueueUsers.SingleOrDefault(x => x.ID == id);
                if (qu != null)
                    db.QueueUsers.Remove(qu);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }
        }

        public void RemoveCoupleUser(long id1, long id2)
        {
            try
            {
                var qu1 = db.QueueUsers.SingleOrDefault(x => x.ID == id1);
                if (qu1 != null)
                    db.QueueUsers.Remove(qu1);
                db.SaveChanges();
                var qu2 = db.QueueUsers.SingleOrDefault(x => x.ID == id2);
                if (qu2 != null)
                    db.QueueUsers.Remove(qu2);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string temp = ex.Message;
            }

        }
    }
}
