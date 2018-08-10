type BuySellTrade =
    | "BUY"
    | "SELL"
    | "TRADE"
    | "NULL"
    ;

type TransactionStatus = 
    | "IN-PROGRESS"
    | "OPEN"
    | "SOLD"
    | "COMPLETED"
    | "NULL"
    ;


export { BuySellTrade, TransactionStatus }