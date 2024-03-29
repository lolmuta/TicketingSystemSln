USE [TicketingSystem]
GO
/****** Object:  Table [dbo].[Acts]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Acts](
	[ID] [int] NOT NULL,
	[Title] [nvarchar](50) NULL,
	[Description] [nvarchar](200) NULL,
 CONSTRAINT [PK_Acts] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Acts_Date]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Acts_Date](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Acts_ID] [int] NULL,
	[Date] [datetime] NULL,
	[Count] [int] NULL,
	[Price] [decimal](18, 0) NULL,
	[Lock_Date] [datetime] NULL,
 CONSTRAINT [PK_Acts_Date] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Paids]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Paids](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Pay_Date] [datetime] NOT NULL,
	[Pay_Type] [int] NOT NULL,
	[Money] [decimal](18, 0) NOT NULL,
	[SendEmail] [int] NOT NULL,
	[User_ID] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Paids] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TempCart]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TempCart](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[User_ID] [nvarchar](50) NOT NULL,
	[Acts_Date_ID] [int] NOT NULL,
	[Count] [int] NOT NULL,
 CONSTRAINT [PK_TempCart] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TicketsUUID]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TicketsUUID](
	[UUID] [nvarchar](50) NULL,
	[Acts_Date_ID] [int] NOT NULL,
	[Paids_ID] [int] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Pwd] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](50) NULL
) ON [PRIMARY]
GO
INSERT [dbo].[Acts] ([ID], [Title], [Description]) VALUES (1, N'測試活動', N'這是測試活動')
GO
SET IDENTITY_INSERT [dbo].[Acts_Date] ON 

INSERT [dbo].[Acts_Date] ([ID], [Acts_ID], [Date], [Count], [Price], [Lock_Date]) VALUES (1, 1, CAST(N'2022-01-01T00:00:00.000' AS DateTime), 20, CAST(100 AS Decimal(18, 0)), CAST(N'2024-03-15T22:11:47.353' AS DateTime))
INSERT [dbo].[Acts_Date] ([ID], [Acts_ID], [Date], [Count], [Price], [Lock_Date]) VALUES (2, 1, CAST(N'2022-02-02T00:00:00.000' AS DateTime), 16, CAST(100 AS Decimal(18, 0)), CAST(N'2024-03-15T22:11:47.353' AS DateTime))
SET IDENTITY_INSERT [dbo].[Acts_Date] OFF
GO
INSERT [dbo].[Users] ([ID], [Name], [Pwd], [Email]) VALUES (N'admin', N'adminName', N'123', N'lolmuta@gmail.com')
INSERT [dbo].[Users] ([ID], [Name], [Pwd], [Email]) VALUES (N'admin2', N'admin2Name', N'123', N'lolmuta@gmail.com')
GO
/****** Object:  StoredProcedure [dbo].[usp_PostPaid]    Script Date: 2024/3/16 上午 03:36:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	回傳 數字, 訊息
-- 若數字為負，則為回傳錯誤
-- 若數字為正，則為回傳 paid_id
-- =============================================
CREATE PROCEDURE [dbo].[usp_PostPaid]
	@userId nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
    -- Insert statements for procedure here
	BEGIN TRY 
		BEGIN TRAN
		declare @now datetime = getdate();
		--鎖定 Acts_Date
		update Acts_Date set Acts_Date.lock_date = @now 
		from Acts_Date
		inner join TempCart on TempCart.Acts_Date_ID = Acts_Date.ID and TempCart.User_ID = @userId;
		
		--確認庫存量是否足夠
		if (exists(
				select 
					1
				from 
					Acts_Date 
				inner join TempCart on TempCart.Acts_Date_ID = Acts_Date.ID and TempCart.User_ID = @userId
				where Acts_Date.Count - TempCart.Count < 0))
			begin
			select -1 as success, '庫存不足' as message
			return;
			end

		--計算總額
		declare @sum decimal(18,0)
		select 
			@sum = sum(TempCart.Count * Acts_Date.Price)
			from 
				Acts_Date
			inner join TempCart on Acts_Date.ID = TempCart.Acts_Date_ID  and TempCart.User_ID = @userId
			group by TempCart.User_ID;

		--寫入購買
		INSERT INTO [dbo].[Paids]
           ([Pay_Date]
           ,[Pay_Type]
           ,[Money]
           ,[SendEmail]
           ,[User_ID])
		values ( 
			@now	
			, 0		
			, @sum  
			, 0		
			, @userId
		);

		declare @Paids_Id int = SCOPE_IDENTITY();
		--產生票的 uuid gen guid
		--暫存資料表，填入購物車的購買數量
		DECLARE @Result TABLE (
			Acts_Date_ID INT,
			[Count] INT
		);
		INSERT INTO @Result (Acts_Date_ID, [Count])
		SELECT 
			TempCart.Acts_Date_ID
			, TempCart.[Count]
		FROM TempCart
		where TempCart.User_ID = @userId;

		--對暫存資料表逐筆逐一執行產生ticket uuid
		DECLARE @Acts_Date_ID INT, @Count INT, @Counter INT;
		DECLARE @uuid nvarchar(50)
		WHILE EXISTS (SELECT 1 FROM @Result)
		BEGIN
			--從暫存資料取得其中一筆資料，該票的 Acts_Date_ID 與 count
			SELECT TOP 1 @Acts_Date_ID = Acts_Date_ID
				, @Count = [Count]
			FROM @Result;
			
			--按 Acts_Date_ID 的 count 填入 [TicketsUUID] 
			SET @Counter = 1;--計數器
			WHILE @Counter <= @Count
			BEGIN
				set @uuid = NEWID();
				INSERT INTO [dbo].[TicketsUUID]
					([UUID]
					,[Acts_Date_ID]
					,[Paids_ID])
				values 
					(@uuid
					, @Acts_Date_ID
					,@Paids_Id);

				DELETE FROM @Result WHERE Acts_Date_ID = @Acts_Date_ID;
				SET @Counter = @Counter + 1;
			END
		end
		
		--更新庫存
		update Acts_Date set 
			Acts_Date.Count = Acts_Date.Count - TempCart.Count 
		from Acts_Date 
		inner join TempCart on TempCart.Acts_Date_ID = Acts_Date.ID 
			and TempCart.User_ID = @userId;
		--刪除暫存購買清單
		delete TempCart where TempCart.User_ID = @userId;

		select 1 as success, @Paids_Id as message
		COMMIT TRAN

	END TRY
	BEGIN CATCH  
		ROLLBACK TRAN
		-- Execute error retrieval routine.  
		SELECT -1 as success , '[' + cast(ERROR_NUMBER() as char) + ']' + ERROR_MESSAGE() as message; 
	END CATCH;  
END
GO
USE [TicketingSystem]
GO

/****** Object:  StoredProcedure [dbo].[usp_GetPaidsList]    Script Date: 2024/3/16 下午 12:59:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[usp_GetPaidsList]
	-- Add the parameters for the stored procedure here
	@userId nvarchar(50)
AS
BEGIN
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   --declare @userId nvarchar(50) = 'admin'
-- 建立临时表，存储付款代号所购买的活动特定日期的票的张数
CREATE TABLE #TempPaids (
    Paids_ID INT
	, Acts_Date_ID Int
	, count int
);
-- 统计每个付款代号所购买的票的张数，并插入到临时表中
insert into #TempPaids (Paids_ID , Acts_Date_ID, count)
select TicketsUUID.Paids_ID
		, TicketsUUID.Acts_Date_ID
		, COUNT(TicketsUUID.Acts_Date_ID) as count 
	from TicketsUUID 
	group by TicketsUUID.Paids_ID, TicketsUUID.Acts_Date_ID;
	
-- 查询用户购买的活动票的详细信息，并按付款代号分组和拼接日期列表
   SELECT [Paids].[ID]
	  ,Acts.Title
	  ,Acts.Description
	  ,STUFF(
		        (
			        SELECT ',' 
				        + format(Acts_Date.[Date], 'yyyy/MM/dd HH:mm') 
				        + ' ' 
						+ cast(#TempPaids.count as nvarchar(20))
						+ '張'
			        from #TempPaids 
					inner join Acts_Date on Acts_Date.ID = #TempPaids.Acts_Date_ID 
					inner join Acts on Acts.ID = Acts_Date.Acts_ID
					where Paids.ID = #TempPaids.Paids_ID
			        FOR XML PATH('')
		        ), 1, 1, '') AS Dates
      ,format([Paids].[Pay_Date], 'yyyy/MM/dd HH:mm:ss') as payDate
      ,[Paids].[SendEmail] as sendEmail
FROM 
	[dbo].[Paids]
inner join #TempPaids on #TempPaids.Paids_ID = Paids.ID
inner join Acts_Date on Acts_Date.ID = #TempPaids.Acts_Date_ID 
inner join Acts on Acts.ID = Acts_Date.Acts_ID
where 1 = 1
	and [Paids].[User_ID] = @userId
group by 
	[Paids].[ID], Acts.Title
	, Acts.Description
	,[Paids].[SendEmail]
	,[Paids].[Pay_Date]
drop table #TempPaids;
END

GO


