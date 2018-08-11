import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { AppRouter } from 'src/react/router/Router';
import './index.css';
import registerServiceWorker from './registerServiceWorker';
import { RegisterFirebase } from 'src/services/Firestore';

ReactDOM.render(
  <AppRouter />,
  document.getElementById('root') as HTMLElement
);

RegisterFirebase();
registerServiceWorker();
