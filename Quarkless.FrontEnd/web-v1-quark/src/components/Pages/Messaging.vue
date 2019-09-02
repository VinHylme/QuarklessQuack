<template>
  <div class="messaging-container">
    
    <div class="columns is-gapless is-mobile container-column">
      <div class="column is-2">
        <div class="side-panel">
          <b-menu>
            <b-menu-list label="Conversations">
                <b-menu-item @click="currentlySelected = 0" icon="target" active expanded>
                    <template slot="label" slot-scope="props">
                        Targeted Users
                        <b-icon
                            class="is-pulled-right"
                            :icon="props.expanded ? 'menu-down' : 'menu-up'">
                        </b-icon>
                    </template>
                    <b-menu-item @click="currentlySelected = 2" icon="account" label="Followers"></b-menu-item>
                    <b-menu-item @click="currentlySelected = 3" icon-pack="fas" icon="map-marker-alt" label="Location"></b-menu-item>
                    <b-menu-item @click="currentlySelected = 4" icon-pack="fas" icon="hashtag" label="Topics"></b-menu-item>
                    <b-menu-item @click="currentlySelected = 5" icon-pack="fas" icon="smile" label="Suggested" disabled></b-menu-item>
                </b-menu-item>
                <b-menu-item @click="currentlySelected = 6" icon="magnify" label="Search">
                    <b-menu-item @click="currentlySelected = 7" label="Location"></b-menu-item>
                    <b-menu-item @click="currentlySelected = 8" label="Topic"></b-menu-item>
                </b-menu-item>
            </b-menu-list>
            <b-menu-list>
                 <b-menu-item @click="currentlySelected = 1" icon="inbox" label="Inbox"></b-menu-item>
                 <b-menu-item @click="currentlySelected = 9" icon="comment" label="Recent Comments">
                </b-menu-item>
            </b-menu-list>
        </b-menu>
        </div>
      </div>
      <div class="column is-10">
        <div class="main-panel">
          <div v-if="currentlySelected === 2">
            <h3 class="title">HELLO</h3>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  data(){
    return{
      currentlySelected:0
    }
  },
  methods:{
  }
}
</script>

<style lang="scss">
.messaging-container{
  margin:0 auto;
  width:100%;
  height:100vh;
  padding-left:5em;
  padding-top:.5em;
 // background:white;
}
.container-column{
  .main-panel{
    background:#222222;
    height:100vh;
  }
  .side-panel{   
    //border-radius: 1em;
    .menu{
      text-align: left;
      .menu-label{
        color:#d9d9d9;
      }

      .menu-list{
        .is-small{
          margin: 0 auto;
        }
        color:#d0d0d0;
        a{
          padding:0.5em 0.75em;
          color:#d9d9d9;
          &:hover{
            background:#232323;
          }
          &.is-active{
            background:#13b94d;
          }
        }
        ul{
          
          li{
            span{
              color:#d0d0d0;
            }
          }
        }
      }
      padding:0;
      margin:0 auto;
      padding:1em;
      color:#d9d9d9;
    }
    .subtitle{
      color:#d9d9d9;
    }
    background: #393939;
    height: 100vh;
  }
}
</style>