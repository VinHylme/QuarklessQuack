<template>
  <transition name="modal" style="margin:0 auto;">
      <b-modal :active.sync="isActive" :onCancel="canceled">
        <div class="modal-card is-verify-instagram">
                <header class="modal-card-head">
                    <p class="modal-card-title">Verify your Instagram Account</p>
                </header>
                <section v-if="type==='phone_number'" class="modal-card-body">
                    <b-field label="Enter a phone number to verify">
                        <b-input
                            type="tel"
                            v-model="phonenumber"
                            placeholder="Your phone number"
                            required>
                        </b-input>
                    </b-field>
                </section>
                <section v-else-if="type === 'sms_code' || type === 'email_code'" class="modal-card-body">
                    <b-field label="Verification Code">
                        <b-input
                            type="text"
                            v-model="code"
                            placeholder="Your Code"
                            required>
                        </b-input>
                    </b-field>
                    <b-message title="Verify - Confirmation Number" type="is-white" has-icon aria-close-label="Close message">
                        Please enter the verification code sent to you, sometimes Instagram detects your account as a spammer or for other security reasons, verifying your account will allow instagram to register your current location as safe
                    </b-message>
                </section>
                <section v-else-if="type === 'captcha'" class="modal-card-body">
                    <b-field label="Captcha was requested"></b-field>
                    <b-message title="Verify - Captcha" type="is-white" has-icon aria-close-label="Close message">
                        This usually happens if your account is new, please go on the instagram app or website and complete the captcha and any other validation steps then refresh login on the account section
                    </b-message>
                </section>
                <footer class="modal-card-foot">
                    <button v-if="type === 'sms_code' || type === 'email_code'" @click="SendVerifyCode" :class="isSendingVerifyCode?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                        <b-icon icon="badge"></b-icon>
                        <span>Send Verification Code</span>
                    </button>
                     <button v-if="type === 'phone_number'" @click="SendPhoneVerify" :class="isSendingVerifyCode?'button is-light is-rounded is-large is-loading' : 'button is-light is-rounded'" style="margin:0 auto;">
                        <b-icon icon="badge"></b-icon>
                        <span>Verify</span>
                    </button>                    
                </footer>
        </div>
      </b-modal>
  </transition>
</template>

<script>
import vue from 'vue';
export default {
    props:{
        details:Object
    },
    data(){
        return {
            isActive:true,
            isVerifying:false,
            code:'',
            phonenumber:'',
            type:''
        }
    },
    mounted(){
        if(this.details.challengeDetail!==undefined){
            this.type = this.details.challengeDetail.verify
        }
        vue.prototype.$toast.open({
                message: "We need to verify you are the right account holder, please verify with the code sent to you (either your email or phone)",
                type: 'is-info',
                position:'is-top',
                duration:25000
        });
    },
    methods:{
        canceled(){
            this.$emit("Finished")
            this.isActive = false;
        },
        SendPhoneVerify(){
            this.isVerifying = true;
            const instagramAccountId = this.details.instagramAccountId;
            if(!instagramAccountId){
                this.isVerifying = false;
                vue.prototype.$toast.open({
                    message: "Invalid Action, You have not selected an account to verify",
                    type: 'is-warning',
                    position:'is-top',
                    duration:25000
                });
                return;
            }
            const data = {
                phoneNumber: this.code,
                instagramAccountId: instagramAccountId
            }
            this.$store.dispatch('SubmitPhoneForChallange', data).then(result=>{
                vue.prototype.$toast.open({
                    message: "Verified!",
                    type: 'is-info',
                    position:'is-top',
                    duration:8000
                });
                this.isVerifying = false;
                this.canceled();
            }).catch(err=>{
                this.isVerifying = false;
                vue.prototype.$toast.open({
                    message: "Could not verify in this moment, please try again later",
                    type: 'is-danger',
                    position:'is-top',
                    duration:15000
                });
            })
            this.isVerifying = false;
        },
        SendVerifyCode(){
            this.isVerifying = true;
            const instagramAccountId = this.details.instagramAccountId;
            if(!instagramAccountId){
                this.isVerifying = false;
                vue.prototype.$toast.open({
                    message: "Invalid Action, You have not selected an account to verify",
                    type: 'is-warning',
                    position:'is-top',
                    duration:25000
                });
                return;
            }

            const data = {
                code: this.code,
                instagramAccountId: instagramAccountId
            }

            this.$store.dispatch('SubmitCodeForChallange', data).then(result=>{
                vue.prototype.$toast.open({
                    message: "Verified!",
                    type: 'is-info',
                    position:'is-top',
                    duration:8000
                });
                this.isVerifying = false;
                this.canceled();
            }).catch(err=>{
                this.isVerifying = false;
                vue.prototype.$toast.open({
                    message: "Could not verify in this moment, please try again later",
                    type: 'is-danger',
                    position:'is-top',
                    duration:15000
                });
            })
            this.isVerifying = false;
        }
    }
}
</script>

<style lang="scss">
@import '../../Style/darkTheme.scss';
.modal-content{
    width:auto !important;
}
.modal-card{
        &.is-verify-instagram{
                 background-color:$modal_background;
                 .modal-card-body{
                        padding-top:1em;
                        padding-left:2em;
                        padding-right:2em;
                        background:$modal_body;
                        color:$main_font_color;
                        label{
                                color:$main_font_color;
                                text-align: left;
                        }
                        .input{
                                color:$main_font_color !important;
                                &::placeholder{
                                        color:$main_font_color;
                                }
                                border:none;
                        }
                        .control-label{
                                &:hover{
                                        color:$wheat;
                                }
                        }
                        .dropdown-menu{
                                background:$backround_back;
                                .dropdown-item{
                                        color:$main_font_color;
                                        &:hover{
                                                background:#292929;
                                        }
                                }
                        }
                }
                .modal-card-foot{
                        background:$backround_back;
                        border:none;
                }
                .modal-card-head{
                        background:$backround_back;
                        border:none;
                        .modal-card-title{
                                color:$main_font_color;
                        }
                        
                }
        }
}
</style>