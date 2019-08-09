<template>
  <section>
    <b-pagination
      @change="onChange"
      :total="total"
      :current.sync="currentPageAt"
      :range-before="rangeBefore"
      :range-after="rangeAfter"
      order="is-centered"
      size="is-medium"
      :simple="false"
      :rounded="isRounded"
      :per-page="perPage"
      aria-next-label="Next page"
      aria-previous-label="Previous page"
      aria-page-label="Page"
      aria-current-label="Current page">
    </b-pagination>
  </section>
</template>

<script>
 export default {
  props:{
    perPage:Number,
    total:Number,
    isRounded:Boolean,
    rangeBefore:Number,
    rangeAfter:Number,
    currentPage:Number
  },
  mounted(){
  },
  methods:{
    onChange(e)
    {
      this.$emit('pageUpdate',e);
    }
  },
  data() {
      return {
        currentPageAt:this.currentPage
      }
    }
  }
</script>

<style lang="scss">
.pagination-previous, .pagination-next, .pagination-link{
  color:#d9d9d9 !important;
}

</style>
