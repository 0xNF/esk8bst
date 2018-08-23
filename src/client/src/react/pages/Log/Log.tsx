import * as React from 'react';
import './Log.css';
import logo from 'src/logo.svg';
import { BSTFooter } from 'src/react/components/BSTFooter/BSTFooter';
import { BSTNav } from 'src/react/components/BSTNav/BSTNav';

interface LogProps {
   
}

function Log(props: LogProps) {
    return (
        <div className="App App-body">
        <header className="App-header"> 
            <BSTNav />
            <img src={logo} className="App-logo" alt="logo" />
                <h1 className="App-title">
                    Release Notes
                </h1>
        </header>

        <div className="AboutZone">

            <h3> 8/23/2018 </h3>
            <ul>
                <li>Fixed a bug where malformed comments would break the parsing engine and cause nothing to render</li>
            </ul>
            

            <h3> 8/16/2018 </h3>
            <ul>
                <li>Added Email Notification. Click on the 'Get Notified of Updates' button the main page to get started.</li>
                <li>Added multi-select when filtering for boards</li>
            </ul>

            <h3> 8/11/2018 </h3>
            <ul>
                <li>Fixed page title</li>
                <li>Added Auto Fetch of the BST thread every 60 seconds</li>
            </ul> 

            <h3> 8/10/2018 </h3>
            Initial release of ESK8BST. Features include:
            <ul>
                <li>Automatically finding and scanning the monthly Buy/Sell/Trade thread for new posts</li>
                <li>Filtering for Company</li>
                <li>Sorting by Price/Seller/Date Posted and others</li>
            </ul>        
        </div>

        <BSTFooter />

      </div>
    );
}

export { Log, LogProps }