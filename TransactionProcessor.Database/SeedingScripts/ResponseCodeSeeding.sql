SET IDENTITY_INSERT ResponseCodes ON

INSERT INTO ResponseCodes(ResponseCode,Description)
SELECT 0, 'Success' UNION ALL
SELECT 1000, 'Unknown Device' UNION ALL
SELECT 1001, 'Unknown Estate' UNION ALL
SELECT 1002, 'Unknown Merchant' UNION ALL
SELECT 1003, 'No Devices Configured' UNION ALL
SELECT 1004, 'No Operators Configured (Estate)' UNION ALL
SELECT 1005, 'Unkown Operator (Estate)' UNION ALL
SELECT 1006, 'No Operators Configured (Merchant)' UNION ALL
SELECT 1007, 'Unkown Operator (Merchant)' UNION ALL
SELECT 1008, 'Declined by Operator' UNION ALL
SELECT 1009, 'Insufficient Funds' UNION ALL
SELECT 1010, 'Operator Comms Error' UNION ALL
SELECT 1011, 'Invalid Amount' UNION ALL
SELECT 9999, 'Unknown Error'

SET IDENTITY_INSERT ResponseCodes OFF