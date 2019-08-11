import Api from './Api'
export default {
  GetProfileConfig(){
    return Api(true).get('query/config');
  },
  GooglePlaceSearch(query){
    return Api(true).get('query/search/places/'+query);
  },
  GooglePlacesAutocomplete(query, radius){
    return Api(true).get('query/search/places/auto/'+query+'/'+radius);
  },
  SimilarImageSearch(urls, limit, offset){
    return Api(true).put('query/search/similar/'+ limit +'/' + offset, urls);
  },
  ReleatedTopic(instaAccount, topic){
    return Api(true, instaAccount).get('query/releated/'+topic);
  }
}