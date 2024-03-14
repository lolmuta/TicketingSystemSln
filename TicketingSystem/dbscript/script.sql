USE [TicketingSystem]
GO
/****** Object:  Table [dbo].[Acts]    Script Date: 2024/3/14 下午 04:56:02 ******/
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
/****** Object:  Table [dbo].[Acts_Date]    Script Date: 2024/3/14 下午 04:56:02 ******/
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
/****** Object:  Table [dbo].[Paids]    Script Date: 2024/3/14 下午 04:56:02 ******/
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
/****** Object:  Table [dbo].[TempCart]    Script Date: 2024/3/14 下午 04:56:02 ******/
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
/****** Object:  Table [dbo].[TicketsUUID]    Script Date: 2024/3/14 下午 04:56:02 ******/
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
/****** Object:  Table [dbo].[Users]    Script Date: 2024/3/14 下午 04:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Pwd] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[usp_PostPaid]    Script Date: 2024/3/14 下午 04:56:02 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[usp_PostPaid]
	@userId nvarchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	--tempcart => paid 
	-- insert paids
    --tempcart => tickets uuid
    --update tempcart
    --update Acts_Date
    -- Insert statements for procedure here
	BEGIN TRY 
		BEGIN TRAN
		declare @now datetime = getdate();
		--鎖定 Acts_Date
		update Acts_Date set Acts_Date.lock_date = @now 
		where 1 =1 
			and Acts_Date.ID in (
				select TempCart.Acts_Date_ID from TempCart where User_ID = @userId
			);
		--確認庫存量是否足夠
		if (exists(
				select 
					1
				from 
					Acts_Date 
				inner join TempCart on TempCart.Acts_Date_ID = Acts_Date.ID and TempCart.User_ID = @userId
				where Acts_Date.Count - TempCart.Count < 0))
			begin
			select '庫存不足'
			return;
			end

		--寫入購買
		declare @sum decimal(18,0)
		select 
			@sum = sum(TempCart.Count * Acts_Date.Price)
			from 
				TempCart
			inner join Acts_Date on Acts_Date.ID = TempCart.Acts_Date_ID
			where TempCart.User_ID = @userId
			group by TempCart.User_ID;


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
		--gen guid
		INSERT INTO [dbo].[TicketsUUID]
				   ([UUID]
				   ,[Acts_Date_ID]
				   ,[Paids_ID])
			select 
				newid() as UUID
				,TempCart.Acts_Date_ID as [Acts_Date_ID]
				,@Paids_Id as [Paids_ID]
			from 
				TempCart
			where 
				TempCart.User_ID = @userId;
		--更新庫存
		update Acts_Date set 
			Acts_Date.Count = Acts_Date.Count - TempCart.Count 
		from Acts_Date 
		inner join TempCart on TempCart.Acts_Date_ID = Acts_Date.ID 
			and TempCart.User_ID = @userId;
		--刪除暫存購買清單
		delete TempCart where TempCart.User_ID = @userId;

		select ''
		COMMIT TRAN

	END TRY
	BEGIN CATCH  
		ROLLBACK TRAN
		-- Execute error retrieval routine.  
		SELECT '[' + cast(ERROR_NUMBER() as char) + ']' + ERROR_MESSAGE(); 
	END CATCH;  
END
GO
