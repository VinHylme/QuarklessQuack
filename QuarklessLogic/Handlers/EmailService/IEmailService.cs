using System.Threading.Tasks;
using QuarklessContexts.Models.FakerModels;
using QuarklessContexts.Models.Proxies;

namespace QuarklessLogic.Handlers.EmailService
{
	public interface IEmailService
	{
		Task CreateGmailEmail(ProxyModel proxy,FakerModel person);
	}
}