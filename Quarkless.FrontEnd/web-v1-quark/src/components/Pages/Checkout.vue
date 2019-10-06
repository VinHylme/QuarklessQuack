<template>
	<div>
		
	</div>
</template>

<script>
export default {
	mounted(){
		var stripe = Stripe('pk_test_HUG9HPrcnZ33VxkT89jmH2g600SFty01Vn');
		this.$store.dispatch('CreateSession',{
			ptype:1,
			curr:20,
			sauce:'',
			accid:this.$store.getters.User
		}).then(res=>{
			stripe.redirectToCheckout({
				sessionId: res.data.id
				}).then(function (result) {
			});
		}).catch(err=>{
			console.log(err)
		});
	}
}
</script>

<style lang="scss">
	
</style>