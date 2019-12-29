using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using QuarklessContexts.Models.Topics;
using Exception = System.Exception;

namespace QuarklessContexts.Extensions
{
	public static class HelperExtensions
	{
		public static int ComputeTopicHashCode(this IEnumerable<CTopic> topics)
			=> topics.Take(2).Sum(x => x.Name.ToByteArray().ComputeHash());

		public static int ComputeTotalHash(this IEnumerable<byte[]> items)
		{
			return items.Sum(_ => _.ComputeHash());
		}
		public static int ComputeTotalHash(this IEnumerable<string> items)
		{
			return items.Sum(_ => _.ToByteArray().ComputeHash());
		}
		public static int ComputeHash(this byte[] data)
		{
			if (data == null) return -1;
			unchecked
			{
				const int p = 16777619;
				var hash = (int)2166136261;

				foreach (var t in data)
					hash = (hash ^ t) * p;

				hash += hash << 13;
				hash ^= hash >> 7;
				hash += hash << 3;
				hash ^= hash >> 17;
				hash += hash << 5;
				return hash;
			}
		}
		public static byte[] ToByteArray(this string item)
		{
			if (item == null) throw new NullReferenceException("Item cannot be null");
			var result = new byte[item.Length];
			for (var x = 0; x < item.Length; x++)
			{
				result[x] = Convert.ToByte(item[x]);
			}

			return result;
		}
		public static string FromByteArray(this byte[] items)
		{
			if (items == null) throw new NullReferenceException("Item cannot be null");
			var result = string.Empty;

			foreach (var t in items)
			{
				result += Convert.ToChar(t);
			}

			return result;
		}
		public static List<string> NormaliseStringList(this List<string> items)
		{
			var splitDualCategories = items
				.Where(_ => _.Contains("&") || _.Contains("/"))
				.SelectMany(_ =>
				{
					if (_.Contains("&"))
						return _.Split("&");
					else if (_.Contains("/"))
						return _.Split("/");
					return null;
				}).Where(_ => _ != null).ToList();

			items.RemoveAll(_ => _.Contains("&") || _.Contains("/"));

			if (splitDualCategories.Any())
				items.AddRange(splitDualCategories);

			items = items.Select(_ =>
			{
				_ = Regex.Replace(_, @"\(\w*\)|[^\w\s\(\)]", " ");
				_ = Regex.Replace(_, @"^\s+", "");
				return _;
			}).ToList();

			return items.Distinct().ToList();
		}
		public static string GetPathByFolderName(this string folderName)
		{
			var initialPath = Directory.GetCurrentDirectory();
			var currentPath = initialPath;
			while (currentPath != Directory.GetDirectoryRoot(initialPath))
			{
				foreach (var directory in Directory.GetDirectories(currentPath))
				{
					if (directory.EndsWith(folderName))
						return directory;
				}

				currentPath = Directory.GetParent(currentPath).FullName;
			}

			return string.Empty;
		}
		public static IServiceCollection Append(this IServiceCollection @org, IServiceCollection all)
		{
			if (all == null || !all.Any()) return org;
			foreach (var desc in all)
			{
				@org.Add(desc);
			}
			return org;
		}
		public static string ToJsonString<TInput>(this TInput input) => JsonConvert.SerializeObject(input);
		public static List<CultureInfo> GetSupportedCultures()
		{
			var culture = CultureInfo.GetCultures(CultureTypes.AllCultures);

			// get the assembly
			var assembly = Assembly.GetExecutingAssembly();

			//Find the location of the assembly
			var assemblyLocation =
				Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(assembly.CodeBase).Path));

			//Find the file anme of the assembly
			var resourceFilename = Path.GetFileNameWithoutExtension(assembly.Location) + ".resources.dll";

