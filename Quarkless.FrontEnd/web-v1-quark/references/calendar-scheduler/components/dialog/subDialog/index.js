import Vue from 'vue';
import LibDialog from './LibaryDialog.vue';
import CaptionDialog from './SavedCaptionDialog.vue';
import HashtagDialog from './SavedHashtagsDialog.vue';
function open(propsData) {
  const EventDialogComponent = Vue.extend(LibDialog);
  return new EventDialogComponent({
      el: document.createElement('div'),
      propsData
  });
}
function openSavedCaptionDialog(propsData){
  const EventDialogComponent = Vue.extend(CaptionDialog);
  return new EventDialogComponent({
    el: document.createElement('div'),
    propsData
  })
}
function openSavedHashtagsDialog(propsData){
  const EventDialogComponent = Vue.extend(HashtagDialog);
  return new EventDialogComponent({
    el: document.createElement('div'),
    propsData
  })
}
export default{
  show(data) {
    const propsData = {
		title : 'Media Libary',
		profile: data.profile,
		type : data.type
    }
    return open(propsData);
  },
  showSavedCaption(data){
    return openSavedCaptionDialog(data);
  },
  showSavedHashtags(data){
    return openSavedHashtagsDialog(data);
  }
}