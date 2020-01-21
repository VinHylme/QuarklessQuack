using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;

namespace Quarkless.Models.Common.Extensions
{
	public static class SerializerExt
	{
		/// <summary>
		/// convert object to byte array, if obj is a string then does encoding.UTF8 to get bytes
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static byte[] SerializeToByteArray(this object obj)
		{
			switch (obj)
			{
				case null:
					return null;
				case string s:
					return Encoding.ASCII.GetBytes(s);
			}

			try
			{
				var bf = new BinaryFormatter();
				using (var ms = new MemoryStream())
				{
					bf.Serialize(ms, obj);
					return ms.ToArray();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				var json = JsonConvert.SerializeObject(obj);
				return Encoding.UTF8.GetBytes(json);
			}
		}
		public static T Deserialize<T>(this byte[] byteArray) where T : class
		{
			if (byteArray == null)
			{
				return null;
			}
			if (typeof(T) == typeof(string))
			{
				return (T) Convert.ChangeType(Encoding.ASCII.GetString(byteArray),typeof(T));
			}

			using (var memStream = new MemoryStream())
			{
				var binForm = new BinaryFormatter();
				memStream.Write(byteArray, 0, byteArray.Length);
				memStream.Seek(0, SeekOrigin.Begin);
				var obj = (T)binForm.Deserialize(memStream);
				return obj;
			}
		}
		public static object Deserialize(this byte[] byteArray, Type type)
		{
			if (byteArray == null)
				return null;
			using (var memStream = new MemoryStream())
			{
				var binForm = new BinaryFormatter();
				memStream.Write(byteArray, 0, byteArray.Length);
				memStream.Seek(0, SeekOrigin.Begin);
				return Convert.ChangeType(binForm.Deserialize(memStream), type);
			}
		}

		public static string Serialize(this object obj) => JsonConvert.SerializeObject(obj, new JsonSerializerSettings
		{

		});

		public static TObject Deserialize<TObject>(this string objJson) =>
			JsonConvert.DeserializeObject<TObject>(objJson, new JsonSerializerSettings()
			{
				ConstructorHandling = ConstructorHandling.Default
			});
		public static object Deserialize(this string objJson, Type type) =>
			JsonConvert.DeserializeObject(objJson, type);
	}
}
