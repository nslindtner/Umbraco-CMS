﻿using System.Collections.Generic;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.NPocoTests
{
    [TestFixture]
    public class NPocoExpressionsTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void WhereInValueFieldTest()
        {
            var sql = new Sql<SqlContext>(SqlContext)
                .Select("*")
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 });
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[id] IN (@0,@1,@2))", sql.SQL);
        }

        [Test]
        public void WhereInObjectFieldTest()
        {
            // this test used to fail because x => x.Text was evaluated as a lambda
            // and returned "[umbracoNode].[text] = @0"... had to fix WhereIn.

            var sql = new Sql<SqlContext>(SqlContext)
                .Select("*")
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" });
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] IN (@0,@1,@2))", sql.SQL);
        }

        [Test]
        public void SelectTests()
        {
            // select the whole DTO
            var sql = Sql()
                .Select<Dto1>()
                .From<Dto1>();
            Assert.AreEqual("SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value] FROM [dto1]", sql.SQL.NoCrLf());

            // select only 1 field
            sql = Sql()
                .Select<Dto1>(x => x.Id)
                .From<Dto1>();
            Assert.AreEqual("SELECT [dto1].[id] AS [Id] FROM [dto1]", sql.SQL.NoCrLf());

            // select 2 fields
            sql = Sql()
                .Select<Dto1>(x => x.Id, x => x.Name)
                .From<Dto1>();
            Assert.AreEqual("SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name] FROM [dto1]", sql.SQL.NoCrLf());

            // select the whole DTO and a referenced DTO
            sql = Sql()
                .Select<Dto1>(r => r.Select(x => x.Dto2))
                .From<Dto1>()
                .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
            Assert.AreEqual(@"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2__Id], [dto2].[dto1id] AS [Dto2__Dto1Id], [dto2].[name] AS [Dto2__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]".NoCrLf(), sql.SQL.NoCrLf(), sql.SQL);

            // select the whole DTO and nested referenced DTOs
            sql = Sql()
                .Select<Dto1>(r => r.Select(x => x.Dto2, r1 => r1.Select(x => x.Dto3)))
                .From<Dto1>()
                .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id)
                .InnerJoin<Dto3>().On<Dto2, Dto3>(left => left.Id, right => right.Dto2Id);
            Assert.AreEqual(@"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2__Id], [dto2].[dto1id] AS [Dto2__Dto1Id], [dto2].[name] AS [Dto2__Name]
, [dto3].[id] AS [Dto2__Dto3__Id], [dto3].[dto2id] AS [Dto2__Dto3__Dto2Id], [dto3].[name] AS [Dto2__Dto3__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]
INNER JOIN [dto3] ON [dto2].[id] = [dto3].[dto2id]".NoCrLf(), sql.SQL.NoCrLf());

            // select the whole DTO and referenced DTOs
            sql = Sql()
                .Select<Dto1>(r => r.Select(x => x.Dto2s))
                .From<Dto1>()
                .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
            Assert.AreEqual(@"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2s__Id], [dto2].[dto1id] AS [Dto2s__Dto1Id], [dto2].[name] AS [Dto2s__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]".NoCrLf(), sql.SQL.NoCrLf());
        }

        [TableName("dto1")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Dto1
        {
            [Column("id")]
            public int Id { get; set; }
            [Column("name")]
            public string Name { get; set; }
            [Column("value")]
            public int Value { get; set; }
            [Reference]
            public Dto2 Dto2 { get; set; }
            [Reference]
            public List<Dto2> Dto2s { get; set; }
        }

        [TableName("dto2")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Dto2
        {
            [Column("id")]
            public int Id { get; set; }
            [Column("dto1id")]
            public int Dto1Id { get; set; }
            [Column("name")]
            public string Name { get; set; }
            [Reference]
            public Dto3 Dto3 { get; set; }
        }

        [TableName("dto3")]
        [PrimaryKey("id", AutoIncrement = false)]
        [ExplicitColumns]
        public class Dto3
        {
            [Column("id")]
            public int Id { get; set; }
            [Column("dto2id")]
            public int Dto2Id { get; set; }
            [Column("name")]
            public string Name { get; set; }
        }
    }
}
