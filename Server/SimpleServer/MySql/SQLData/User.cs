using SqlSugar;
using System;
namespace MySql.SQLData
{
    [SugarTable("user")]
    public class User
    {
        [SugarColumn(IsPrimaryKey = true,IsIdentity = true)]
        public int Id { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public DateTime LoginDate { get; set; }
        
        public string LoginType { get; set; }
        
        public string Token { get; set; }
    }
}
