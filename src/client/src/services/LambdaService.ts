import { Subscriber } from "src/models/subscriber";

const subscribeEndoint: string = "https://1lol87xzbj.execute-api.us-east-2.amazonaws.com/Prod/subscribe"//"https://lambda.esk8bst.com/subscribe";
const unsubscribeEndoint: string = "https://1lol87xzbj.execute-api.us-east-2.amazonaws.com/Prod/subscribe?confirmkey=";//"https://lambda.esk8bst.com/unsubscribe";


function postData(url = ``, data = {}) {
    // Default options are marked with *
      return fetch(url, {
          method: "POST", // *GET, POST, PUT, DELETE, etc.
          mode: "no-cors", // no-cors, cors, *same-origin
          cache: "no-cache", // *default, no-cache, reload, force-cache, only-if-cached
          credentials: "same-origin", // include, same-origin, *omit
          headers: {
              "Content-Type": "application/text; charset=utf-8",
              // "Content-Type": "application/x-www-form-urlencoded",
          },
          redirect: "follow", // manual, *follow, error
          referrer: "no-referrer", // no-referrer, *client
          body: JSON.stringify(data), // body data type must match "Content-Type" header
      })
      .then(response => response.json()); // parses response to JSON
  }

async function SubmitSubscriber(sub: Subscriber) {
    const s = JSON.stringify(sub);
    console.log("stringified: ");
    console.log(s);
    await postData(subscribeEndoint, sub);
}

async function UnsubscribeEmail(email: string) {
    // Default options are marked with *
    return fetch(unsubscribeEndoint+email, {
        method: "DELETE", // *GET, POST, PUT, DELETE, etc.
        mode: "cors", // no-cors, cors, *same-origin
        cache: "no-cache", // *default, no-cache, reload, force-cache, only-if-cached
        credentials: "same-origin", // include, same-origin, *omit
        headers: {
            "Content-Type": "application/json; charset=utf-8",
            // "Content-Type": "application/x-www-form-urlencoded",
        },
        redirect: "follow", // manual, *follow, error
        referrer: "no-referrer", // no-referrer, *client
    })
    .then(response => response.json()); // parses response to JSON
}

export { SubmitSubscriber, UnsubscribeEmail }