CREATE OR REPLACE VIEW uvwSettlements
AS 
SELECT
	s.SettlementId,
	s.SettlementDate,
	s.IsCompleted,
	FORMAT(s.SettlementDate, 'dddd') as DayOfWeek,
	DATEPART(wk, s.SettlementDate) as WeekNumber,
	FORMAT(s.SettlementDate, 'MMMM') as Month,
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
inner join merchantsettlementfee f on s.SettlementReportingId = f.SettlementReportingId
inner join [transaction] t on t.TransactionReportingId = f.TransactionReportingId
inner join [merchant] m on t.MerchantReportingId = m.MerchantReportingId
inner join [estate] e on e.EstateReportingId = m.EstateReportingId
left outer join contractproducttransactionfee cptf on f.TransactionFeeReportingId = cptf.TransactionFeeReportingId
left outer join transactionadditionalrequestdata tar on tar.TransactionReportingId = t.TransactionReportingId
inner join contract c on c.ContractReportingId = t.ContractReportingId
inner join operator o on o.OperatorId = c.operatorid
