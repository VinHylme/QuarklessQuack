using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Quarkless.Base.Auth.Common.Models.Enums;
using Quarkless.Base.Auth.Common.Models.Interfaces;
using Quarkless.Base.InstagramCollections;
using Quarkless.Base.InstagramCollections.Models;
using Quarkless.Base.ResponseResolver.Models.Interfaces;

namespace Quarkless.Controllers
{
	[ApiController]
	[HashtagAuthorize(AuthTypes.EnterpriseUsers)]
	[HashtagAuthorize(AuthTypes.TrialUsers)]
	[HashtagAuthorize(AuthTypes.BasicUsers)]
	[HashtagAuthorize(AuthTypes.PremiumUsers)]
	[HashtagAuthorize(AuthTypes.Admin)]
	public class CollectionsController : ControllerBaseExtended
	{
		private readonly IUserContext _userContext;
		private readonly ICollectionsLogic _collectionsLogic;
		private readonly IResponseResolver _responseResolver;
		public CollectionsController(IUserContext userContext, ICollectionsLogic collectionsLogic, IResponseResolver responseResolver)
		{
			_collectionsLogic = collectionsLogic;
			_userContext = userContext;
			_responseResolver = responseResolver;
		}

		[HttpGet]
		[Route("api/collections/{limit}")]
		public async Task<IActionResult> GetCollections(int limit = 5)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid ID");
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _collectionsLogic.GetCollections(limit));
			
			return ResolverResponse(results, () => results.Response.Value.Items.Count > 0);
		}

		[HttpGet]
		[Route("api/collections/{collectionId}/{limit}")]
		public async Task<IActionResult> GetCollection(long collectionId, int limit = 5)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid ID");

			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _collectionsLogic.GetCollection(collectionId,limit));

			return ResolverResponse(results);
		}

		[HttpPost]
		[Route("api/collections/create/{collectionName}")]
		public async Task<IActionResult> CreateCollection([FromRoute]string collectionName)
		{
			if (!_userContext.UserAccountExists || string.IsNullOrEmpty(collectionName))
				return BadRequest("Invalid ID");

			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(() => _collectionsLogic.CreateCollection(collectionName));

			return ResolverResponse(results);
		}

		[HttpPut]
		[Route("api/collections/{collectionId}")]
		public async Task<IActionResult> AddItemsCollection([FromRoute]long collectionId, [FromBody] AddItemsToCollectionsRequest addItemsToCollectionsRequest)
		{
			if (!_userContext.UserAccountExists || addItemsToCollectionsRequest.MediaIds.Count <= 0)
				return BadRequest("Invalid ID");
			
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _collectionsLogic.AddItemsCollection(collectionId,
				addItemsToCollectionsRequest.MediaIds.ToArray()));

			return ResolverResponse(results);
		}

		[HttpPut]
		[Route("api/collections/edit/{collectionId}")]
		public async Task<IActionResult> EditCollections([FromRoute] long collectionId, [FromBody] EditCollectionRequest editCollection)
		{
			if (!_userContext.UserAccountExists || editCollection == null) return BadRequest("Invalid Request");
			
			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(()=> _collectionsLogic.CreateCollection(collectionId,
				editCollection.CollectionName,editCollection.PhotoCoverId));
			
			return ResolverResponse(results);
		}

		[HttpDelete]
		[Route("api/collections/delete/{collectionId}")]
		public async Task<IActionResult> DeleteCollection(long collectionId)
		{
			if (!_userContext.UserAccountExists) return BadRequest("Invalid ID");

			var results = await _responseResolver
				.WithAttempts(1)
				.WithResolverAsync(
				()=> _collectionsLogic.DeleteCollection(collectionId));
			
			return ResolverResponse(results);
		}
	}
}
