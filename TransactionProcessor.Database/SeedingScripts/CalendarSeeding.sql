delete from calendar
declare @startdate date 
declare @enddate date

set @startdate = '2022-01-01'
set @enddate = dateadd(dd,-1,DATEADD(yy,5,@startdate))

set datefirst 1;

-- Build up the dates required
WITH CTE_Calendar as 
(SELECT @startdate as [date]
UNION ALL
SELECT DATEADD(dd,1,[date])
from CTE_Calendar
WHERE DATEADD(dd,1,[date]) <= @enddate)

	--[Date] [datetime2](7) NOT NULL,
	--[DayOfWeek] [nvarchar](max) NULL,
	--[DayOfWeekNumber] [int] NOT NULL,
	--[DayOfWeekShort] [nvarchar](max) NULL,
	--[MonthNameLong] [nvarchar](max) NULL,
	--[MonthNameShort] [nvarchar](max) NULL,
	--[MonthNumber] [int] NOT NULL,
	--[Year] [int] NOT NULL,
	--[WeekNumber] [int] NULL,
	--[YearWeekNumber] [nvarchar](max) NULL,
	--[WeekNumberString] [nvarchar](max) NULL,

--select [date] from CTE_Calendar

INSERT INTO calendar(Date,DayOfWeek, DayOfWeekNumber,DayOfWeekShort,MonthNameLong,MonthNameShort,MonthNumber,Year,WeekNumber,YearWeekNumber,WeekNumberString)
SELECT 
	[date] as date,
	DATENAME(WEEKDAY,[date]) as DayOfWeek, 
	DATEPART(WEEKDAY,[date]) as DayOfWeekNumber,
	SUBSTRING( DATENAME(WEEKDAY,[date]), 1, 3) as DayOfWeekShort,
	DATENAME(Month,[date]) as MonthNameLong,
	SUBSTRING( DATENAME(Month,[date]),1,3) as MonthNameShort,
	Month([date]) as MonthNumber,
	YEAR([date]) as Year,
	datepart(week, [date]) as WeekNumber,
	cast(YEAR([date]) as varchar) + ''+ RIGHT('00' + cast(DATEPART(WEEK,[date]) as varchar), 2) as YearWeekNumber,
	'Week Number ' + cast(datepart(week, [date]) as varchar) as WeekNumberString
FROM CTE_Calendar

OPTION (MAXRECURSION 10000);

select * from calendar