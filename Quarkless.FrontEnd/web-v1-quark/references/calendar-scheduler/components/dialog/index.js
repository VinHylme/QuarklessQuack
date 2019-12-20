import Vue from 'vue'
import EventDialog from './EventDialog.vue';
import EventViewDialog from './EventViewDialog.vue';
import MiniViewDialog from './MiniViewDialog.vue';

function open(propsData) {
    const EventDialogComponent = Vue.extend(EventDialog);
    return new EventDialogComponent({
        el: document.createElement('div'),
        propsData
    });
}
function viewDialog(propsData){
    const ViewDialog = Vue.extend(EventViewDialog);
    return new ViewDialog({
        el: document.createElement('div'), 
        propsData
    })
}
function miniViewDialog(propsData){
    const miniView = Vue.extend(MiniViewDialog);
    return new miniView({
        el: document.createElement('div'), propsData
    })
}
export default {
    hoverView(params){
        const bodyResp = JSON.parse(params.actionObject.body);

        const caption = bodyResp.MediaInfo.Caption;
        const hashtags = bodyResp.MediaInfo.Hashtags;
        const credit = bodyResp.MediaInfo.Credit;
        const location = bodyResp.Location;

        const type = params.actionObject.actionType.toLowerCase().includes('image') ? 0 
        : params.actionObject.actionType.toLowerCase().includes('video') ? 1 
        : params.actionObject.actionType.toLowerCase().includes('carousel') ? 2 : 404;

        var medias = [];
        switch(type){
            case 0: 
                medias.push(bodyResp.Image.ImageBytes);
                break;
            case 1:
                medias.push(bodyResp.Video.Video.VideoBytes);
                break;
            case 2:
                for(var i = 0; i < bodyResp.Album.length; i++){
                    if(bodyResp.Album[i].ImageToUpload.ImageBytes!==undefined){
                        medias.push(bodyResp.Album[i].ImageToUpload.ImageBytes);
                    }
                }
                break;
        }
        const propData = {
            id:params.id,
            title: params.actionObject.actionName,
            caption:caption,
            hashtags:hashtags,
            credit:credit,
            location:location,
            medias:medias,
            startTime: params.startTime,
            type: type
        }
        return miniViewDialog(propData);
    },
    view(params){
        const bodyResp = JSON.parse(params.actionObject.body);

        const caption = bodyResp.MediaInfo.Caption;
        const hashtags = bodyResp.MediaInfo.Hashtags;
        const credit = bodyResp.MediaInfo.Credit;
        const location = bodyResp.Location;
        const type = params.actionObject.actionType.toLowerCase().includes('image') ? 1 
        : params.actionObject.actionType.toLowerCase().includes('video') ? 2 
        : params.actionObject.actionType.toLowerCase().includes('carousel') ? 3 : 404;
        var medias = [];
        switch(type){
            case 1: 
                medias.push({type:1,url:bodyResp.Image.Uri});
                break;
            case 2:
                medias.push({type:1, url:bodyResp.Video.Video.Uri});
                break;
            case 3:
                for(var i = 0; i < bodyResp.Album.length; i++){
                    if(bodyResp.Album[i].ImageToUpload!==undefined && bodyResp.Album[i].ImageToUpload!==null){
                        medias.push({type:1, url:bodyResp.Album[i].ImageToUpload.Uri});
					}
					else if(bodyResp.Album[i].VideoToUpload!==undefined && bodyResp.Album[i].VideoToUpload!== null ){
                        medias.push({type:2, url:bodyResp.Album[i].VideoToUpload.Video.Uri});
                    }
                }
                break;
        }
        const propData = {
            id:params.id,
            title: params.actionObject.actionName,
            caption:caption,
            hashtags:hashtags,
            credit:credit,
            location:location,
            medias:medias,
            mediaTopic:bodyResp.mediaTopic,
            startTime: params.startTime,
            type: type
        }
        return viewDialog(propData);
    },
    show(params, extraFields) {
        const defaultParam = {
            title: 'Create Post',
            inputClass: null,
            overrideInputClass: false,
            createButtonLabel: 'Create',
            //  -------------------------
            date: null,
            startTime: null,
            endTime: null,
            enableTimeInputs: true,
        };

        const propsData = Object.assign(defaultParam, params);

        const defaultFields = [
            {
                name: 'date',   //  Required
                type: 'date',   //  def: 'text'
                label: 'Date',  //  def: this.name
                // showLabel: false,    //  def: true
                required: true, //  def: false,
                value: propsData.date,   //  def: null
            }
        ];

        if ( propsData.enableTimeInputs )
            defaultFields.splice(1, 0, {
                label: 'Times',
                fields: [
                    {
                        name: 'startTime',
                        type: 'time',
                        label: 'Start Time',
                        required: true,
                        value: propsData.startTime
                    },
                    {
                        name: 'endTime',
                        type: 'time',
                        label: 'End Time',
                        required: true,
                        value: propsData.endTime
                    }
                ]
            });

        propsData.fields = extraFields ? defaultFields.concat(extraFields) : defaultFields;
        return open(propsData);
    }
}