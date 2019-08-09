<template>
  <div class="card">
  <header class="card-header">
      <div class="media">
      <div class="media-center">
          <figure class="image is-96x96">
          <img class="is-rounded" v-if="profilePicUrl!==undefined" v-bind:src="profilePicUrl" alt="Placeholder image">
          <img class="is-rounded" v-else src="https://alumni.crg.eu/sites/default/files/default_images/default-picture_0_0.png" alt="default image">
          </figure>
      </div>
      <div class="media-content">
          <p class="title is-6">{{config.name}}</p>

      </div>
      <b-button class="is-clear">
        <b-icon pack="fas" icon="times-circle" type="is-warning" size="is-medium"></b-icon>
      </b-button>
  </div>

  </header>
  <div class="card-content">
      <p>{{config.description}}</p>
  </div>
  <footer class="card-footer">
      <p class="card-footer-item">
          <b-icon pack="fas" icon="edit"></b-icon>
      </p>
  </footer>
</div>
</template>

<script>
export default {
  props:{
    config:{}
  },
  data(){
    return{
      profilePicUrl:''
    }
  },
  mounted(){
    this.profilePicUrl = this.$store.getters.InstagramProfilePicture(this.config.instagramAccountId);
  }
}
</script>

<style lang="scss" scoped>
.is-clear{
  background:transparent;
  border:none;
}
.media{
  width: 100%;
  margin: 0 auto;
  float:none;
}
.media-center{
  margin-left: 38%;
  margin-top:-2em;
  left:0;
  right:0;  
  top:0;
  bottom:0;
  padding:0;
  img:hover{
    cursor: pointer;
  }
}
.card {
    margin-left:0.5em;
    margin-top:3em;
    width:400px !important;
    height:195px !important;
    background-color: #292929 !important;
    &.is-dark{
      color:white !important;
    }
    .card-content{
      p {
        color:#fefefe !important;
        padding:0em;
        font-size:16px;
        text-align: left;
        background-color: #292929 !important;
      }
   }
}
.select{
    width:190px;
}
.card-header{
    background-color: #414141;
    height:40px;
    width:100%;
    .card-header-title{
      color:white !important;
    }
}
.card-content{
    padding-top:2em;
    text-align: left;
    height:100px;
    width: 100%;
}
.card-footer{
    height:55px;
    border: none;
    font-weight: bold;
    background-color:#3b6dda;
    .card-footer-item{
        font-size: 20px;
        border: none;
        color:white;
        &:hover{
            color:bisque;
            cursor: pointer;
        } 
    }
}
select{
    background: #414141 !important;
    border:none !important;
    color:white!important;
    option {
        color: #f3f3f3 !important;
        background: #414141;
    }
}
</style>
