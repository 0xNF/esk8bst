# ESK8BST
Deployed at https://esk8bst.netlify.com

A small webapp that scans the monthly reddit <a href="https://old.reddit.com/r/ElectricSkateboarding/">/r/ElectricSkateboarding</a> Buy / Sell / Trade thread and displays high signal posts in a more digestable format. Allows for sorting and filtering on a number of fields.  

Future updates are likely to include stats on the number of boards available, as well as email notifications if a board you want comes up for sale.  

The site is all in-browser and doesn't require a back-end server. Reddit's API is used to collect thread information when you load the page.

This is a project built with TypeScript 3 and React. Shout out to Microsoft's [TypeScript-React Starter](https://github.com/Microsoft/TypeScript-React-Starter) for making it easy to get started with TypeScript.

# Building

`npm install`

`npm run start`

# Architecture

## Front End

* client/
    * models/
    * react/
        * components/
        * pages/
        * router/
    * services/
    * types/


Most of the magic happens in the Services, specifically `Services/FindThread.ts` and `Services/GetNewestData.ts`, which contain the fetcher and the parser.

## Database
Google's Cloud Firestore is the datastore. It is structured thusly:  

* Databases/  
    * Scan/  
    * Matches/
    * Preconfirmed/

### Scan
The scan collection contains a single document with id `ScanData` structured like:
```TypeScript
{
    LastScanDate: Timestamp
}
```

### Matches
The matches object contains one collection for each user, where the id is their email. Each document contains an array of Match objects, each describing one potential search query the user wants to search for over the Buy Sell Trade thread:  

```TypeScript
{
    matches: [
        {
            bst: string,
            currency: string,
            price: number,
            companies: [
                string
            ]
        }
    ]
}
```

Although both the Datastore and the Lambda's support an arbitrary number of matches, the front end presently only supports one match per user.  


### Preconfirmed
Stores a sha256 of each email that has previously opted-in so that additional subscription requests in the futrue, for example, to edit their match object, aren't locked behind another opt-in email.

Each object is simply of the format:

```TypeScript
sha256(email)

{
    exists: true
}
```
where the document id is the hash and the only field in the document is an exists field set to true. They are non-empty to avoid Firebase automatically pruning them.

## Backend 
This project uses AWS Lambda to host 4 functions:  

1. Subscribe  
2. Confirm Subscribe  
3. Unsubscribe  
4. Scan  

Email is quite tricky, finnicky, and dependant upon, among a hundred other things, a senders 'reputation'. One way for a sender to main good reputation is to support double opt-in. In our architecture, a user requests to be subscribed by hitting the `Subscribe` endpoint, the first Lambda sends a confirmation email to the user, the user clicks a link that email which pings the `Confirm Subscribe` endpoint, and the second lambda registers their email.

`Unsubscribe` is another lambda function, which starts when a user hits the unsubscribe endpoint. This removed their email from Firestore.

And finally, the `Scan` lambda does the hard work of fetching subscribers, fetching the reddit thread, parsing into machine readable formats, determining if any user had any matching new posts, and sending out daily emails. This Lambda is only triggerable by a scheduled cronjob from AWS every hour. It is not available via any HTTP endpoint.

## Email
Email is handled with the Mailgun API.

# Known Bugs
* Tracking-Protection Enabled browsers will refuse to load the reddit json. 
* Posts for items designated in Canadian Dollars can parse weirdly at times
* Known Boards / Companies is static and hard to update, meaning obvious brands like OneWheel are bucketed into the `?`, also known as the `other / unknown` category. 
* Small screens have trouble displaying the Filter Zone
* Some links run out of their post box if they are long enough
