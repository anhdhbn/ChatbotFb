namespace Model
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class MessengerBotDbContext : DbContext
    {
        public MessengerBotDbContext()
            : base("name=MessengerBotDbContext")
        {
        }

        public virtual DbSet<ChattingUser> ChattingUsers { get; set; }
        public virtual DbSet<QueueUser> QueueUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
