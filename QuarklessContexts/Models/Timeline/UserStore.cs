
using System.ComponentModel;

namespace QuarklessContexts.Models.Timeline
{
	public class UserStore : INotifyPropertyChanged, IUserStoreDetails
	{
		public string instaramUserName { get; set; }
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
					OnPropertyChanged("OAccountId");
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
					OnPropertyChanged("OAccessToken");
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
					OnPropertyChanged("OInstagramAccountUser");
				}
			}
		}
		public string OInstagramAccountUsername
		{
			get { return instaramUserName; }
			set
			{
				if (value != instaramUserName)
				{
					instaramUserName = value;
					OnPropertyChanged("OInstagramAccountUsername");
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
