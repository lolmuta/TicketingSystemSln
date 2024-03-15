# 系統架構
- 使用 asp.net mvc 6開發
- SPA 前端使用 jquery
- 採用 jwt登入認證
- 所有需驗證的api 都要驗證jwt token 
- Need to prevent CRSF attack when POST action(未處理)
- 資料庫使用ado.net + dapper
- log使用serilog
- 使用 swagger
- 有建立簡單的單元測試

# API Spec
- 用swagger 表示

# db
- Acts (活動列表) 
	- ID (pk) 活動代號
	- Title 活動title
	- Descript 活動Descsript
- Acts_date(活動ref日期列表)
	- ID (pk) 活動日期代號
	- Acts_ID (fk) 活動代號
	- Date 活動日期
	- Count 票數庫存
	- price 單價
- Paids (付款列表)
	- ID (pk) 付款代號
	- Pay_Date 付款日期
	- Pay_Type 付款方式(一律 0 表示信用卡)
	- Send_Email 是否寄信( 0 表示未寄)
	- User_ID (fk) 購買人(使用者帳號)
- TempCart (購物車資料表)
	- ID (pk) 購物車代號
	- Acts_date_id (fk)活動日期代號
	- count 購買數量
	- user_id (fk)  購買人(使用者帳號)
- TicketsUUID (票的序號資料表)
	- UUID (pk) 購票流水號
	- Acts_date_id (fk)活動日期代號
	- Paids_ID (fk) 付款代號
- Users 
    - ID 使用者帳號
    - Name 姓名
    - Email 信箱
    

	
