using SqlSugar;
using System;
using System.Linq;
namespace MySql
{
    public class MySqlMgr : Singleton<MySqlMgr>
    {
#if DEBUG
        private const string CONNECTING_STR = "Server=127.0.0.1;uid=root;pwd=VasA4z,rXz*a;database=ocean";
        // private const string CONNECTING_STR = "Server=localhost;";
#else
        private const string CONNECTING_STR = "Server=localhost;uid=root;pwd=VasA4z,rXz*a;database=ocean";
#endif

        public SqlSugarClient SqlSugarDb = null;
        public void Init()
        {
            SqlSugarDb = new SqlSugarClient(
                new ConnectionConfig()
                {
                    ConnectionString = CONNECTING_STR,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    // InitKeyType = InitKeyType.Attribute
                }
            );
#if DEBUG
            //调试SQL事件，可以删掉
            SqlSugarDb.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" + SqlSugarDb.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
#endif
        }
    }
}
