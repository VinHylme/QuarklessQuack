import axios from 'axios';
import Fingerprint2 from 'fingerprintjs2';

function GetUserBrowser(){
    return new Promise((resolve)=>{
        if (window.requestIdleCallback) {
            requestIdleCallback(function () {
                Fingerprint2.get(function (components) {
                  var cmp = components.map(function (component) { return component })
                  var hashId = Fingerprint2.x64hash128(cmp.join(''), 31)
                  resolve({
                      uniqueId:hashId,
                      components: cmp
                  })
                })
            });
          } else {
            setTimeout(function () {
                Fingerprint2.get(function (components) {
                    var cmp = components.map(function (component) { return component })
                    var hashId = Fingerprint2.x64hash128(cmp.join(''), 31)
                    resolve({
                        uniqueId:hashId,
                        components: cmp
                    })
                })  
            }, 500)
        }
    })
}

function GetUserLocation(){
    return new Promise((resolve,reject)=>{
        let locale = "";
        if(navigator.geolocation){
            navigator.geolocation.getCurrentPosition(function(e){
             const latitude  = e.coords.latitude;
             const longitude = e.coords.longitude;
             locale =  latitude + ',' + longitude
             resolve(locale)
            }, function(){
                reject("failed")
            }); 
         }
    })
}
function GetUserIpAddress(){
    const base_url = "https://extreme-ip-lookup.com";
    let _axios = axios;
    _axios.defaults.baseURL = base_url;
    return new Promise((resolve, reject)=>{
        _axios.get('/json', {
            headers:{
                'Accept': 'application/json',
                'Content-Type': 'application/json',
                'Access-Control-Allow-Origin': '*' 
            }
        }).then(resp=>{
            resolve(resp)
        }).catch(err=>{
            reject(err)
        })
    })
}

function GetUserDetails(){
    return Promise.all([GetUserBrowser(), GetUserIpAddress(), GetUserLocation()])
    .then(value=>{
        let response = {
            _id:'',
            ipDetails:{
                ipAddress:'',
                ipType:'',
                ispName:'',
                ipName:'',
                org:''
            },
            geoLocationDetails:{
                city:'',
                continent:'',
                country:'',
                countryCode:'',
                region:'',
                regionOtherNames:'',
                location:{
                    latitude:0.0,
                    longitude:0.0
                }
            },
            deviceDetails:{
                uniqueId:'',
                userAgent:'',
                webDriver:'',
                language:'',
                colourDepth:0,
                deviceMemory:0,
                hardwareConcurrency:0,
                screenResolution:{
                    width:0,
                    height:0
                },
                availableScreenResolution:{
                    width:0,
                    height:0
                },
                timezoneOffset:0,
                timezone:'',
                sessionStorage:false,
                localStorage:false,
                indexedDatabase:false,
                addBehavior:false,
                openDatabase:false,
                cpuClass:'',
                platform:'',
                plugins:[],
                canvas:[],
                webGl:[],
                webGlVendor:'',
                adBlock:false,
                hasLiedLanguages:false,
                hasLiedResolution:false,
                hasLiedOs:false,
                hasLiedBrowser:false,
                touchSupport:false,
                fonts:[],
                audio:0.0
            }
        }
        //#region Map
        const comp = value[0].components;
        response.deviceDetails.uniqueId = value[0].uniqueId
        response.deviceDetails.userAgent = comp[0].value
        response.deviceDetails.webDriver = comp[1].value
        response.deviceDetails.language = comp[2].value
        response.deviceDetails.colourDepth = comp[3].value
        response.deviceDetails.deviceMemory = comp[4].value
        response.deviceDetails.hardwareConcurrency = comp[5].value
        const screenResultion = comp[6].value;
        if(screenResultion){
            response.deviceDetails.screenResolution.width = screenResultion[0]
            response.deviceDetails.screenResolution.height = screenResultion[1]
        }
        const availableScreenResolution = comp[7].value
        if(availableScreenResolution){
            response.deviceDetails.availableScreenResolution.width = availableScreenResolution[0]
            response.deviceDetails.availableScreenResolution.height = availableScreenResolution[1]
        }
        response.deviceDetails.timezoneOffset = comp[8].value
        response.deviceDetails.timezone = comp[9].value
        response.deviceDetails.sessionStorage = comp[10].value
        response.deviceDetails.localStorage = comp[11].value
        response.deviceDetails.indexedDatabase = comp[12].value
        response.deviceDetails.addBehavior = comp[13].value
        response.deviceDetails.openDatabase = comp[14].value
        response.deviceDetails.cpuClass = comp[15].value
        response.deviceDetails.platform = comp[16].value
        response.deviceDetails.canvas = comp[18].value
        response.deviceDetails.webGl = comp[19].value
        response.deviceDetails.webGlVendor = comp[20].value
        response.deviceDetails.adBlock = comp[21].value
        response.deviceDetails.hasLiedLanguages = comp[22].value
        response.deviceDetails.hasLiedResolution = comp[23].value
        response.deviceDetails.hasLiedOs = comp[24].value
        response.deviceDetails.hasLiedBrowser = comp[25].value
        const touchSupport = comp[26].value
        if(touchSupport && touchSupport.length > 0)
            response.deviceDetails.touchSupport = true
        
        if(comp[27] && comp[27].length > 0)
            response.deviceDetails.fonts = comp[27].value

        //response.deviceDetails.audio = comp[28].value
        //#endregion
        
        const data = value[1].data;
        if(data && data.query){
            response.ipDetails.ipAddress = data.query;
            response.ipDetails.ipType = data.ipType;
            response.ipDetails.ipName = data.ipName;
            response.ipDetails.ispName = data.isp;
            response.ipDetails.org = data.org;
            response.geoLocationDetails.city = data.city;
            response.geoLocationDetails.continent = data.continent;
            response.geoLocationDetails.country = data.country;
            response.geoLocationDetails.countryCode = data.countryCode;
            response.geoLocationDetails.region = data.region;
        }
        const locationParse = value[2].split(',');
        const lat = parseFloat(locationParse[0]);
        const lon = parseFloat(locationParse[1]);
        response.geoLocationDetails.location.latitude = lat;
        response.geoLocationDetails.location.longitude = lon;
        return response;
    }).catch(err=>{
        console.log(err);
    })
}

const _GetUserDetails = GetUserDetails
export {_GetUserDetails as GetUserDetails}

const _GetUserIpAddress = GetUserIpAddress;
export { _GetUserIpAddress as GetUserIpAddress }
const _GetUserLocation = GetUserLocation;
export { _GetUserLocation as GetUserLocation }
const _GetUserBrowser = GetUserBrowser;
export {_GetUserBrowser as GetUserBrowser};
