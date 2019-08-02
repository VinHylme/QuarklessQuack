<template>
  <div></div>
</template>

<script>
import moment from 'moment'

export default {
  data(){
    return{
        today: moment(),
        dateContext: moment(),
        days: ['S', 'M', 'T', 'W', 'T', 'F', 'S']
    }
},
}
</script>

<style>

</style>
