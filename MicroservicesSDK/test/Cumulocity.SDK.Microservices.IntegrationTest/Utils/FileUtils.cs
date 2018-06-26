using System;
using System.IO;
using System.Linq;

namespace Cumulocity.SDK.Microservices.IntegrationTest.Utils
{
	public static class FileUtils
	{
		public static string GetImportPath()
		{
			string[] importPaths =
			{
				@"TestData", @"..\TestData", @"..\..\TestData", @"..\..\..\TestData",
			};
			string importPath = importPaths.FirstOrDefault(Directory.Exists);
			Guard.ThrowIfNull(importPath, "importPath not found");
			return importPath;
		}

		public static Credentials GetCredentials()
		{
			var importPath = GetImportPath();
			Credentials creds = new Credentials();

			using (var reader = new StreamReader(importPath + @"\creds.txt"))
			{
				var userName = reader.ReadLine();
				var hash = reader.ReadLine();

				if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(userName.Split(':')[1]) || String.IsNullOrEmpty(hash) || String.IsNullOrEmpty(hash.Split(':')[1]))
					throw new ArgumentNullException("Credentials are null.");
				creds.UserName = userName.Split(':')[1];
				creds.Hash = hash.Split(':')[1];
			}

			return creds;
		}
	}
}