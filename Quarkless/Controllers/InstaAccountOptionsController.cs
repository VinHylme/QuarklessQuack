using Microsoft.AspNetCore.Mvc;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.UserAuth.AuthTypes;
using QuarklessLogic.Logic.InstaAccountOptionsLogic;

namespace Quarkless.Controllers
{

    [ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class InstaAccountOptionsController : ControllerBase
    {
		private readonly IInstaAccountOptionsLogic _instaAccountOptionsLogic;
		private readonly IUserContext _userContext;
		public InstaAccountOptionsController(IUserContext userContext,IInstaAccountOptionsLogic instaAccountOptionsLogic)
		{
			_instaAccountOptionsLogic = instaAccountOptionsLogic;
			_userContext = userContext;
		}




    }
}