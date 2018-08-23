import { IRedditData } from "src/models/reddit";
import { Company } from "src/models/company";
import { Board } from "src/models/board";
import { TransactionStatus } from "src/types/TransactionStatus";
import { BuySellTrade } from "src/types/BuySellTrade";
import { IBSTThreadMetadata, IBSTThreadComment, IBSTThread } from "src/models/IBST";
import { IBSTError } from "../models/BSTErrors";


// Gets the header and comments for the Buy Sell Trade thread at the given threadurl
function GetBuySellTradeThreadData(threadurl: string) : Promise<IRedditData | IBSTError> {
    console.log("the bst thread is at: " + threadurl);
    return fetch(threadurl)
        .then(response => {
        if (!response.ok) {
            // TODO make actual IBSTError here
            throw new Error(response.statusText);
        }
        return response.json()
        }).catch((e: Error) => {
            console.log(e.name);
            console.log(e.message);
            const err: IBSTError = {
                Code: 0,
                Text: ""
            };

            if(e.message === "NetworkError when attempting to fetch resource.") {
                err.Code = 403;
                err.Text = "CORS error, check privacy settings";
            } else if (e.message === "Failed to fetch") {
                err.Code = 404;
                err.Text = "File not found";
            }
            return err;            
        })
}


// Given a Reddit API thread object,
// we parse out or compute the relevant metadata to show the user.
// We include: 
function _GetThreadMetadata(jobj: IRedditData) : IBSTThreadMetadata {
    const tmeta : any = jobj[0]["data"]["children"][0]["data"];
    const title = tmeta["title"];
    const posts = tmeta["num_comments"]
    const totalSell =  0;
    const totalBuy = 0;
    const created = new Date(tmeta["created_utc"] * 1000);
    const modified = new Date(tmeta["created_utc"] * 1000);
    const perma = tmeta["url"]
    const totalClosed = 0;
    const totalOpen = 0;
    const totalTrades = 0;

    const obj = {
        TotalPosts: posts,
        TotalSell: totalSell,
        TotalBuy: totalBuy,
        TotalTrades: totalTrades,
        ThreadCreatedAt: created,
        ThreadTitle: title,
        LastUpdated: modified,
        ThreadUrl: perma,
        TotalClosed: totalClosed,
        TotalOpen: totalOpen,
    };

    return obj;
}

// Given JSON, crawl the thread counting each "replies" non-empty field
function _parseReplyCount(jobj: any, accul: number = 0) : number {
    let r = accul;
    if ( jobj === null 
        || typeof(jobj) !== typeof({}) 
        || !("replies" in jobj)
        || jobj["replies"] === null
        || jobj["replies"] === ""
        || !("data" in jobj["replies"])
        || !("children" in jobj["replies"]["data"])
     ) {
        return 0;
    }

    const children = jobj["replies"]["data"]["children"];
    for(let i = 0; i < children.length; i++) {
        r = _parseReplyCount(children[i], r) + 1;
    }

    return r;
}

const forSale: RegExp = new RegExp("[\\[\\(].*?(sell|sale|wts|lts).*?[\\)\\]]", 'i');
const forPurchase: RegExp = new RegExp("[\\[\\(].*?(buy|ltb|wtb|purchase).*?[\\)\\]]", 'i')
const forTrade: RegExp = new RegExp("[\\[\\(].*?(trade|wtt|swap).*?[\\)\\]]", 'i');
function _parseBuySellTradeType(s: string): BuySellTrade {
    if(forSale.test(s)){
        return "SELL";
    }
    if(forPurchase.test(s)){
        return "BUY";
    }
    if(forTrade.test(s)){
        return "TRADE";
    }
    return "NULL";
}

const sold: RegExp = new RegExp("[\\[\\(].*?(sold|no longer available).*?[\\)\\]]", 'i');
function _parseTransactionStatus(s: string) : TransactionStatus {
    if(sold.test(s)){
        return "SOLD";
    }
    return "OPEN";
}

/* all these regexs are the same format:
* [list of terms defining the currency] + number 
* or
* number + [list of terms defining the currency]
* Each has two capture groups, and the first one to not not undefined wins
* all other grups are non-capture. */
const currencyGBP: [string, RegExp] = [
    "gbp",
    new RegExp("(?:(?:gbp|\\$gbp|£gbp|£)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:gbp|\\$gbp|£gbp|£)))", 'i')

];
const currencyEUR: [string, RegExp] = [
    "eur",
    new RegExp("(?:(?:eur|\\$eur|€)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:eur|\\$eur|€)))", 'i')

];
const currencyAUD: [string, RegExp] = [
    "aud",
    new RegExp("(?:(?:AUD|\\$AUD|Austrailian Dollars)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:AUD|\\$AUD|Austrailian Dollars)))", 'i')
];
const currencyUSD: [string, RegExp] = [
    "usd", 
    new RegExp("(?:(?:USD|\\$USD|\\$)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:USD|\\$USD|\\$)))", 'i')
];
const currencyCAD: [string, RegExp] = [
    "cad",
    new RegExp("(?:(?:CAD|\\$CAD|CND|\\$CND|Canadian Dollars)\\s*?\\$?\\s*?([0-9]{1,5}))|(?:\\$?\\s*?(?:([0-9]{2,5})\\s*?(?:CAD|\\$CAD|CND|\\$CND|Canadian Dollars)))", 'i')
];
function _parcePriceAndCurrency (s: string) : [string, number] {
    const regexprarr: Array<[string, RegExp]> = [currencyGBP, currencyEUR, currencyAUD, currencyUSD, currencyCAD]; 
    s = s.replace(',','').replace('`', '').replace("'", '')
    for(let i = 0; i < regexprarr.length; i++) {
        const cur: string = regexprarr[i][0];
        let price: number = 0;
        const regresult: null | RegExpExecArray = regexprarr[i][1].exec(s);
        if(regresult) {
            const p = parseInt(regresult[1]);
            const p2 = parseInt(regresult[2]);
            if(!isNaN(p)){
                price = p;
            }else if(!isNaN(p2)){
                price = p2;
            }
            return [cur, price];
        }
    }

    // if everything turned up empty:
    return ["", 0];
}

