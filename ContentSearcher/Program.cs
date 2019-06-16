using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using ContentSearcher.Extensions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace ContentSearcher
{
	class Program
	{
		static void Main(string[] args)
		{
			string yandexBaseImageUrl = @"https://yandex.com/images/search?url=";
			string rpt = @"&rpt=imagelike";
			string googlelink = @"https://www.google.co.uk/search?hl=en-GB&tbs=simg:CAES2wIJ69mfkE6JQf4azwILELCMpwgaYgpgCAMSKKsP_1g6sD7sV_1Q6jBbgL_1w6WB-sLuiSkLZ8toS3kIbM4nCrfIaAtsCAaMIof3Znbkfi-nzcEWY5VyqxGu0nVFaQO76JjI3wFjcupjNcn1J7WJ5gAgc8I129_1pCAEDAsQjq7-CBoKCggIARIEzApbLAwLEJ3twQkaxwEKIQoOc2FraGFsaW4gaHVza3napYj2AwsKCS9tLzBidHdsdgorChhtaW5pYXR1cmUgc2liZXJpYW4gaHVza3napYj2AwsKCS9tLzA4ejh0OQooChVtYWNrZW56aWUgcml2ZXIgaHVza3napYj2AwsKCS9tLzAxcHJ6bgojChBhbGFza2FuIG1hbGFtdXRl2qWI9gMLCgkvbS8wMXAzZDcKJgoTY2FuYWRpYW4gZXNraW1vIGRvZ9qliPYDCwoJL20vMDU3XzRkDA&q=husky+wallpapers+for+iphone&tbm=isch&sa=X&ved=2ahUKEwjto-iNhrLiAhUxVBUIHcGBADUQsw56BAgGEAE&biw=1347&bih=678";
			SeleniumClient.SeleniumClient seleniumClient = new SeleniumClient.SeleniumClient();
			seleniumClient.AddArguments("headless");

			Console.Write("Please give me the url: ");

			string url_ = Console.ReadLine();
			var fullurl_ = yandexBaseImageUrl + url_ + rpt;

			try
			{
				var result = seleniumClient.Reader(fullurl_, "serp-item_pos_", 150);
				using (WebClient client = new WebClient())
				{
					int intx = 0;
					foreach (var re in result)
					{
						try
						{
							client.DownloadFile(re, $@"C:\Users\yousef.alaw\source\repos\ContentSearcher\ContentSearcher\bin\Debug\images\image{intx++}.jpg");
						}
						catch (Exception ee)
						{
							Console.Write(ee.Message);
							continue;
						}
					}
				}
			}
			catch (Exception ee)
			{
				Console.Write(ee.Message);
			}
		}
	}
}
