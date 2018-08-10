import * as React from 'react';
import { 
    Route, 
     Switch,
     BrowserRouter
     } from 'react-router-dom';
import { Home } from 'src/react/pages/Home/Home';
import { About } from 'src/react/pages/About/About';
import { Log } from 'src/react/pages/Log/Log';

interface AppRouterProps {

}

function AppRouter(props: AppRouterProps) {
    return (
        <BrowserRouter>
            <div className="container-fluid">
                <Switch>
                    <Route path="/about" component={About}/> 
                    <Route path="/log" component={Log}/>
                    <Route path="/" component={Home}/>
                </Switch>
            </div>
        </BrowserRouter>
    );
}

export { AppRouter, AppRouterProps }
