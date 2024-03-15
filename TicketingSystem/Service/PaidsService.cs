using Dapper;
using TicketingSystem.Models;
using TicketingSystem.Utils;

namespace TicketingSystem.Repo
{
    public class PaidsService
    {
        private readonly DbHelper dbHelper;

        public PaidsService(DbHelper dbHelper)
        {
            this.dbHelper = dbHelper;
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

        internal void UpdateSendMailStatusToTrue(int paids_Id)
        {
            dbHelper.ConnDb(conn =>
            {
                var sql = @"
                    UPDATE [dbo].[Paids]
                       SET 
                            [SendEmail] = 1
                     WHERE ID = @paids_Id
                ";
                conn.Execute(sql, new { paids_Id });
            });
        }
    }
}