function _parseCompany(s: string, companies: Array<[string, RegExp]>) : string {
    s = s.toLocaleLowerCase();
    // Pass one, direct match
    for(let i = 0; i < companies.length; i++){
        const creg = companies[i][1];
        const company = companies[i][0];
        if(creg.test(s)){
            return company;
        }
    }
    return "?";
}

function _parseProduct(s: string, products: Array<[Board, RegExp]>): [string, string] {
    for(let i = 0; i < products.length; i++) {
        const preg = products[i][1];
        const product = products[i][0];
        if(preg.test(s)) {
            return [product.company, product.board];
        }
    }
    return ["?", "?"];
}


// Given a Reddit API thread object
// We parse out the individual comments and figure out whih items are BUY, SELL, and other per-comment metadat
function _ParseBSTComments(jobj: IRedditData) : Array<IBSTThreadComment> {
    const tcom : any = jobj[1]["data"]["children"];
    const comments: Array<IBSTThreadComment> = [];
    for(let i = 0; i < tcom.length; i++) {
        try {
            const com = tcom[i]["data"];
            const body = com["body"];
            const author = com["author"];
            const created_utc = com["created_utc"];
            const perma = com["permalink"];
            if(!com || !body || !author || !created_utc || !perma) {
                console.log("encountered a malformed comment at index " + i );
                continue;
            }
            const posted = new Date(created_utc * 1000);
            const url = "https://reddit.com" + perma;
            const bst: BuySellTrade = _parseBuySellTradeType(body);
            const tstatus: TransactionStatus = _parseTransactionStatus(body);
            const replycount = _parseReplyCount(com);
            const priceAndCurr =  _parcePriceAndCurrency(body);
            let company = _parseCompany(body, Companies);
            const productTuple = _parseProduct(body, Boards);
            company = productTuple[0] === "?" ? company : productTuple[0];
            const product = productTuple[1];
            
            const comment : IBSTThreadComment = {
                Seller: author,
                DatePosted: posted,
                Url: url,
                NumberOfReplies: replycount,
                Text: body,
                Currency: priceAndCurr[0],
                Price: priceAndCurr[1],
                Location: "",
                BST: bst,
                TransactionStatus: tstatus,
                Company: company,
                Product: product,
            };

            if(comment.BST !== "NULL") { 
                comments.push(comment);
            } 
        } catch(e) {
            console.log("an error happened at index: " + i);
            console.log(e);
        }
        
    }
    return comments;
}

let Companies: Array<[string,RegExp]> = [];
let Boards: Array<[Board, RegExp]> = [];
function ParseData(jobj: IRedditData, companies: Company[], boards: Board[]): IBSTThread {
    const companiesSorted: string[] = companies.map(x => x.company).sort((a,b)=> b.length - a.length)
    for(let i = 0; i < companiesSorted.length; i++){
        const s = "\\s" + companiesSorted[i] + "[\\s\\,\\./\\\!\\'\\\"\\`]";
        const r: RegExp = new RegExp(s, 'i');
        Companies.push([companiesSorted[i], r]);
    }
    for( let i = 0; i < boards.length; i++) {
        const s = "\\s" + boards[i].board + "[\\s\\,\\./\\\!\\'\\\"\\`]";
        const r: RegExp = new RegExp(s, 'i');
        Boards.push([boards[i], r]);
    }
    /* Add one more custom element to the boards: a catchall for DIY boards */
    Boards.push([{company: "DIY", board: "?"}, new RegExp("\\s(?:DIYEboard|DIY\\s?Eboard|DIY\\s?E\\s?Board|DIY\s?e-board)", 'i')])
    const meta: IBSTThreadMetadata = _GetThreadMetadata(jobj);
    const comments : Array<IBSTThreadComment> = _ParseBSTComments(jobj);

    comments.forEach((val,idx,arr) => {
        switch(val.BST){
            case "BUY":
                meta.TotalBuy += 1;
                break;
            case "SELL":
                meta.TotalSell += 1;
                break;
            case "TRADE":
                meta.TotalTrades += 1;
                break;
            default:
                return; //Don't count null values or weird
        }

        switch(val.TransactionStatus){
            case "OPEN":
                meta.TotalOpen += 1;
                break;
            case "SOLD":
                meta.TotalClosed += 1;
                break;
            default:
                break;
        }

    });
    const obj = {
        Metadata: meta,
        BSTs: comments,
    };
    return obj;
}


export { GetBuySellTradeThreadData, ParseData }