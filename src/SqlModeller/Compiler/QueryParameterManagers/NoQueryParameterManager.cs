using System.Data;
using SqlModeller.Interfaces;
using SqlModeller.Compiler.SqlServer;

namespace SqlModeller.Compiler.QueryParameterManagers
{
    public class NoQueryParameterManager : IQueryParameterManager
    {
        
        public string Parameterize(string value, DbType type, string alias = null)
        {
            return ToStringHelper.ValueString(value, type);
        }
    }
}