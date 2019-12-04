<template>
    <transition name="zoom-out">
        <div class="v-cal-dialog" v-if="isActive">
          <div class="v-cal-dialog__bg" @click="cancel"></div>
              <div class="v-cal-dialog-card is-big">
                <form>
                  <header class="v-cal-dialog-card__header">
                        <h5 class="v-cal-dialog__title is-large">Posting at {{date}}</h5>
                        <button type="button" class="v-cal-dialog__close" @click="cancel"></button>
                  </header>
                  <div>
                    <section class="v-cal-dialog-card__body" >
                        <article class="media">
                          <figure class="media-left">
                            <p class="image is-64x64">
                               <img class="media_container isImage" id="image" v-bind:src="'data:image/jpeg;base64,' + medias[0]">
                            </p>
                          </figure>
                          <div class="media-content">
                            <div class="field">
                              <p class="control">
                                {{caption}}
                              </p>
                            </div>
                            <div class="field">
                              <p class="control">
                                <button class="button is-info">Post Now</button>
                              </p>
                            </div>
                          </div>
                        </article>
                    </section>
                  </div>
                </form>
            </div>       
          </div>
    </transition>
</template>

<script>
import moment from 'moment';

export default {
  props:{
    id:String,
    title: String,
    caption: String,
    hashtags: Array,
    credit: String,
    location: Object,
    medias:Array,
    startTime: Object,
    type: Number
  },
  data() {
      return {
          isActive: false,
          event: {},
          date:Date
      }
  },
  beforeMount(){
    document.body.appendChild(this.$el);
    this.date = moment(this.startTime).format("YYYY-MM-DD HH:mm:ss"); 
  },
  mounted() {
    this.isActive = true;
  },
  methods:{
    cancel() {
      this.close();
    }, 
    close() {
      this.isActive = false;
      // Timeout for the animation complete before destroying
      setTimeout(() => {
          this.$destroy();
          this.$el.remove();
      }, 150);
    }
  }
}
</script>

<style lang="scss">

</style>
