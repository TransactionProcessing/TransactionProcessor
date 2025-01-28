CREATE OR ALTER VIEW uvwSettlements
AS

SELECT
	s.SettlementId,
	s.SettlementDate,
	s.IsCompleted,
	DATENAME(WEEKDAY, s.SettlementDate) as DayOfWeek,
	DATEPART(wk, s.SettlementDate) as WeekNumber,
	DATENAME(MONTH, s.SettlementDate) as Month,
	DATEPART(MM, s.SettlementDate) as MonthNumber,
	YEAR(s.SettlementDate) as YearNumber,
	f.CalculatedValue,
	t.TransactionId,
	e.EstateId,
	m.MerchantId,
	m.Name as MerchantName,
	cptf.Description as FeeDescription,
	o.Name as OperatorIdentifier,
	CAST(ISNULL(tar.Amount,0) as decimal) as Amount,
	f.IsSettled
from settlement s 
inner join merchantsettlementfee f on s.SettlementId = f.SettlementId
inner join [transaction] t on t.TransactionId = f.TransactionId
inner join [merchant] m on t.MerchantId = m.MerchantId
inner join [estate] e on e.EstateId = m.EstateId
left outer join contractproducttransactionfee cptf on f.ContractProductTransactionFeeId = cptf.ContractProductTransactionFeeId
left outer join transactionadditionalrequestdata tar on tar.TransactionId = t.TransactionId
inner join contract c on c.ContractId = t.ContractId
inner join operator o on o.OperatorId = c.operatorid