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

* client/
    * models/
    * react/
        * components/
        * pages/
        * router/
    * services/
    * types/


Most of the magic happens in the Services, specifically `Services/FindThread.ts` and `Services/GetNewestData.ts`, which contain the fetcher and the parser.

# Known Bugs
* Tracking-Protection Enabled browsers will refuse to load the reddit json. 
* Posts for items designated in Canadian Dollars can parse weirdly at times
* Known Boards / Companies is static and hard to update, meaning obvious brands like OneWheel are bucketed into the `?`, also known as the `other / unknown` category. 
* Small screens have trouble displaying the Filter Zone
* Some links run out of their post box if they are long enough
