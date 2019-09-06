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

namespace QuarklessContexts.Extensions
{
	public static class HelperExtensions
	{
		public static string ToJsonString<TInput>(this TInput input) => JsonConvert.SerializeObject(input);
		public static List<CultureInfo> GetSupportedCultures()
		{
			CultureInfo[] culture = CultureInfo.GetCultures(CultureTypes.AllCultures);

			// get the assembly
			Assembly assembly = Assembly.GetExecutingAssembly();

			//Find the location of the assembly
			string assemblyLocation =
				Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(assembly.CodeBase).Path));

			//Find the file anme of the assembly
			string resourceFilename = Path.GetFileNameWithoutExtension(assembly.Location) + ".resources.dll";

			//Return all culture for which satellite folder found with culture code.
			return culture.Where(cultureInfo =>
				assemblyLocation != null &&
				Directory.Exists(Path.Combine(assemblyLocation, cultureInfo.Name)) &&
				File.Exists(Path.Combine(assemblyLocation, cultureInfo.Name, resourceFilename))
			).ToList();
		}

		public static string OnlyWords(this string @string)
		{
			return Regex.Replace(@string, "[^a-zA-Z]", "").ToLower();
		}

		public static bool ContainsAnyFromCommentsAndCaptionCorpus(this string target)
		{
			return string.IsNullOrEmpty(target) 
			       || target.ContainsMentions() 
			       || target.ContainsHashtags() 
			       || target.ContainsPhoneNumber() 
			       || target.ContainsWebAddress();
		}
		public static bool ContainsWebAddress(this string target)
		{
			var regex = new Regex(@"(http|www|.*?\.)\S*");
			return regex.IsMatch(target);
		}
		public static bool ContainsMentions(this string target) => new Regex(@"@\S*|_{4}").IsMatch(target);
		public static bool ContainsHashtags(this string target) => new Regex(@"#\S*").IsMatch(target);
		public static bool ContainsPhoneNumber(this string target) => new Regex(@"\d{7,}").IsMatch(target);
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

			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// initialize the top and right of the table to 0, 1, 2, ...
			for (int i = 0; i <= n; d[i, 0] = i++) ;
			for (int j = 1; j <= m; d[0, j] = j++) ;

			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
					int min1 = d[i - 1, j] + 1;
					int min2 = d[i, j - 1] + 1;
					int min3 = d[i - 1, j - 1] + cost;
					d[i, j] = Math.Min(Math.Min(min1, min2), min3);
				}
			}
			return d[n, m];
		}
		public static string JoinEvery(this IEnumerable<string> @strings, string seperator, int every)
		{
			string result = string.Empty;

			for (int i = 0; i < @strings.Count(); i++)
			{
				if (i % every == 0 && i != 0)
				{
					for (int j = Math.Abs(i - every); j < (i - every) + every; j++)
						result += @strings.ElementAt(j) + " ";
					result += seperator;
				}
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
					continue;
				}
			}
			return resp;
		}
		public static IEnumerable<TObject> SquashMe<TObject>(this IEnumerable<IEnumerable<TObject>> @items)
		{
			foreach (var item in @items)
			{
				foreach (var ite in item)
				{
					yield return ite;
				}
			}
		}
		public static string GetDescription<T>(this T e) where T : IConvertible
		{
			if (e is Enum)
			{
				Type type = e.GetType();
				Array values = System.Enum.GetValues(type);

				foreach (int val in values)
				{
					if (val == e.ToInt32(CultureInfo.InvariantCulture))
					{
						var memInfo = type.GetMember(type.GetEnumName(val));
						var descriptionAttribute = memInfo[0]
							.GetCustomAttributes(typeof(DescriptionAttribute), false)
							.FirstOrDefault() as DescriptionAttribute;

						if (descriptionAttribute != null)
						{
							return descriptionAttribute.Description;
						}
					}
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
				var attribute = Attribute.GetCustomAttribute(field,
					typeof(DescriptionAttribute)) as DescriptionAttribute;
				if (attribute != null)
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
			throw new ArgumentException("Not found.", "description");
			// or return default(T);
		}
		public static IEnumerable<T> TakeBetween<T>(this IEnumerable<T> @items, int start, int max)
		{
			for (int x = start; x < start + max && x < @items.Count(); x++)
			{
				yield return @items.ElementAtOrDefault(x);
			}
		}
		public static IEnumerable<T> TakeAny<T>(this IEnumerable<T> @items, int amount)
		{
			if(@items.Count() < amount)
			{
				amount = @items.Count()-1;
			}
			List<T> uniqueItems = new List<T>();
			while (uniqueItems.Count < amount)
			{
				var item = @items.ElementAtOrDefault(SecureRandom.Next(@items.Count()));
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
				string text = item.GetType().GetProperty("Text").GetValue(item).ToString();
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
			catch(Exception ee)
			{
				return null;
			}
		}

		public static PropertyInfo GetProp<TObject>(this TObject @object, string propName)
		{
			try { 
				return @object.GetType().GetProperty(propName);
			}
			catch(Exception ee)
			{
				return null;
			}
		}

		public static void SaveAsJSON<T>(this IEnumerable<T> @obj, string filePath)
		{
			using (StreamWriter writter = new StreamWriter(filePath, true))
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
			Type type = typeof(T);
			var properties = type.GetProperties();

			DataTable dataTable = new DataTable();
			foreach (PropertyInfo info in properties)
			{
				dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
			}

			foreach (T entity in list)
			{
				object[] values = new object[properties.Length];
				for (int i = 0; i < properties.Length; i++)
				{
					values[i] = properties[i].GetValue(entity);
				}

				dataTable.Rows.Add(values);
			}

			return dataTable;
		}
	}
}
