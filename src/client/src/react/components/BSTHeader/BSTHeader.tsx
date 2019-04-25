import * as React from 'react';
import { IBSTThread } from 'src/models/IBST';
import logo from 'src/logo.svg';
import { BSTNav } from 'src/react/components/BSTNav/BSTNav';


interface BSTHeaderProps {
  threads?: Array<IBSTThread>;
  error?: boolean;
}

function BSTHeader(props: BSTHeaderProps) {
  const metaUrls: Array<string> = props.threads ? props.threads.map(x => x.Metadata.ThreadUrl) : ["/"];
  const metaTitle: string = (props.threads || props.error) ?
    "BUY/SELL/TRADE Monthly Sticky Reader"
    :
    "Loading...";
  return (
    <header className="App-header">
      <BSTNav />
      <img src={logo} className="App-logo" alt="logo" />
      <a href={metaUrls[0]} className="titleLink">
        <h1 className="App-title">
          {metaTitle}
        </h1>
        {
          metaUrls.length > 0 ? metaUrls.forEach(x => {
            <div>hello</div>
          })
            :
            ""
        }
      </a>
      {
        props.threads ?
          (
            <div>
              <p>Thread created on {props.threads[0].Metadata.ThreadCreatedAt.toDateString()}</p>
              <div>
                <div className="BSTMeta">
                  Looking to Sell: {props.threads.reduce((prev, curr) => { return prev + curr.Metadata.TotalSell }, 0)}<br />
                  Looking to Buy: {props.threads.reduce((prev, curr) => { return prev + curr.Metadata.TotalBuy }, 0)}<br />
                  Looking to Trade: {props.threads.reduce((prev, curr) => { return prev + curr.Metadata.TotalTrades }, 0)}<br />
                </div>
              </div>
            </div>
          )
          :
          ""
      }

    </header>
  );
}

export { BSTHeader, BSTHeaderProps }