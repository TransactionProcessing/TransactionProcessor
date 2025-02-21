CREATE OR REPLACE VIEW uvwMerchantBalanceHistory
AS
SELECT 
	OriginalEventId, 
	ChangeAmount, 
	DateTime as EntryDateTime, 
	Reference, 
	DebitOrCredit,
	MerchantId,
	SUM(CASE DebitOrCredit
			WHEN 'D' THEN ChangeAmount * -1
			ELSE ChangeAmount
		END) over (PARTITION BY MerchantId order by datetime,OriginalEventId) as Balance
FROM [MerchantBalanceChangedEntry]