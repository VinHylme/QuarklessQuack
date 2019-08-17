<template>
<div class="container_dropzone">
 <section>
        <b-field>
            <b-upload :style="isHidden ? styleObject : '' " @input="onChange" v-model="dropFiles" :multiple="isMulti" :accept="acceptFile" drag-drop>
                <section class="section">
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
        isHidden:false
    },
   data() {
    return {
        dropFiles: [],
        styleObject:{
            margin: "0 auto",
            padding:0,
            width:"250px",
            height:"250px",
            position:"absolute",
            top:0,
            left:0,
            opacity:0,
        }
    }
  },
  methods: {
    uuidv4() 
    {
        return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
          (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
        )
    },
    onChange(e){
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
}
.body-container .dropStyle{
    margin-left:0 !important;
}
</style>
