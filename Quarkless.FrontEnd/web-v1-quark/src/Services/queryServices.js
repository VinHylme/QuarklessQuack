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
  ReleatedTopicByParent(parentId){
    return Api(true).get(Calling['query_related_by_parent'] + parentId);
  },
  BuildTags(suggestRequest){
    return Api(true).post(Calling["query_buildtags"],suggestRequest);
  },
  SearchByTopic(query, instaAccount, limit){
    return Api(true, instaAccount).post(Calling["query_searchTopic"]+instaAccount+'/'+limit, {query});
  },
  SearchByLocation(query, instaAccount, limit){
    return Api(true, instaAccount).post(Calling["query_searchLocation"]+instaAccount+'/'+limit, {query});
  },
  GetUserMedias(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userMedia"],profileRequest)
  },
  GetUserInbox(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
   }
    return Api(true).post(Calling["query_userInbox"],profileRequest)
  },
  GetUserFeed(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userFeed"], profileRequest)
  },
  GetUserFollowerList(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userFollowerList"], profileRequest)
  },
  GetUserFollowingList(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userFollowingList"], profileRequest)
  },
  GetUserTargetLocation(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userTargetLocation"],profileRequest)
  },
  GetUserFollowingSuggestions(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userFollowingSuggestions"], profileRequest)
  },
  GetUsersTargetList(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
    }
    return Api(true).post(Calling["query_userTargetList"], profileRequest)
  },
  GetMediasByLocation(instaAccount, topic){
    const profileRequest = {
      accountId:null,
      instagramAccountId:instaAccount,
      topic: topic
  }
    return Api(true).post(Calling["query_mediaByLocation"],profileRequest)
  },
  GetRecentComments(instaAccount, topic){

    const profileRequest = {
        accountId:null,
        instagramAccountId:instaAccount,
        topic: topic
    }
  return Api(true).post(Calling["query_recentComments"], profileRequest)
  }
}
