using SqlSugar;
using System;
using System.Linq;
namespace MySql
{
    public class MySqlMgr : Singleton<MySqlMgr>
    {
#if DEBUG
        private const string connectingStr = "Server=localhost;uid=root;pwd=VasA4z,rXz*a;database=ocean";
#else
        private const string connectingStr = "Server=localhost;uid=root;pwd=VasA4z,rXz*a;database=ocean";
#endif

        public SqlSugarClient SqlSugarDB = null;
        public void Init()
        {
            SqlSugarDB = new SqlSugarClient(
                new ConnectionConfig()
                {
                    ConnectionString = connectingStr,
                    DbType = DbType.MySql,
                    IsAutoCloseConnection = true,
                    InitKeyType = InitKeyType.Attribute
                }
            );
#if DEBUG
            //调试SQL事件，可以删掉
            SqlSugarDB.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" + SqlSugarDB.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
#endif
        }
    }
}
