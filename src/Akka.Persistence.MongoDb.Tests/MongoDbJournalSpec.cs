﻿using System;
using System.Configuration;
using Akka.Persistence.TestKit.Journal;
using Mongo2Go;
using MongoDB.Driver;

namespace Akka.Persistence.MongoDb.Tests
{
    public class MongoDbJournalSpec : JournalSpec
    {
        private static readonly MongoDbRunner Runner = MongoDbRunner.Start(ConfigurationManager.AppSettings[0]);
        protected override bool SupportsRejectingNonSerializableObjects { get { return false; } }

        private static readonly string SpecConfig = @"
        akka.persistence {
            publish-plugin-commands = on
            journal {
                plugin = ""akka.persistence.journal.mongodb""
                mongodb {
                    class = ""Akka.Persistence.MongoDb.Journal.MongoDbJournal, Akka.Persistence.MongoDb""
                    connection-string = ""<ConnectionString>""
                    collection = ""EventJournal""
                }
            }
            snapshot-store {
                plugin = ""akka.persistence.snapshot-store.mongodb""
                mongodb {
                    class = ""Akka.Persistence.MongoDb.Snapshot.MongoDbSnapshotStore, Akka.Persistence.MongoDb""
                    connection-string = ""<ConnectionString>""
                    collection = ""SnapshotStore""
                }
            }
        }";

        public MongoDbJournalSpec() : base(CreateSpecConfig(), "MongoDbJournalSpec")
        {
            AppDomain.CurrentDomain.DomainUnload += (_, __) =>
            {
                try
                {
                    Runner.Dispose();
                }
                catch { }
            };

            Initialize();
        }

        private static string CreateSpecConfig()
        {
            return SpecConfig.Replace("<ConnectionString>", Runner.ConnectionString + "akkanet");
        }

        protected override void Dispose(bool disposing)
        {
            new MongoClient(Runner.ConnectionString)
                .GetDatabase("akkanet")
                .DropCollectionAsync("EventJournal").Wait();

            base.Dispose(disposing);
        }
    }
}
