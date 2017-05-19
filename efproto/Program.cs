using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CavemanTools.Logging;
using efproto.Model;
using efproto.Query;
using SqlFu;
using SqlFu.Builders.CreateTable;
using SqlFu.Providers.Sqlite;
using SqlFu.Providers.SqlServer;

namespace efproto
{
    public static class Const
    {
        public static Guid Recepient1 = new Guid("{00000000-0000-0000-0000-000000000001}");
        public static Guid Recepient2 = new Guid("{00000000-0000-0000-0000-000000000002}");
        public static Guid Recepient3 = new Guid("{00000000-0000-0000-0000-000000000003}");
        public static Guid Message1 = new Guid("{00000000-0000-0000-0001-000000000000}");
        public static Guid Message2 = new Guid("{00000000-0000-0000-0002-000000000000}");
        public static Guid Message3 = new Guid("{00000000-0000-0000-0003-000000000000}");
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            ConfigureSqlFu();
            EnsureDb();
            GenerateRecords();

            QueryDb();

        }

        static void QueryDb()
        {
            var factory = SqlFuManager.GetDbFactory();
            using(var db = factory.Create())
            {
                var result = db.QueryAs(q => q.From<Message>().SelectAll());
                result.ForEach(r =>
                {
                    Console.WriteLine("Message: {0}", r.Subject);
                });

                var r2 = db.QueryAs(q => q.FromTemplate(new MessageOwnerTemplate()).SelectAll());
                r2.ForEach(r => {
                    Console.WriteLine("Message: {0}, Owner: {1}", r.Subject, r.FirstName);
                });

                var r3 = db.QueryAs(q => q.FromTemplate(new MessageOwnerTemplate())
                    .Where(mo => mo.FirstName.Contains("2"))
                    .SelectAll());
                r3.ForEach(r => {
                    Console.WriteLine("Message: {0}, Owner: {1}", r.Subject, r.FirstName);
                });
            }
        }
        static void GenerateRecords()
        {
            var factory = SqlFuManager.GetDbFactory();
            using(var db = factory.Create())
            {
                db.Insert(new Recipient() {
                    Id = Const.Recepient1,
                    FirstName = "user 1",
                    LastName = "user 1",
                });
                db.Insert(new Recipient() {
                    Id = Const.Recepient2,
                    FirstName = "user 2",
                    LastName = "user 2",
                });
                db.Insert(new Recipient() {
                    Id = Const.Recepient3,
                    FirstName = "user 3",
                    LastName = "user 3",
                });

                db.Insert(new Message() {
                    Id = Const.Message1,
                    CreatorId = Const.Recepient1,
                    Subject = "Subject 1",
                    Body = "Body 1",
                });
                db.Insert(new Message() {
                    Id = Const.Message2,
                    CreatorId = Const.Recepient2,
                    Subject = "Subject 2",
                    Body = "Body 2",
                });

                var tokenSource = new CancellationTokenSource(100);
                var id = db.InsertAsync( new Message() {
                    Id = Const.Message3,
                    CreatorId = Const.Recepient3,
                    Subject = "Subject 3 from 1",
                    Body = "Body 3 from 1",
                }, tokenSource.Token).Result;
            }
        }

        static void ConfigureSqlFu()
        {
            LogManager.OutputToTrace();
            var connString = @"Data Source=d:\efproto.sqlite3; Version=3; FailIfMissing=True; Foreign Keys=True; ";
            SqlFuManager.Configure(c =>
            {
                c.AddProfile(new SqliteProvider(() =>
                {
                    return new SQLiteConnection(connString);
                }), connString);

                c.OnException = (cmd, ex) => Console.WriteLine(cmd.FormatCommand(), ex);
            });
        }

        static void EnsureDb()
        {
            var factory = SqlFuManager.GetDbFactory();
            using(var db = factory.Create())
            {
                if(true || !db.TableExists<Message>())
                {
                    db.CreateTableFrom<Message>(cf => {
                        cf.DropIfExists()
                          .PrimaryKey(pk => pk.OnColumns(d => d.Id))
                        ;
                    });
                }

                if(true || !db.TableExists<Message>())
                {
                    db.CreateTableFrom<Recipient>(cf => {
                        cf.DropIfExists()
                          .PrimaryKey(pk => pk.OnColumns(d => d.Id))
                        ;
                    });
                }
            }
        }
    }
}
