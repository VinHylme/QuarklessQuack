<template>
    <div class="container-wide">
        <h1 class="title is-static is-0"> 404 </h1>
        <h1 class="title is-static is-1">This page does not exist</h1>
        <img class="not-found-asset" src="../../../assets/notfound.png" alt="not found image">
    </div>
</template>

<script>
export default {

}
</script>

<style lang="scss">
.container-wide{
    overflow: hidden;
    padding:0;
    margin:0 auto;
    width: 100vw;
    height: 100vh;
}
.title{
    &.is-0{
        font-size: 20rem;
    }
    &.is-static{
       position: absolute;
        float:right;
        right:0;
        color:white;
    }
}
.not-found-asset{
    height: 100vh;
    width:45vw;
}
</style>
