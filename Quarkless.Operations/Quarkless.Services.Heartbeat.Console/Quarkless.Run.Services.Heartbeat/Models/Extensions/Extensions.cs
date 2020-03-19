using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Quarkless.Models.Common.Extensions;
using Quarkless.Models.SearchResponse;

namespace Quarkless.Run.Services.Heartbeat.Models.Extensions
{
	public static class Extensions
	{
		public static ConcurrentQueue<T> EnqueueAll <T>(this IEnumerable<T> items)
		{
			var results = new ConcurrentQueue<T>();
			foreach (var item in items)
			{
				results.Enqueue(item);
			}

			return results;
		}
		public static IEnumerable<MediaResponse> RandomAny(this Media medias, int amount)
		{
			return medias.Medias.TakeAny(amount);
		}
		public static IEnumerable<TA> Between<TA>(this IEnumerable<TA> obj, int start, int limit)
		{
			for (var j = start; j < limit + start; j++)
			{
				yield return obj.ElementAtOrDefault(j);
			}
		}
		public static List<List<TCut>> CutObject<TCut>(this List<TCut> @item, int cutAmount) where TCut : new()
		{
			var pos = 0;
			var objects = new List<List<TCut>>();
			for (var x = 0; x <= @item.Count(); x++)
			{
				if (x % cutAmount == 0 && x != 0)
				{
					objects.Add(@item.Between(pos, cutAmount).ToList());
				}
				pos = x;
			}
			return objects;
		}
		public static IEnumerable<Media> CutObjects(this Media medias, int amount)
		{
			if (medias == null) return null;
			var pos = 0;
			var media = new List<Media>();
			for (var x = 0; x <= medias.Medias.Count; x++)
			{
				if (x % amount != 0 || x == 0) continue;
				media.Add(new Media
				{
					Medias = medias.Medias.Between(pos, amount).ToList(),
					Errors = medias.Errors
				});
				pos = x;
			}
			return media;
		}
		public static T RandomAny<T>(this IEnumerable<T> @ts)
		{
			return ts.ElementAtOrDefault(SecureRandom.Next(@ts.Count() - 1));
		}

	}
}