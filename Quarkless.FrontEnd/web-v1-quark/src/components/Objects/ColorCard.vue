<template>
<div class="card">
  <div class="card-header">
      <b-select v-model="nameChange" :placeholder="name==undefined?'Add a new color':name" icon="brush">
        <option v-for="(color,index) in colorsAllowed" :key="index" :value="color">
          {{color}}
        </option>
      </b-select>
  </div>
  <div class="card-content">
    <div class="content">
      <chrome-picker style=" width:100%; height:150px; box-shadow:none; border:none; border-radius:0; background:#212121;" v-model="colorChange"/>
    </div>
  </div>
  <footer class="card-footer">
      <a v-if="name!==undefined" @click="updateColor" class="card-footer-item">
        <b-icon pack="fas" icon="check" type="is-light" size="is-default"></b-icon>
      </a>
      <a v-else @click="addColor" class="card-footer-item">
        <b-icon pack="fas" icon="check" type="is-light" size="is-default"></b-icon>
      </a>
      <a v-if="name!==undefined" @click="deleteColor" class="card-footer-item">
        <b-icon pack="fas" icon="trash" type="is-light" size="is-default"></b-icon>
      </a>
  </footer>
</div>
</template>

<script>
import { Chrome } from 'vue-color'
export default {
  name:'color_card',
  props:{
    id:Number,
    color:Object,
    name:String,
    colorsAllowed:Array
  },
  data(){
    return {
      colorChange:{},
      nameChange:null
    }
  },
  components:{
    'chrome-picker': Chrome
  },
  mounted(){
    this.colorChange = this.color;
  },
  methods:{
    updateColor(){
      if(this.colorChange.rgba!==undefined){
        const updateRequest = {name:this.nameChange===null ? this.name : this.nameChange, red: this.colorChange.rgba.r, green: this.colorChange.rgba.g, blue: this.colorChange.rgba.b, alpha: this.colorChange.rgba.a}
        this.$emit('updateColor', this.id, updateRequest);
      }
      else if(this.nameChange!==null){
        const updateRequest = {name:this.nameChange, red: this.color.r, green: this.color.g, blue: this.color.b, alpha: 255}
        this.$emit('updateColor', this.id, updateRequest);
      }
      this.$emit('updateColor', undefined);
    },
    deleteColor(){
      this.$emit('deleteColor',this.id)
    },
    addColor(){
       if(this.colorChange.rgba!==undefined){
        if(this.name===undefined){
          this.name = 'any'
        }
        const addRequest = {name:this.nameChange===null ? 'any' : this.nameChange, red: this.colorChange.rgba.r, green: this.colorChange.rgba.g, blue: this.colorChange.rgba.b, alpha: this.colorChange.rgba.a}
        this.$emit('addColor', addRequest)
      }
      this.$emit('addColor', undefined);
    }
  }
}
</script>

<style lang="scss" scoped>
.card{
  box-shadow: none;
  margin:0 auto;
  width:450px;
  background:#121212;
}
select{
  border:none;
  border-radius: 0;
  &:focus{
    box-shadow: none;
    background:transparent;
    border:0;
  }
}
option{
  color:white;
}
.card-content{
  padding:0;
  height:400px !important;
}
.card-footer{
  height:80px !important;
  border:none;
  .card-footer-item{
    border:none;
  }
}

</style>
