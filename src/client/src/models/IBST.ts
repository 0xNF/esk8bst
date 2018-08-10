import { BuySellTrade, TransactionStatus } from "src/types/TransactionStatus";

interface IBSTThreadMetadata {
    TotalPosts: number;
    TotalSell: number;
    TotalBuy: number;
    TotalTrades: number;
    TotalClosed: number; // posts marked as SOLD or canceled
    TotalOpen: number; 
    ThreadCreatedAt: Date; // number?
    ThreadTitle: string;
    ThreadUrl: string;
    LastUpdated: Date;
}

interface IBSTThreadComment {
    Seller: string; // The posting user
    DatePosted: Date;
    NumberOfReplies: number;
    BST: BuySellTrade; // Whether this post is buying, selling, or trading
    TransactionStatus: TransactionStatus; // Whether this post is sold or open, etc
    Text: string;
    Price: number;
    Currency: string;
    Location: string;
    Url: string;
    Company: string;
    Product: string;
}

interface IBSTThread {
    Metadata: IBSTThreadMetadata;
    BSTs: Array<IBSTThreadComment>;
}

const DefautThread: IBSTThread = {
    BSTs: [],
    Metadata: {
      LastUpdated: new Date(Date.now()),
      ThreadCreatedAt: new Date(Date.now()),
      ThreadTitle: "Loading...",
      ThreadUrl: "/404",
      TotalBuy: 0,
      TotalClosed: 0,
      TotalOpen: 0,
      TotalPosts: 0,
      TotalSell: 0,
      TotalTrades: 0
    }
  };

export { IBSTThreadMetadata, IBSTThreadComment, IBSTThread, DefautThread }