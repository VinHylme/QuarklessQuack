<template>
<div class="container is-confirm">
  <div class="box is-dark">
			<h1 class="title" style="color:white;">Confirm Your Account</h1>
			<br>
			<b-input size="is-large" v-model="confirmationCode" pattern="[0-9]+" type="text" placeholder="Confirmation Code" minlength="6" required></b-input>
				<br>
			<b-field position="is-centered">
				<p class="control">
					<button @click="Confirm()" type="submit" class="button is-medium is-info">
						Confirm
					</button>
				</p>
				<p class="control">
					<button @click="ResendConfirm()" type="submit" class="button is-medium is-white">
						Resend Confirmation Code
					</button>
				</p>
			</b-field>
		</div>
</div>
</template>

<script>
import vue from 'vue'
export default {
    props:{
        username:String,
        reload:Boolean
    },
    data(){
        return {
            user:this.username,
            confirmationCode:''
        }
    },
    methods:{
		Confirm(){
			const data = {
				username:this.user,
				confirmationCode: this.confirmationCode
			}
			this.$store.dispatch('confirmAccount', data).then(resp=>{
                if(!this.reload){
                    this.$router.push({name:'login'})
                }
                else{
                    window.location.reload();
                }
			}).catch(err=>{
				vue.prototype.$toast.open({
					message: "The verification code entered is incorrect",
					type: 'is-danger',
					position:'is-top'
				})
				return;
			})
		},
		ResendConfirm(){
			this.$store.dispatch('resendConfirmation', this.user).then(res=>{
				vue.prototype.$toast.open({
					message: "Verification code sent again!",
					type: 'is-success',
					position:'is-top'
				})
				return;
			}).catch(err=>{
				vue.prototype.$toast.open({
					message: "Could not resend... please try again later",
					type: 'is-danger',
					position:'is-top'
				})
			})
		},
    }
}
</script>

<style lang=scss>

</style>