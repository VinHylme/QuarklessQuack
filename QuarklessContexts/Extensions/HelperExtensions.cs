using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace QuarklessContexts.Extensions
{
	public static class HelperExtensions
	{
		public static IEnumerable<T> TakeBetween<T>(this IEnumerable<T> @items, int start, int max)
		{
			for (int x = start; x < start + max && x < @items.Count(); x++)
			{
				yield return @items.ElementAtOrDefault(x);
			}
		}
		public static IEnumerable<T> TakeAny<T>(this IEnumerable<T> @items, int amount)
		{
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
			StringBuilder fileContent = new StringBuilder();

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
			var lines = fileContent.ToString().Split(Environment.NewLine).ToList();
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
