const path = require('path');

module.exports ={
    devServer:{
        proxy:{
            '/api':{
                target:'http://localhost:51518'
            }
        }
    }
}