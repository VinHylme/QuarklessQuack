<template>
<div class="accounts_container">
      <InstaCard/>
</div>
</template>

<script>
import InstaCard from "../Objects/InstaAccountCard";
export default {
        name:"homePage",

        components:{
                InstaCard
        },
        mounted() {
                var res = this.$store.getters.GetInstagramAccounts("lemonkaces")
                 // eslint-disable-next-line no-console
                console.log(res);
                return res;
        }
}
</script>

<style lang="scss">
.accounts_container{
        padding:1em;
        display: flex;
        flex-flow: row wrap;
        align-items: center;

}
body {
        width: 100%;
        text-align: center;
}
</style>
