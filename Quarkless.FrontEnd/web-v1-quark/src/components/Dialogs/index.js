import Vue from 'vue'
import RightSideMenuPanel from './RightSideMenuPanel.vue';

function open(propsData) {
  const DialogComponent = Vue.extend(RightSideMenuPanel);
  return new DialogComponent({
      el: document.createElement('div'), propsData
  });
}

export default {
  View(data){
    let propData = {
      selectedAccount: data.selectedAccount,
      selectedItems: data.selectedItems
    }
    return open(propData)
  }
}