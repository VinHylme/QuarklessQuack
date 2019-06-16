using ContentSearcher.SeleniumClient;
using MoreLinq;
using Newtonsoft.Json;
using QuarklessContexts.Extensions;
using QuarklessContexts.Models.ServicesModels.DatabaseModels;
using QuarklessLogic.Handlers.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Quarkless.Worker
{
	public static class AnalyserExtensions
	{
		public static IEnumerable<A> Squash<A>(this IEnumerable<IEnumerable<A>> @items)
		{
			foreach(var item in @items)
			{
				foreach(var ite in item)
					yield return ite;
			}
		}
	}
	public class Holder<TObject>
	{
		public IEnumerable<string> Langs { get; set; }
		public IEnumerable<TObject> Items { get; set; }
		public bool IsDone { get; set; }
	}
	public class Analyser
	{
		internal struct DataHolder
		{
			public string Text;
			public string Topic;
			public string Language;
		}

		private readonly string _filePath ;
		private readonly IUtilProviders _utilProviders;
		public Analyser(string filePath, IUtilProviders utilProviders)
		{
			_filePath = filePath;
			_utilProviders = utilProviders;
		}
		public void Start()
		{
			DirectoryInfo directory = new DirectoryInfo(_filePath);
			if(!directory.Exists) return;

			List<List<UserBiographyModel>> totalBios = new List<List<UserBiographyModel>>();
			List<List<CaptionsModel>> totalCaptions = new List<List<CaptionsModel>>();
			List<List<CommentsModel>> totalComments = new List<List<CommentsModel>>();

			try { 
				foreach (var folder in directory.GetDirectories())
				{
					var folderFiles = folder.GetFiles();

					totalBios.AddRange(folderFiles.Where(_=>_.Name.Contains("_bios")).Select(o=>{ 
						var content = File.ReadAllText(o.FullName);
						var bioObject = JsonConvert.DeserializeObject<IEnumerable<UserBiographyModel>>(content);
						return bioObject.Where(w => !string.IsNullOrEmpty(w.Text) && w.User.FollowerCount > 1000).DistinctBy(t => t.Text).RemoveUslessTexts().ToList();
					}));

					totalCaptions.AddRange(folderFiles.Where(_ => _.Name.Contains("_captions")).Select(o => {
						var content = File.ReadAllText(o.FullName);
						var captionObject = JsonConvert.DeserializeObject<IEnumerable<CaptionsModel>>(content);
						return captionObject.Where(w => !string.IsNullOrEmpty(w.Text)).DistinctBy(t => t.Text).RemoveUslessTexts().ToList();
					}));

					totalComments.AddRange(folderFiles.Where(_ => _.Name.Contains("_comments")).Select(o => {
						var content = File.ReadAllText(o.FullName);
						var commentObject = JsonConvert.DeserializeObject<IEnumerable<CommentsModel>>(content);
						return commentObject.Where(w => !string.IsNullOrEmpty(w.Text)).DistinctBy(t => t.Text).RemoveUslessTexts().ToList();
					}));
				}

				var t1 = Work(totalBios, @"../../../Data/normalised_data/_bios.csv");
				var t2 = Work(totalCaptions, @"../../../Data/normalised_data/_captions.csv");
				var t3 = Work(totalComments, @"../../../Data/normalised_data/_comments.csv");

				Task.WaitAll(t1,t2,t3);

				//Task.WaitAll(ct,ca,bo);

				//if (comments.Count<=0) { 
				//	var dtc = totalComments[0].ToDataTable<CommentsModel>(
				//		a=>a.Text, b=>b.Topic, c=>c.Language);
				//	dtc.WriteToCsvFile($"../../../Data/normalised_data/_comments.csv");
				//}

				//if (captions.Count<=0) { 
				//	var dta = captions.ToDataTable(a=>a.Text, b=>b.Topic, c=>c.Language);
				//	dta.WriteToCsvFile($"../../../Data/normalised_data/_captions.csv");
				//}

				//if (bios.Count<=0) { 
				//	var dtb = bios.ToDataTable(a=>a.Text, b=>b.Topic, c=> c.Language);
				//	dtb.WriteToCsvFile($"../../../Data/normalised_data/_bios.csv");
				//}

				//foreach (var folder in directory.GetDirectories())
				//{
				//	var folderFiles = folder.GetFiles();
				//	foreach(var file in folderFiles)
				//	{
				//		file.MoveTo($"../data_bk/Processed/{file.Name}");
				//	}
				//}

			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}

		private Task Work<TObject>(IEnumerable<IEnumerable<TObject>> items, string fileName)
		{
			if(items.Count()<=0) return null;
			return Task.Run(() =>
			{
				items.ForEach(_=>
				{
					if (_.Count() <= 0) return ; 
					_ = HandleMultiTranslate(_,_.Count()/2);
					if(_==null) return; 
					
					var dtc = _.Select(o => 
					{
						return new DataHolder()
						{ 
							Text = o.GetValue("Text").ToString(),
							Topic = o.GetValue("Topic").ToString(),
							Language = o.GetValue("Language")?.ToString()
						};
					}).ToDataTable(
						a => a.Text,
						b => b.Topic,
						c => c.Language);

					dtc.WriteToCsvFile(fileName);
				});
			}).ContinueWith(a=>
			{
				var it = items;
			});
		}
		private IEnumerable<TObject> HandleMultiTranslate<TObject>(IEnumerable<TObject> items, int seperateBy = 255)
		{
			try { 
				int incrementedBy = 0;
				int amountToLoop = items.Count() >= seperateBy
					? items.Where(_ => !string.IsNullOrEmpty(_.GetValue("Text").ToString())).Count() / seperateBy
					: items.Where(_ => !string.IsNullOrEmpty(_.GetValue("Text").ToString())).Count();

				List<Holder<TObject>> workQueue = new List<Holder<TObject>>();

				for (int i = 0; i < amountToLoop; i++)
				{
					workQueue.Add(new Holder<TObject>{ 
						Items = items.TakeBetween(incrementedBy, seperateBy).ToList(),
					});
					incrementedBy += seperateBy;
				}
			
				Parallel.ForEach(workQueue, fn =>
				{
					Task.Delay(250);
					fn.Langs = _utilProviders.TranslateService.DetectLanguageViaGoogle(texts:fn.Items
						.Select(c => c.GetValue("Text").ToString()).ToArray());
					if (fn.Langs != null) { 
						for (int x = 0; x < fn.Items.Where(a => (!string.IsNullOrEmpty(a.GetValue("Text").ToString()))).Count(); x++)
						{
							fn.Items.ElementAt(x).GetProp("Language")
							.SetValue(fn.Items.ElementAt(x), fn.Langs.ElementAtOrDefault(x));
						}
					}
					else
					{

					}
					fn.IsDone = true;
				});

				return workQueue.Select(t=>t.Items).Squash();
			}
			catch(Exception ee)
			{
				Console.WriteLine(ee.Message);
				return null;
			}
		}

	}
}
