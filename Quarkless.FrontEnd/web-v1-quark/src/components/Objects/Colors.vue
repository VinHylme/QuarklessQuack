<template>
<div class="color_container">
  <div class="box">
    <div class="color_display" :style="colorPick" @click="openDialog"></div>
  </div>
</div>
</template>

<script>
export default {
 name:'color_card',
  props:{
    color:Object,
    id:Number
  },
  methods:{
    openDialog(){
      this.$emit("openColorDialog", this.id, this.color);
    }
  },
  computed:{
    colorPick(){
      return 'background:rgb('+this.color.red +','+ this.color.green + ',' + this.color.blue+')';
    }
  }
}
</script>

<style lang="scss" scoped>
.color_container{
  display: inline-flex;
  margin:0 auto;
}
.box{
  padding:0em;
  margin-left:0em;
  margin-top:1em;
  background:transparent !important;
  &:hover{
    cursor: pointer;
    background:#d1d1d1;
  }
}
.color_display{
  width:80px;
  height:80px;
  &:hover{
    opacity: 0.5;
  }
}
</style>
