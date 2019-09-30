export default {
  install(Vue) {
    const { EventBusing } = require('../EventBusing')
    Vue.prototype.$bus = EventBusing
  }
}