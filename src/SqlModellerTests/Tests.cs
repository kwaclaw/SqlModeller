﻿using System;
using System.Data;
using NUnit.Framework;
using SqlModeller.Model;
using SqlModeller.Model.Having;
using SqlModeller.Model.Where;
using SqlModeller.Shorthand;
using System.Collections.Generic;
using SqlModeller.Model.From;

namespace SqlModellerTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void WhereColumnIn() {
            var teamTable = new Table("t", "Team");
            var names = new List<string> { "Jorge", "Benito", "Juan" };
            var select = new SelectQuery()
                .From(teamTable)
                .Select(teamTable, "Name", "T_NAME")
                .WhereColumnIn(teamTable.Alias, "Name", names, DbType.AnsiString);
            var query = new Query();
            query.SelectQuery = select;
            var compiled = query.Compile(false);

            Console.WriteLine(compiled.Sql);
        }

        [Test]
        public void WhereColumnValue() {
            var teamTable = new Table("t", "Team");
            var select = new SelectQuery()
                .From(teamTable)
                .Select(teamTable, "Name", "T_NAME")
                .WhereColumnValue(teamTable.Alias, "Name", Compare.Equal, "Jorge")
                .WhereColumnValue(teamTable.Alias, "Name", Compare.Equal, "@Name", DbType.Object);
            var query = new Query();
            query.SelectQuery = select;
            var compiled = query.Compile(false);

            Console.WriteLine(compiled.Sql);
        }

        [Test]
        public void TestShorthand()
        {
            var countryTable = new Table("c", "Country");
            var teamTable = new Table("t", "Team");
            var playerTable = new Table("p", "Player");

            var cteQuery = new SelectQuery()

            // SELECT
                .SelectRowNumber("_ROW")
                    .SelectOrderByColumn("_SORT")
                    .SelectCount("_COUNT")
                    .SelectGroupKey("_GROUP_KEY")
                    .Select("1 as ONE")
                    .SelectAll()
                    .Select(countryTable, "ID", "C_ID", Aggregate.Min) // should not aggregate, its the group by
                    .Select(teamTable, "ID", "T_ID", Aggregate.Min)
                    .Select(teamTable, "Name", "T_NAME", Aggregate.Avg)
                    .Select(playerTable, "Name", "P_NAME", Aggregate.Sum)
                    .Select(playerTable, "IsMale", "P_ALLMALE", Aggregate.Bit)
                    .Select(playerTable, "IsProfessional", "P_HASPRO", Aggregate.BitMax)
            // FROM
                .From(countryTable)
                    .LeftJoin(teamTable, "CountryID", countryTable, "ID")
                    .Join(playerTable, "TeamID", teamTable, "ID", JoinType.InnerJoin, "AND 1 = 1")
            // WHERE
                .Where(Combine.And)
                    .Where("p.Name IS NOT NULL")
                    .WhereColumnLike(teamTable, "Name", "F.C.", LikeMode.WildcardLeft)
                    .WhereColumnLike(teamTable, "Description", "\"premier league\" -winners", LikeMode.FreeText)
                    .WhereColumnContains(teamTable, "Name", "Man -Cit", ContainsMode.FreeText)
                    .WhereColumnContains(teamTable, "Description", "Winner Runner-up", ContainsMode.AnyWordWildcardRight)
                    .WhereColumnColumn(teamTable, "ID", Compare.NotEqual, countryTable, "ID")
                    .WhereColumnValue(playerTable, "FirstName", Compare.NotEqual, "Peter")
                    .WhereColumnValue(playerTable, "StartDate", Compare.NotEqual, DateTime.Now)
                    .WhereColumnValue(playerTable, "Score", Compare.LessThanOrEqual, 10, isNullValue: 0)
                    .WhereColumnValue(playerTable, "Score", Compare.GreaterThan, "1", DbType.Int32, isNullValue: "0")
                    .WhereCollection(Combine.Or, new WhereFilterCollection()
                        .WhereColumnColumn(teamTable, "Value1", Compare.GreaterThan, countryTable, "Value2")
                        .WhereColumnValue(teamTable, "Value3", Compare.LessThan, 1)
                    )
            // GROUP BY
                .GroupBy(countryTable, "ID")
            // Having
                .Having(Combine.And)
                    .Having("SUM(t.Points) > 4")
                    .HavingColumnValue(Aggregate.Sum, playerTable, "Goals", Compare.GreaterThan, 10)
                    .HavingCollection(Combine.Or, new HavingFilterCollection()
                        .HavingColumnValue(Aggregate.Min, playerTable, "RedCards", Compare.GreaterThan, 1)
                        .HavingColumnValue(Aggregate.Max, playerTable, "RedCards", Compare.LessThan, 5)
                    )
                    .HavingColumnValue(Aggregate.Sum, playerTable, "Score", Compare.LessThan, 100, isNullValue: 0)
            // ORDER BY
                .OrderBy(countryTable, "ID", OrderDir.Asc)
                       .OrderByDesc(playerTable, "ID");

            var cte = new CommonTableExpression()
            {
                Alias = "cte1",
                Query = cteQuery
            };


            var query = new Query();
            query.CommonTableExpressions.Add(cte);
            query.SelectQuery = new SelectQuery()
                .SelectRowNumber("_ROW")
                .SelectTotal("_TOTAL_ROWS", 0)
                .SelectAll()
                .From(cte.Alias, cte.Alias)
                //.OrderBy(cte.Alias, "_ROW")
                .Offset(10)
                .Fetch(5);

            var compiled = query.Compile();

            Console.WriteLine(compiled.ParameterSql);
            Console.WriteLine(compiled.Sql);

        }


        [Test]
        public void TestUnion()
        {

            var firstTable = new Table("t1", "FirstTable");
            var secondTable = new Table("t2", "SecondTable");
            var thirdTable = new Table("t3", "ThirdTable");

            var selectQuery = new SelectQuery().SelectAll().From(firstTable)
            .UnionAll(new SelectQuery().SelectAll().From(secondTable))
            .Union(new SelectQuery().SelectAll().From(thirdTable))
            .OrderBy(string.Empty, "ProductName");

            var query = new Query().Select(selectQuery);

            var compiled = query.Compile();

            Console.WriteLine(compiled.Sql);

            // http://sqlfiddle.com/#!6/f9f24/9
        }

        [Test]
        public void TestUnionCte()
        {

            var firstTable = new Table("t1", "FirstTable");
            var secondTable = new Table("t2", "SecondTable");
            var thirdTable = new Table("t3", "ThirdTable");

            var selectQuery = new SelectQuery().SelectAll().From(firstTable)
            .UnionAll(new SelectQuery().SelectAll().From(secondTable))
            .Union(new SelectQuery().SelectAll().From(thirdTable));

            var cte = new CommonTableExpression { Alias = "Cte", Query = selectQuery };

            var query = new Query();
            query.CommonTableExpressions.Add(cte);
            query.SelectQuery = new SelectQuery()
                .From(cte.Alias, cte.Alias)
                .Select("ProductName")
                .OrderBy(string.Empty, "ProductName");


            var compiled = query.Compile(false);

            Console.WriteLine(compiled.Sql);
        }

        [Test]
        public void TestSelectDistinct()
        {
            var firstTable = new Table("t1", "FirstTable");

            var selectQuery = new SelectQuery()
                .SelectDistinct()
                .SelectAll()
                .From(firstTable);

            var cte = new CommonTableExpression { Alias = "Cte", Query = selectQuery };

            var query = new Query();
            query.CommonTableExpressions.Add(cte);
            query.SelectQuery = new SelectQuery(selectDistinct: true)
                .From(cte.Alias, cte.Alias)
                .Select("ProductName")
                .OrderBy(string.Empty, "ProductName");


            var compiled = query.Compile(false);

            Console.WriteLine(compiled.Sql);

        }

        [Test]
        public void TestGroupedTableJoin()
        {  
            var contact = new Table("ct", "contact");
            var phone = new Table("p", "phone");
            var company = new Table("c", "company");
            var network = new Table("pn", "phonenetwork");

            var selectQuery = new SelectQuery().SelectAll().From(company)
                     .GroupedJoin(contact, "company_id", company, "id",
                         x => x.Join(phone, "contact_id", contact, "id")
                               .Join(network, "id", phone, "network_id")
                     );

            var cte = new CommonTableExpression { Alias = "Cte", Query = selectQuery };

            var query = new Query();
            query.CommonTableExpressions.Add(cte);
            query.SelectQuery = new SelectQuery()
                .From(cte.Alias, cte.Alias)
                .Select("CompanyName")
                .OrderBy(string.Empty, "CompanyName");


            var compiled = query.Compile(false);

            Console.WriteLine(compiled.Sql);

        }
    }
}
