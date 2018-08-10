import * as React from 'react';
import { IBSTThread } from 'src/models/IBST';
import logo from 'src/logo.svg';
import { BSTNav } from 'src/react/components/BSTNav/BSTNav';


interface BSTHeaderProps {
    thread?: IBSTThread;
    error?: boolean;
}

function BSTHeader(props: BSTHeaderProps) {
    const metaUrl: string = props.thread ? props.thread.Metadata.ThreadUrl : "/";
    const metaTitle: string = (props.thread || props.error) ? 
          "BUY/SELL/TRADE Monthly Sticky Reader" 
          :
          "Loading...";
    return (
        <header className="App-header">
          <BSTNav />
          <img src={logo} className="App-logo" alt="logo" />
          <a href={metaUrl} className="titleLink">
            <h1 className="App-title">
              {metaTitle}
            </h1>
          </a>
          {
              props.thread ?
                  (
                  <div>
                      <p>Thread created on {props.thread.Metadata.ThreadCreatedAt.toDateString()}</p>
                      <div>
                          <div className="BSTMeta">
                          Looking to Sell: {props.thread.Metadata.TotalSell}<br/>
                          Looking to Buy: {props.thread.Metadata.TotalBuy}<br/>
                          Looking to Trade: {props.thread.Metadata.TotalTrades}<br/>
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