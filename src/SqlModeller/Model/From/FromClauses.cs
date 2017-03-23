using System.Collections.Generic;

namespace SqlModeller.Model.From
{
    public class FromClause
    {
    }

    public class JoinClause: FromClause
    {
        public JoinType JoinType { get; set; }
        public FromClause JoinFrom { get; set; }
        public JoinColumn JoinColumn { get; set; }
        public FromClause ForeignFrom { get; set; }
        public JoinColumn ForeignColumn { get; set; }
        public string Extra { get; set; }
    }

    public class TableClause: FromClause
    {
        public Table FromTable { get; set; }
    }
}