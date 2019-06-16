using Microsoft.AspNetCore.Mvc;
using Quarkless.Auth.AuthTypes;
using QuarklessContexts.Contexts;
using QuarklessContexts.Models.ApiModels;
using QuarklessLogic.Logic.CollectionsLogic;
using System.Linq;
using System.Threading.Tasks;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class CollectionsController : ControllerBase
	{
		private readonly IUserContext _userContext;
		private readonly ICollectionsLogic _collectionsLogic;
		public CollectionsController(IUserContext userContext, ICollectionsLogic collectionsLogic)
		{
			_collectionsLogic = collectionsLogic;
			_userContext = userContext;
		}

		[HttpGet]
		[Route("api/collections/{limit}")]
		public async Task<IActionResult> GetCollections(int limit = 5)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _collectionsLogic.GetCollections(limit);
				if (results.Succeeded && results.Value.Items.Count > 0)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("Invalid ID");
		}

		[HttpGet]
		[Route("api/collections/{collectionId}/{limit}")]
		public async Task<IActionResult> GetCollection(long collectionId, int limit = 5)
		{
			if (_userContext.UserAccountExists)
			{
				var results = await _collectionsLogic.GetCollection(collectionId,limit);
				if (results.Succeeded)
				{
					return Ok(results.Value);
				}
				return NotFound(results.Info);
			}
			return BadRequest("Invalid ID");
		}

		[HttpPost]
		[Route("api/collections/create/{collectionName}")]
		public async Task<IActionResult> CreateCollection([FromRoute]string collectionName)
		{
			if (_userContext.UserAccountExists && !string.IsNullOrEmpty(collectionName))
			{
				var res = await _collectionsLogic.CreateCollection(collectionName);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				return BadRequest(res.Info);
			}
			return BadRequest("Invalid ID");
		}

		[HttpPut]
		[Route("api/collections/{collectionId}")]
		public async Task<IActionResult> AddItemsCollection([FromRoute]long collectionId, [FromBody] AddItemsToCollectionsRequest addItemsToCollectionsRequest)
		{
			if (_userContext.UserAccountExists && addItemsToCollectionsRequest.MediaIds.Count>0)
			{
				var res = await _collectionsLogic.AddItemsCollection(collectionId, addItemsToCollectionsRequest.MediaIds.ToArray());
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				return BadRequest(res.Info);
			}
			return BadRequest("Invalid ID");
		}

		[HttpPut]
		[Route("api/collections/edit/{collectionId}")]
		public async Task<IActionResult> EditCollections([FromRoute] long collectionId, [FromBody] EditCollectionRequest editCollection)
		{
			if (_userContext.UserAccountExists && editCollection!=null)
			{
				var res = await _collectionsLogic.CreateCollection(collectionId,editCollection.CollectionName,editCollection.PhotoCoverId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				return NotFound(res.Info);
			}
			return BadRequest("Invalid Request");
		}

		[HttpDelete]
		[Route("api/collections/delete/{collectionId}")]
		public async Task<IActionResult> DeleteCollection(long collectionId)
		{
			if (_userContext.UserAccountExists)
			{
				var res = await _collectionsLogic.DeleteCollection(collectionId);
				if (res.Succeeded)
				{
					return Ok(res.Value);
				}
				return NotFound(res.Info);
			}
			return BadRequest("Invalid ID");
		}
	}
}
