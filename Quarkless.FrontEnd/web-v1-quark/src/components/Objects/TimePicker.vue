<template>
  <div class="time-picker-container">
    <b-field custom-class="is-light" :label="label">
        <b-clockpicker
            style=""
            v-model="timedate"
            :placeholder="when"
            icon="clock"
            type="is-danger"
            @input="changed"
            :hour-format="format">                 
        </b-clockpicker>
    </b-field>
  </div>
</template>

<script>
export default {
    props:{
        isAmPm:Boolean,
        label:String,
        time:Date,
    },
    data() {
        return {
            date:{}
        }
    },
    computed: {
      when(){
        return this.timedate.getHours() + ':' + this.timedate.getMinutes()
      },
      timedate:{
          get(){
              return this.time
          },
          set(value){
              this.time = value;
          }
      },
      format() {
        return this.isAmPm ? '12' : '24'
      }
    },
    methods:{
        changed(){
            this.$emit('input', this.time);
        }
    }
}
</script>

<style lang="scss">

</style>