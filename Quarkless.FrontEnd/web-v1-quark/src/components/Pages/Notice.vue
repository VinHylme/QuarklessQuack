<template>
	<div>

	</div>
</template>

<script>
import Fingerprint2 from 'fingerprintjs2';
import {deviceDetect} from 'mobile-device-detect';
export default {
	mounted(){
		console.log(deviceDetect())
		if (window.requestIdleCallback) {
			requestIdleCallback(function () {
				Fingerprint2.get(function (components) {
					console.log(components) // an array of components: {key: ..., value: ...}
					var values = components.map(function (component) { return component.value })
					var murmur = Fingerprint2.x64hash128(values.join(''), 31)
					console.log(murmur)
				})
			});
			} else {
			setTimeout(function () {
				Fingerprint2.get(function (components) {
					console.log(components) // an array of components: {key: ..., value: ...}
				})  
			}, 500)
		}
		this.$router.push('/manage')
	}
}
</script>

<style lang="scss">

</style>