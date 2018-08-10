import * as React from 'react';
import logo from 'src/logo.svg';
import 'src/react/pages/Home/App.css';
import './About.css';
import { BSTFooter } from 'src/react/components/BSTFooter/BSTFooter';
import { VersionInfo } from 'src/models/version';
import { BSTNav } from 'src/react/components/BSTNav/BSTNav';


interface AboutProps {
    version: VersionInfo;
}

function About(props: AboutProps) {    
    return (
        <div className="App App-body">
            <header className="App-header"> 
                <BSTNav />
                <img src={logo} className="App-logo" alt="logo" />
                    <h1 className="App-title">
                        About
                    </h1>
            </header>

            <div className="AboutZone">
                <h3> What is this? </h3>
                <p> This site scans the monthly /r/ElectricSkateboard Buy/Sell/Trade thread and parses the entries there into a more digestable, sortable, and filterable format.</p>

                <h3>Why does this site exits?</h3>
                <p>
                    I wanted an easy way to filter for Boosted Boards that wasn't just ctrl-f'ing the thread every day.<br/>
                    Noise from comments, or items that have been [SOLD], or poorly formated posts are not included in this list.
                </p>
                
                <h3>How does it work?</h3>
                <p>
                    The page uses your browser to query reddit for a JSON file that contains the thread information.
                    <br/>
                    That information is then parsed according to some rough english-language hueristics to try to figure out what everyone is selling at what price.
                    <br/>
                    For more information, please check out the <a className="inlineLink" target="_blank" href="https://github.com/0xnf/esk8bst">GitHub repo</a>
                </p>

                <h3>How can you improve it?</h3>
                <ul>
                    <li>
                        <p>
                            Thread Maintainer? Try to keep the thread title something resembling "BUY/SELL/TRADE Monthly". This site does its best to figure out what the current thread is, but it isn't perfect.
                        </p>
                    </li>
                    <li>
                        <p>
                            Regular User? Stick to labeling your posts with [BUY] [SELL] or [TRADE]. The scanner is somewhat lenient with how it determines something is for sale or not, but it's quite tricky to get this right. Also, don't post more than one item at once. If you have multiple items and you want them to end up here, please make multiple posts.
                        </p>
                    </li>
                    <li>
                        <p>
                            Developer? Check out the <a className="inlineLink" target="_blank" href="https://github.com/0xnf/esk8bst">GitHub repo</a>
                        </p>
                    </li>
                </ul>

                <h3>Known Bugs</h3>
                <ul>
                    <li>
                        <p> Certain formattings of Canadian Dollar postings confuse the parser</p>
                    </li>
                    <li>
                        <p>Only a single [Sell] [Buy] or [Trade] tag can be present per post</p>
                    </li>
                    <li>
                        <p>If you have Tracking Protection enabled, your browser will refuse to download from Reddit. Blame Reddit for this. This site embeds no trackers and respects your privacy.</p>
                    </li>
                    <li>
                        <p>This site uses an API that is being discontinued by Reddit, and may require a substantial rewrite in the future.</p>
                    </li>
                    <li>
                        <p>Obvious brands like OneWheel are grouped into the unknown category.</p>
                    </li>
                    <li>
                        <p>Posts about items that aren't boards confuse the parser. Batteries, helmets, peripherals, etc.</p>
                    </li>                        
                </ul>
            </div>

            <BSTFooter />
  
          </div>

    );
}

export { About, AboutProps }