			//Return all culture for which satellite folder found with culture code.
			return culture.Where(cultureInfo =>
				assemblyLocation != null &&
				Directory.Exists(Path.Combine(assemblyLocation, cultureInfo.Name)) &&
				File.Exists(Path.Combine(assemblyLocation, cultureInfo.Name, resourceFilename))
			).ToList();
		}
		public static int IndexOf<T>(this IEnumerable<T> items, T find)
		{
			var itemEnumerable = items as T[] ?? items.ToArray();
			if (find == null) return -1;
			for (var x = 0; x < itemEnumerable.Count(); x++)
			{
				if (Equals(itemEnumerable.ElementAt(x), find))
					return x;
			}
			return -1;
		}
		public static T CloneObject<T>(this T obj)
		{
			var serialized = JsonConvert.SerializeObject(obj);
			return (T) JsonConvert.DeserializeObject(serialized, obj.GetType());
		}
		public static bool IsValidUrl(this string url) => Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute);
		public static bool IsBase64String(this string base64)
		{
			try
			{
				var filter = base64.Split(',')[1];
				var buffer = new Span<byte>(new byte[filter.Length]);
				return Convert.TryFromBase64String(filter, buffer, out int bytesParsed);
			}
			catch (Exception ee)
			{
				return false;
			}
		}

		public static IEnumerable<TType> ToCastList<TType> (this IEnumerable<object> @objects)
		{
			var results = new List<TType>();
			var classes = (from domainAssembly in AppDomain.CurrentDomain.GetAssemblies()
				from assemblyType in domainAssembly.GetTypes()
				where typeof(TType).IsAssignableFrom(assemblyType)
				select assemblyType).ToArray();

			if (!classes.Any()) return null;

			var objectsArray = objects as object[] ?? objects.ToArray();

			foreach (var type in classes)
			{
				try
				{
					var conv = JsonConvert.DeserializeObject(objectsArray.First().ToString(), type, new JsonSerializerSettings
					{
						NullValueHandling = NullValueHandling.Include
					});

				}
				catch
				{
					//
				}
			}

			foreach (var @object in objectsArray)
			{
				var exists = classes.IndexOf(@object.GetType()) > 0;
				if (!exists) continue;
				results.Add((TType) @object);
			}

			return !results.Any() ? null : results;
		}

		public static string OnlyWords(this string @string)
			=> Regex.Replace(@string, "[^a-zA-Z]", "").ToLower();
		
		public static int Similarity(this string s, string t)
		{
			if (string.IsNullOrEmpty(s))
			{
				return string.IsNullOrEmpty(t) ? 0 : t.Length;
			}

			if (string.IsNullOrEmpty(t))
			{
				return s.Length;
			}

			var n = s.Length;
			var m = t.Length;
			var d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (var i = 0; i <= n; d[i, 0] = i++) ;
			for (var j = 1; j <= m; d[0, j] = j++) ;

			for (var i = 1; i <= n; i++)
			{
				for (var j = 1; j <= m; j++)
				{
					var cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					var min1 = d[i - 1, j] + 1;
					var min2 = d[i, j - 1] + 1;
					var min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}

		public static string JoinEvery(this IEnumerable<string> @strings, string separateBy, int every)
		{
			var result = string.Empty;

			for (var i = 0; i < @strings.Count(); i++)
			{
				if (i % every != 0 || i == 0) continue;
				for (var j = Math.Abs(i - every); j < (i - every) + every; j++)
					result += @strings.ElementAt(j) + " ";
				result += separateBy;
			}

			return result;
		}
		public static object TryGetType(this string json, params Type[] tests)
		{
			object resp = null;
			foreach(var test in tests) { 
				try
				{
					resp = JsonConvert.DeserializeObject(json,test);
				}
				catch
				{
					//
				}
			}
			return resp;
		}
		public static IEnumerable<TObject> SquashMe<TObject>(this IEnumerable<IEnumerable<TObject>> @items)
		{
			return @items.SelectMany(item => item);
		}
		public static string GetDescription<T>(this T e) where T : IConvertible
		{
			if (!(e is Enum)) return null; // could also return string.Empty
			var type = e.GetType();
			var values = System.Enum.GetValues(type);

			foreach (int val in values)
			{
				if (val != e.ToInt32(CultureInfo.InvariantCulture)) continue;
				var memInfo = type.GetMember(type.GetEnumName(val));

				if (memInfo[0]
					.GetCustomAttributes(typeof(DescriptionAttribute), false)
					.FirstOrDefault() is DescriptionAttribute descriptionAttribute)
				{
					return descriptionAttribute.Description;
				}
			}
			return null; // could also return string.Empty
		}
		public static T GetValueFromDescription<T>(this string description)
		{
			var type = typeof(T);
			if (!type.IsEnum) throw new InvalidOperationException();
			foreach (var field in type.GetFields())
			{
				if (Attribute.GetCustomAttribute(field,
					typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
				{
					if (attribute.Description == description)
						return (T)field.GetValue(null);
				}
				else
				{
					if (field.Name == description)
						return (T)field.GetValue(null);
				}
			}
			throw new ArgumentException("Not found.", nameof(description));
			// or return default(T);
		}
		public static IEnumerable<T> TakeBetween<T>(this IEnumerable<T> @items, int start, int max)
		{
			for (var x = start; x < start + max && x < @items.Count(); x++)
			{
				yield return @items.ElementAtOrDefault(x);
			}
		}
		public static IEnumerable<T> TakeAny<T>(this IEnumerable<T> @items, int amount)
		{
			var enumerable = @items as T[] ?? @items.ToArray();
			if(enumerable.Count() < amount)
			{
				amount = enumerable.Count()-1;
			}
			var uniqueItems = new List<T>();
			while (uniqueItems.Count < amount)
			{
				var item = enumerable.ElementAtOrDefault(SecureRandom.Next(enumerable.Count()));
				if (!uniqueItems.Contains(item))
				{
					uniqueItems.Add(item);
				}
			}
			return uniqueItems;
		}
		public static IEnumerable<T> RemoveUslessTexts<T>(this IEnumerable<T> @items)
		{
			foreach(var item in @items)
			{
				if(item.GetType().GetProperty("Text")==null) throw new Exception("Format not supported");
				var text = item.GetType().GetProperty("Text")?.GetValue(item).ToString();
				if(!text.Contains("@"))
					yield return item;
			}
		}
		public static object GetValue<TObject>(this TObject @object, string propName)
		{
			try { 
				var prop = GetProp(@object,propName);
				if(prop == null) return null;
				return @object.GetType().GetProperty(propName).GetValue(@object)?? null;
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch(Exception ee)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				return null;
			}
		}
		public static PropertyInfo GetProp<TObject>(this TObject @object, string propName)
		{
			try { 
				return @object.GetType().GetProperty(propName);
			}
#pragma warning disable CS0168 // Variable is declared but never used
			catch(Exception ee)
#pragma warning restore CS0168 // Variable is declared but never used
			{
				return null;
			}
		}

		public static void SaveAsJson<T>(this IEnumerable<T> @obj, string filePath)
		{
			using (var writter = new StreamWriter(filePath, true))
			{
				writter.WriteLine(JsonConvert.SerializeObject(@obj));
			}
		}
		public static IEnumerable<string> CleanText(this string[] texts)
		{
			foreach (var text in texts)
			{
				yield return HttpUtility.UrlEncode(text, Encoding.UTF8);
			}
		}
		public static void WriteToCsvFile(this DataTable dataTable, string filePath)
		{
			var fileContent = new StringBuilder();

			foreach (var col in dataTable.Columns)
			{
				fileContent.Append(col.ToString() + ",");
			}

			fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);

			foreach (DataRow dr in dataTable.Rows)
			{
				foreach (var column in dr.ItemArray)
				{
					fileContent.Append("\"" + column.ToString() + "\",");
				}

				fileContent.Replace(",", System.Environment.NewLine, fileContent.Length - 1, 1);
			}
			var lines = fileContent.ToString().Split(Environment.NewLine).Where(x=>!string.IsNullOrEmpty(x)).ToList();
			if (File.Exists(filePath))
			{
				lines.RemoveAt(0);
			}
			System.IO.File.AppendAllLines(filePath, lines);
		}
		public static DataTable CreateDataTable<T>(this IEnumerable<T> list)
		{
			var type = typeof(T);
			var properties = type.GetProperties();

			var dataTable = new DataTable();
			foreach (var info in properties)
			{
				dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
			}

			foreach (var entity in list)
			{
				var values = new object[properties.Length];
				for (var i = 0; i < properties.Length; i++)
				{
					values[i] = properties[i].GetValue(entity);
				}

				dataTable.Rows.Add(values);
			}

			return dataTable;
		}
	}
}
