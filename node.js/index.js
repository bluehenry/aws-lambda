// const AWS = require('aws-sdk');
// const docClient = new AWS.DynamoDB.DocumentClient({ region: 'us-east-1'});

let dataStore;

exports.handler = async (event) => {
    // Deal with URL Resrouce: service 
    if (event.context['resource-path'] === '/service') {
        if (event.context['http-method'] === 'DELETE') {
            dataStore = undefined;
            console.log('DELETE');
        } else if (event.context['http-method'] === 'GET') {
            console.log('GET');
            dataStore = event.params.querystring.serviceID
        } else if (event.context['http-methdo'] === 'POST') {
            console.log('POST');
            dataStore = event['body-json']
        } else if (event.context['http-methdo'] === 'PUT') {
            dataStore = event['body-json']
            console.log('POST');
        } 
    } 
    // Deal with anohter URL Resrouce: users 
    else if (event.context['resource-path'] === '/users') {
        if (event.context['http-method'] === 'DELETE') {
            dataStore = undefined;
            console.log('DELETE');
        } else if (event.context['http-method'] === 'GET') {
            console.log('GET');
            dataStore = event.params.querystring.serviceID
        } else if (event.context['http-methdo'] === 'POST') {
            console.log('POST');
            dataStore = event['body-json']
        } else if (event.context['http-methdo'] === 'PUT') {
            dataStore = event['body-json']
            console.log('POST');
        }
    }
    
    return {event, dataStore};
};