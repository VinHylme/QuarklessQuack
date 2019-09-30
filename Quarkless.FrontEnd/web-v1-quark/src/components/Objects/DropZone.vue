<template>
<div class="container_dropzone">
 <section>
        <b-field>
            <b-upload :style="isHidden ? [styleObject, styleSize] : [styleSize] " @input="onChange" v-model="dropFiles" :multiple="isMulti" :accept="acceptFile" @drop="droped" drag-drop>
                <section v-if="!isHidden" class="section">
                    <div class="content has-text-centered">
                        <p>
                            <b-icon
                                icon="upload"
                                size="is-large">
                            </b-icon>
                        </p>
                        <p>Drop your files here or click to upload</p>
                    </div>
                </section>
                <section v-else style="width:100%; padding:2em;"></section>
            </b-upload>
        </b-field>

        <div class="tags">
            <span v-for="(file, index) in dropFiles"
                :key="index"
                class="tag is-primary" >
                {{file.name}}
                <button class="delete is-small"
                    type="button"
                    @click="deleteDropFile(index)">
                </button>
            </span>
        </div>
    </section>
</div>
</template>

<script>
export default {
    props:{
        isMulti:Boolean,
        acceptFile:String,
        isHidden:Boolean,
        swidth:String,
        sheight:String
    },
   data() {
    return {
        width:'250px',
        height:'250px',
        dropFiles: [],
        styleSize:{
            width:this.width + '!important',
            height:this.height +'!important',
        },
        styleObject:{
            margin: "0 auto",
            padding:0,
            position:"absolute",
            top:0,
            left:0,
            right:0,
            bottom:0,
            opacity:0,
        }
    }
  },
  created(){
      if(this.swidth !== undefined||this.swidth !== null){
          this.width = this.swidth;
          this.height = this.sheight;
      }
  },
  methods: {
    droped(e){
    },
    onChange(e){
        if(e!==null){
            let formData = new FormData();
            for(var i = 0; i < this.dropFiles.length; i++){
                formData.append('file_'+ this.dropFiles[i].name.replace('.',"_"), this.dropFiles[i]);
            }
            this.$emit('readyUpload', 
            {
                formData: formData, 
                requestData: this.dropFiles
            });
            this.dropFiles = [];
        }
        },
      deleteDropFile(index) {
          this.dropFiles.splice(index, 1)
      }
  }
}
</script>

<style>
.container_dropzone{
    width:100%;
    margin-left:0;
    text-align: center;
}
.body-container .dropStyle{
    margin-left:0 !important;
}
</style>
