import Api from './Api';
import { Calling } from './constants';
export default {
  GetProfileConfig(){
    return Api(true).get(Calling["query_config"]);
  },
  GooglePlaceSearch(query){
    return Api(true).get(Calling["query_place_search"]+query);
  },
  GooglePlacesAutocomplete(query, radius){
    return Api(true).get(Calling["query_place_autocomplete"]+query+'/'+radius);
  },
  SimilarImageSearch(urls, limit, offset, moreAccurate){
    return Api(true).put(Calling["query_similar_image_search"]+ limit +'/' + offset + "/" + moreAccurate, urls);
  },
  ReleatedTopic(instaAccount, topic){
    return Api(true, instaAccount).get(Calling["query_related_topics"]+topic);
  },
  BuildTags(topic, subcat, lang, limit, pickRate){
    return Api(true).get(Calling["query_buildtags"]+topic + '/' + subcat + '/' + lang + '/' + limit + '/'+ pickRate);
  },
  SearchByTopic(query, instaAccount, limit){
    return Api(true, instaAccount).post(Calling["query_searchTopic"]+instaAccount+'/'+limit, {query});
  },
  SearchByLocation(query, instaAccount, limit){
    return Api(true, instaAccount).post(Calling["query_searchLocation"]+instaAccount+'/'+limit, {query});
  },
  GetUserMedias(instaAccount, topic){
    return Api(true).get(Calling["query_userMedia"]+instaAccount+'/'+topic)
  },
  GetUserInbox(instaAccount, topic){
    return Api(true).get(Calling["query_userInbox"]+instaAccount+'/'+topic)
  },
  GetUserFeed(instaAccount, topic){
    return Api(true).get(Calling["query_userFeed"]+instaAccount+'/'+topic)
  },
  GetUserFollowerList(instaAccount, topic){
    return Api(true).get(Calling["query_userFollowerList"]+instaAccount+'/'+topic)
  },
  GetUserFollowingList(instaAccount, topic){
    return Api(true).get(Calling["query_userFollowingList"]+instaAccount+'/'+topic)
  },
  GetUserTargetLocation(instaAccount, topic){
    return Api(true).get(Calling["query_userTargetLocation"]+instaAccount+'/'+topic)
  },
  GetUserFollowingSuggestions(instaAccount, topic){
    return Api(true).get(Calling["query_userFollowingSuggestions"]+instaAccount+'/'+topic)
  },
  GetUsersTargetList(instaAccount, topic){
    return Api(true).get(Calling["query_userTargetList"]+instaAccount+'/'+topic)
  },
  GetMediasByLocation(instaAccount, topic){
    return Api(true).get(Calling["query_mediaByLocation"]+instaAccount+'/'+topic)
  }
}
