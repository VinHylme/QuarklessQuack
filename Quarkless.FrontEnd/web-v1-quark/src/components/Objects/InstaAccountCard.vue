<template>
 <router-link :to="'/view/'+id">
    <div class="card">
        <div class="card-content">
            <div class="media">
            <div class="media-left">
                <figure class="image is-96x96">
                <img src="https://upload-assets.vice.com/files/2016/04/09/1460231001Alchemy_Resonance_Archan_Nair.jpg" alt="Placeholder image">
                </figure>
            </div>
            <div class="media-content">
                <p class="title is-4">{{name}}</p>
                <p class="subtitle is-6">@{{username}}</p>
            </div>
            </div>
            <div class="content">
            {{biography}}
        </div>
    </div>
      <footer class="card-footer">
    </footer> 
    </div>
 </router-link>
</template>

<script>
export default {
name:"InstaAccountCard",
props: {
    id: Number,
    name:String,
    username: String,
    biography: String,
    profilePicture: String,
  },
}
</script>

<style>
.card{
    margin:0.2em;
    width:330px !important;
}
</style>
