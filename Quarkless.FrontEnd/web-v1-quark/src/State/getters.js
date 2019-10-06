export default {
    User: state => state.user,
    IsLoggedIn: state => !!state.token,
    AuthStatus: state => state.status,
    UserRole:state=> state.role,
    GetInstagramAccounts:state => {return state.AccountData.InstagramAccounts},
    UserTimeline: state => {return state.AccountData.TimelineData},
    UserTimelineLogs: state => state.AccountData.TimelineLogData,
    UserTimelineLogForUser: state => instaId => {
      return state.AccountData.TimelineLogData.filter(item=>item.instagramAccountID === instaId);
    },
    UserProfiles: state => {return state.AccountData.Profiles},
    UserLibraries: state => {return state.AccountData.Library},
    UserMediaLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.MediaLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },
    UserCaptionLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.CaptionLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },
    UserHashtagsLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.HashtagLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },
    UserMessageLibrary: state => (instagramAccountId) => {
      return state.AccountData.Library.MessagesLibrary.filter(item=>item.instagramAccountId === instagramAccountId);
    },  
    UserProfile: state => (instaId) => 
    {
      let profile = state.AccountData.Profiles[state.AccountData.Profiles.findIndex(_=>_.instagramAccountId ==instaId)];
      if(profile!==undefined)
        return profile; 
    },
    InstagramProfilePicture: state => id => {
      var elment = state.AccountData.InstagramAccounts[state.AccountData.InstagramAccounts.findIndex(_=>_.id==id)];
      if(elment !== undefined){
        return elment.profilePicture;
      }
    },
    GetProfileConfig:state=> state.AccountData.ProfileConfg
  }