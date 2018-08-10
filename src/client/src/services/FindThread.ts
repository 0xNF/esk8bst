import { IBSTError } from "src/models/BSTErrors";
import { IRedditData } from "../models/reddit";

// Scans the ESkate subreddit for the newst Buy Sell Trade thread
// Returns the url of the thread, or an error.
async function FindBuySellTradeThread(): Promise<string | IBSTError> {
    try {
        const obj = await _FetchFrontPage();
        if("Code" in obj) {
            return obj;
        }
        const bsturl: string | IBSTError = _ParseFrontPage(obj); 
        return bsturl;  
    } catch (e) {
        console.log(e);
        return {
            Code: 404,
            Text: "Couldn't find the BST thread!"
        };
    }
}

/**
 * Fetches the Eskate frontpage and returns its json object
 * or throws an error.
 */
async function _FetchFrontPage(): Promise<IRedditData | IBSTError> {
    const testing: boolean = false;
    let eurl: string = "https://old.reddit.com/r/ElectricSkateboarding/.json";
    if(testing) {
        eurl = "/data/frontpage.json";
    }
    return fetch(eurl)
    .then(response => {
      if (!response.ok) {
          // TODO make an actual IBST error here.
        throw new Error(response.statusText)
      }
      return response.json()
    }).catch((e: Error) => {
        console.log("wtf happened here");
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

/**
 * Given a JSON object of the Eskate front page,
 * scan it looking for something that looks like this months BST thread.
 * @param jobj 
 */
function _ParseFrontPage(jobj: IRedditData): string | IBSTError {
    const testing: boolean = false;
    if(testing) {
        return "/data/august01.json"
    }
    const err: IBSTError = {
        Code: 0,
        Text: ""
    };

    try {
        var children: any[] = jobj["data"]["children"];
        let url: string = "";
        for(let i = 0; i < children.length; i++) {
            let item = children[i];
            if("data" in item) {
                item = item["data"];
                if("title" in item && "url" in item) {
                    const title: string = item["title"];
                    if(_FuzzyMatchBST(title)) {
                        url = item["url"];
                        break;
                    }
                }
            }
        }
        if(url === "") {
            err.Code = 404
            err.Text = "No threads matched the BST format. Has one been made this month?";
            return err;
        } else {
            return url + ".json";
        }
    } catch {
        err.Code = 300,
        err.Text = "Crashed while parsing reddit thread."
        return err;
    }

}

/**
 * Attempts to match the given input
 * with some forgiving variants of BUY / SELL / TRADE
 * @param s
 */
function _FuzzyMatchBST(s: string): boolean {
    if(s.includes("BUY/SELL/TRADE Monthly Sticky")) {
        return true;
    }
    const splits: string[] = (s.toLocaleLowerCase().split(/",\/"\s/));
    if("buy" in splits && "sell" in splits && "trade" in splits && "monthly" in splits) {
        return true;
    }
    return false;
}



export { FindBuySellTradeThread }