
using System.ComponentModel;

namespace QuarklessContexts.Models.Timeline
{
	public class UserStore : INotifyPropertyChanged, IUserStoreDetails
	{
		private string accountId { get; set; }
		private string instaAccountId { get; set; }
		private string accessToken { get; set; }
		public UserStore()
		{
			
		}

		public void AddUpdateUser(string accountId, string InstagramAccountId, string accessToken)
		{
			this.accountId = accountId;
			this.instaAccountId = InstagramAccountId;
			this.accessToken = accessToken;
		}
		public string OAccountId
		{
			get
			{
				return accountId;
			}
			set
			{
				if (value != accountId)
				{
					accountId = value;
					OnPropertyChanged("GetAccountId");
				}
			}
		}
		public string OAccessToken
		{
			get { return accessToken; }
			set
			{
				if (value != accessToken)
				{
					accessToken = value;
					OnPropertyChanged("GetAccessToken");
				}
			}
		}
		public string OInstagramAccountUser
		{
			get { return instaAccountId; }
			set
			{
				if (value != instaAccountId)
				{
					instaAccountId = value;
					OnPropertyChanged("GetInstagramAccountUser");
				}
			}
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
		}
		public event PropertyChangedEventHandler PropertyChanged;
	}
}
