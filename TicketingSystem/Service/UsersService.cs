using Dapper;
using TicketingSystem.Models;
using TicketingSystem.Utils;

namespace TicketingSystem.Repo
{
    public class UsersService
    {
        private readonly DbHelper dbHelper;

        public UsersService(DbHelper dbHelper)
        {
            this.dbHelper = dbHelper;
        }
        internal bool ValidUser(string userId, string userPwd)
        {
            var sql = "select 1 from Users where Id = @userId and pwd=@userPwd";
            return dbHelper.ConnDb(conn =>
            {
                return conn.Query(sql, new { userId, userPwd }).Count() > 0;
            });
        }

        internal string CreateUser(string userId, string userName, string pwd, string email)
        {
            var sql = @"select 1 from Users where Id = @userId";
            bool userExists = dbHelper.ConnDb(conn =>
            {
                return conn.Query(sql, new { userId }).Count() > 0;
            });
            if (userExists)
            {
                return "帳號已存在";
            }
            var sql2 = @"
            INSERT INTO [dbo].[Users]
                   ([ID]
                   ,[Name]
                   ,[Pwd]
                   , Email)
             VALUES
                   (@userId
                   ,@userName
                   ,@Pwd
                   ,@email );";
            bool insertSuccess = dbHelper.ConnDb(conn =>
            {
                return conn.Execute(sql2, new
                {
                    userId,
                    userName,
                    pwd,
                    email
                }) == 1;
            });
            if (!insertSuccess)
            {
                return "新增使用者異常";
            }
            return "";
        }

        internal UserInfo GetUserInfo(string userId)
        {
            return dbHelper.ConnDb(conn =>
            {
                var sql = @"
                    select 
                        [ID]
                        ,[Name] 
                        , Email
                    from Users where Id = @userId";
                return conn.QueryFirst<UserInfo>(sql, new { userId });
            });
        }
    }
}
