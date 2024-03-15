using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using TicketingSystem.Models;
using TicketingSystem.Utils;

namespace TicketingSystem.Repo
{
    public class ActService
    {
        private readonly DbHelper dbHelper;

        public ActService(DbHelper dbHelper)
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

        internal string CreateUser(string userId, string userName, string pwd)
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
                   ,[Pwd])
             VALUES
                   (@userId
                   ,@userName
                   ,@Pwd);";
            bool insertSuccess = dbHelper.ConnDb(conn =>
            {
                return conn.Execute(sql2, new
                {
                    userId,
                    userName,
                    pwd
                }) == 1;
            });
            if (!insertSuccess)
            {
                return "新增使用者異常";
            }
            return "";
        }

        

        internal int GetTicketCount(int id)
        {
            return dbHelper.ConnDb(conn =>
            {
                return conn.ExecuteScalar<int>(@"
                    SELECT 
                        [Acts_Date].[Count]
                    FROM [Acts_Date]
                    where [Acts_Date].ID = @id
                ", new { id });
            });
        }

        internal IEnumerable<SelectListItem> GetDDlActDates(int id)
        {
            return dbHelper.ConnDb(conn =>
            {
                return conn.Query<SelectListItem>(@"
                    SELECT 
                        ID as Value
                        ,format([Date],'yyyy/MM/dd HH:mm') as Text 
                    FROM [dbo].[Acts_Date]
                    where 1 = 1 
                        and [Acts_Date].Count>0
                        and Acts_Date.[Acts_ID] = @id
                ", new { id });
            });
        }

        internal ActDetail GetActDetail(int id)
        {
            return dbHelper.ConnDb(conn =>
            {
                return conn.QueryFirst<ActDetail>(@"
                    SELECT TOP (1000) [ID]
                          ,[Title]
                          ,[Description]
                    FROM [dbo].[Acts] 
                    where
                        Acts.ID = @id
                ", new { id });
            });
        }

        internal IEnumerable<ActInfo> GetActList()
        {
            return dbHelper.ConnDb(conn =>
            {
                var sql = @"
                    SELECT TOP (1000) Acts.[ID]
                          ,Acts.[Title]
                          ,Acts.[Description]
	                      ,STUFF(
		                    (
			                    SELECT ',' 
				                    + format([Date], 'yyyy/MM/dd HH:mm') 
				                    + ' ' 
				                    + case when [Acts_Date].Count = 0 then '已滿' 
					                    else cast([Acts_Date].Count as nvarchar(10)) + '張' 
				                    end
			                    FROM [Acts_Date]
			                    where [Acts_Date].Acts_ID = [Acts].ID
			                    FOR XML PATH('')
		                    ), 1, 1, '') AS Dates
                        ,(ROW_NUMBER() OVER (ORDER BY [Acts].id)) AS RowNumber
                      FROM [dbo].[Acts]
                ";
                return conn.Query<ActInfo>(sql);
            });
        }
    }
}
