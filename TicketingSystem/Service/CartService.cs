using Dapper;
using System.Data;
using TicketingSystem.Models;
using TicketingSystem.Utils;

namespace TicketingSystem.Service
{
    public class CartService
    {
        private readonly DbHelper dbHelper;

        public CartService(DbHelper dbHelper)
        {
            this.dbHelper = dbHelper;
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
        internal int GetCurBuyCount(int id, string userId)
        {
            return dbHelper.ConnDb(conn =>
            {
                var result = conn.Query<int>(@"
                    SELECT 
                        [Count]
                    FROM [TempCart]
                    where [User_ID] = @userId
                    and [Acts_Date_ID] = @id", new { id, userId });

                if (result.Count() > 0)
                {
                    return result.First();
                }
                else
                {
                    return 0;
                }
            });
        }
        internal string PostAddToTempCart(int id, int count, string userId)
        {
            int ticketCount = GetTicketCount(id);
            int curBuyCount = GetCurBuyCount(id, userId);
            bool isOverBuy = (curBuyCount + count) > ticketCount;
            if (isOverBuy)
            {
                return "已經超過庫存量";
            }

            int effectRows = dbHelper.ConnDb(conn =>
            {
                return conn.Execute(@"
                    MERGE INTO [dbo].[TempCart] AS Target
                    USING (VALUES (@User_ID, @Acts_Date_ID, @Count)) AS Source 
                        ([User_ID], [Acts_Date_ID], [Count]) ON Target.[User_ID] = Source.[User_ID] AND Target.[Acts_Date_ID] = Source.[Acts_Date_ID]
                    WHEN MATCHED THEN
                        UPDATE SET Target.[Count] = Target.[Count] + Source.[Count]
                    WHEN NOT MATCHED THEN
                        INSERT ([User_ID], [Acts_Date_ID], [Count])
                        VALUES (Source.[User_ID], Source.[Acts_Date_ID], Source.[Count]);

                ", new { Acts_Date_ID = id, Count = count, User_ID = userId });
            });
            if (effectRows == 1)
            {
                return "";
            }
            else
            {
                return "更新失敗";
            }
        }

        internal string PostDeleteCart(int id, string userId)
        {
            return dbHelper.ConnDb(conn =>
            {
                int effectRows = conn.Execute("delete TempCart where id = @id and user_id=@userId;", new { id, userId });
                if (effectRows == 1)
                {
                    return "";
                }
                else
                {
                    return "刪除異常";
                }
            });
        }

        internal string PostPaid(string userId, out int Paids_Id)
        {
            Paids_Id = -1;
            var storedProcedureName = "usp_PostPaid";
            var values = new { userId };
            SpResult spResult = dbHelper.ConnDb(conn =>
            {
                return conn.QueryFirst<SpResult>(storedProcedureName, values,
                            commandType: CommandType.StoredProcedure);
            });
            bool 預存執行是否有誤 = spResult.success < 0;
            if (預存執行是否有誤)
            {
                return spResult.message;
            }

            Paids_Id = int.Parse(spResult.message);

            return "";
        }

        internal IEnumerable<CartInfo> GetCartList(string userId)
        {
            return dbHelper.ConnDb(conn =>
            {
                var sql = @"
                    select 
	                    [TempCart].id
	                    , [Acts].title
	                    , [Acts].description
	                    , format([Acts_Date].date, 'yyyy/MM/dd HH:mm') as date
	                    ,[TempCart].Count
                        , Acts_Date.price
                    from 
	                    [TempCart]
                    join [Acts_Date] on [Acts_Date].ID = [TempCart].Acts_Date_ID
                    join [Acts] on [Acts].ID = [Acts_Date].Acts_ID
                    where 1 = 1
	                    and [TempCart].User_ID = @userId
                ";
                return conn.Query<CartInfo>(sql, new { userId });
            });
        }
    }
}