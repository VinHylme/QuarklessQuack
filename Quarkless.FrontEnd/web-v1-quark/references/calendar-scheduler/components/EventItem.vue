<template>
    <div ref="event_block" class="v-cal-event-item"
         :title="'Post created for ' + formatEvent(event.startTime)"
         :class="eventClasses"
         @click.stop="eventClicked"
         @mouseover="eventOver"
         :style="eventStyles">
        <span class="v-cal-event-time">{{ fromNowFormat(event.startTime) }}</span>
        <!-- <span class="v-cal-event-name">{{ event.displayText }}</span> -->
		<img class="v-cal-event-image" :src="(displayUrl)" alt="image" title="image">
    </div>
</template>

<script>
    import moment from 'moment';
    import { EventBus } from './EventBus';

    export default {
        name: "EventItem",
        props: {
            event: {
                type: Object,
                required: true
            },
            use12: {
                type: Boolean,
                required: true
            },
            hasDynamicSize: {
                type: Boolean,
                default: true
            }
        },
        data() {
            return {
                ancestorHeight: 0
            }
        },
        mounted() {
            if ( this.hasDynamicSize ) {
                this.getAndSetAncestorHeight();
                window.addEventListener('resize', this.getAndSetAncestorHeight);
            }
        },
        beforeDestroy() {
            if ( this.hasDynamicSize )
                window.removeEventListener('resize', this.getAndSetAncestorHeight);
        },
        methods: {
			fromNowFormat(timestamp){
				return moment(timestamp).fromNow();
			},
			formatEvent(timestamp){
				return moment(timestamp).format('llll')
			},
            eventOver(){
                EventBus.$emit('event-hover',this.event);
            },
            eventClicked() {
                EventBus.$emit('event-clicked', this.event);
            },
            getAndSetAncestorHeight() {
                this.ancestorHeight = this.findAncestor(this.$refs.event_block, 'v-cal-day__hour-content').offsetHeight;
            },
            findAncestor (el, cls) {
                while ((el = el.parentElement) && !el.classList.contains(cls)) ;
                return el;
            }
        },
        computed: {
            displayUrl(){
                const item = this.event.actionObject;
                if(item === undefined || item == null)
                    return ''
                else
                {
                    let url = '';
                    switch(item.body.MediaInfo.MediaType){
                        case 1:
                            url = item.body.Image.Uri
                            break;
                        case 2:
                            url = item.body.Video.VideoThumbnail.Uri
                            break;
                        case 8: 
                            url = item.body.Album[0].ImageToUpload.Uri
                            break
                    }
                    return url;
                }
            },
            displayHeight() {

                const end = this.event.endTime.hours() > 0 ? moment(this.event.endTime) : moment(this.event.endTime).add(1, 'days');

                const hours = end.diff(this.event.startTime, 'hours', true);
                const bordersOffset = hours - 1;
                return  ( hours * this.ancestorHeight ) + bordersOffset;
            },
            eventStyles() {

                let styles = [];
                styles.push({
                    'backgroundColor': this.event.color,
                    'color': this.event.color,
                });

                if ( this.hasDynamicSize ) {
                    styles.push({
                        'height': this.displayHeight + 'px',
                    });

                    if ( this.event.overlaps > 0 ) {
                        const width = 100 / ( this.event.overlaps + 1 );
                        styles.push({
                            'width': width + '%',
                            'left': width + '%'
                        });
                    }

                    if ( this.event.startTime.minutes() > 0 ) {
                        const distance = ( this.ancestorHeight / 60 ) * this.event.startTime.minutes();
                        styles.push({
                            'top': distance + 'px'
                        });
                    }
                }

                return styles;
            },
            eventClasses() {
                return {
                    'is-overlapping': this.event.overlaps > 0
                }
            }
        },
        filters: {
            formatEventTime(hour, use12 = false) {
                if ( !hour )
                    return '';

                if ( use12 )
                    return hour.format( hour.minutes() > 0 ? 'h.mma' : 'ha' ).slice(0, -1);

                return hour.format( hour.minutes() > 0 ? 'HH.mm' : 'HH' );
            }
        },
    }
</script>

<style scoped>

</style>