import React from 'react';
import { 
  BrowserRouter as Router,
  Route } from 'react-router-dom';
import Layout from './components/Layout';
import Home from './components/Home';
import Orders from './components/Orders';
import './App.css';
import OrderStates from './components/OrderStates';
import Upload from './components/Upload';

function App() {
  return (
    <Router>
    <Layout>
      <Route exact path='/' component={Home} />                  
      <Route  exact path='/orders' component={Orders} />
      <Route  exact path='/orders/states' component={OrderStates} />
      <Route  exact path='/upload' component={Upload} />
    </Layout>
    </Router>
  );
}

export default App;
