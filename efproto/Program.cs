using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data.SQLite;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CavemanTools.Logging;
using efproto.Model;
using SqlFu;
using SqlFu.Builders.CreateTable;
using SqlFu.Providers.Sqlite;
using SqlFu.Providers.SqlServer;

namespace efproto
{
    public static class Const
    {
        public static Guid Recepient1 = Guid.NewGuid();
        public static Guid Recepient2 = Guid.NewGuid();
        public static Guid Message1 = Guid.NewGuid();
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
                var result = db.WithSql(q => q.From<Message>().SelectAll()).GetRows();
                result.ForEach(r =>
                {
                    Console.WriteLine("Message: {0}", r.Subject);
                });

                var r2 = db.WithSql(q => q.FromTemplate(new MessageOwnerTemplate()).SelectAll()).GetRows();
                r2.ForEach(r => {
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
                    Id = new Guid("{00000000-0000-0000-0000-000000000001}"),
                    FirstName = "user 1",
                    LastName = "user 1",
                });
                db.Insert(new Recipient() {
                    Id = new Guid("{00000000-0000-0000-0000-000000000002}"),
                    FirstName = "user 2",
                    LastName = "user 2",
                });
                db.Insert(new Recipient() {
                    Id = new Guid("{00000000-0000-0000-0000-000000000003}"),
                    FirstName = "user 3",
                    LastName = "user 3",
                });

                db.Insert(new Message() {
                    Id = new Guid("{00000000-0000-0000-0001-000000000000}"),
                    CreatorId = new Guid("{00000000-0000-0000-0000-000000000001}"),
                    Subject = "Subject 1",
                    Body = "Body 1",
                });
                db.Insert(new Message() {
                    Id = new Guid("{00000000-0000-0000-0002-000000000000}"),
                    CreatorId = new Guid("{00000000-0000-0000-0000-000000000002}"),
                    Subject = "Subject 2",
                    Body = "Body 2",
                });
                db.Insert(new Message() {
                    Id = new Guid("{00000000-0000-0000-0003-000000000000}"),
                    CreatorId = new Guid("{00000000-0000-0000-0000-000000000003}"),
                    Subject = "Subject 3",
                    Body = "Body 3",
                });
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
