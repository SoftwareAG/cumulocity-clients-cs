using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cumulocity.SDK.Microservices.FunctionalTest.Utils
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
			Credentials creds = new Credentials();
			string userName;
			string hash ;
			string path = @"Properties\launchSettings.json";
			
				           if (File.Exists(path))
				           {
				               using (var file = File.OpenText(path))
					               {
									var reader = new JsonTextReader(file);
									var jObject = JObject.Load(reader);
					
									var variables = jObject
						                      .GetValue("profiles")
						                      //select a proper profile here
						                       .SelectMany(profiles => profiles.Children())
						                       .SelectMany(profile => profile.Children<JProperty>())
						                       .Where(prop => prop.Name == "environmentVariables")
						                       .SelectMany(prop => prop.Value.Children<JProperty>())
						                       .ToList();
					
						                   foreach (var variable in variables)
						                   {
							                   if (Environment.GetEnvironmentVariable(variable.Name) == null)
													Environment.SetEnvironmentVariable(variable.Name, variable.Value.ToString());
						                   }
					               }

					           userName = Environment.GetEnvironmentVariable("TEST_FUNCTIONAL_USERNAME");
					           hash = Environment.GetEnvironmentVariable("TEST_FUNCTIONAL_HASH");

							   CreateCreds(userName, hash, creds);
				           }
				           else
				           {
								var importPath = GetImportPath();
								JObject settings = JObject.Parse(File.ReadAllText(importPath + @"\settings.txt"));
								userName = (string)settings["credentials"]["userName"];
								hash = (string)settings["credentials"]["hash"];

					           CreateCreds(userName, hash, creds);
						}

			return creds;
		}

		private static void CreateCreds(string userName, string hash, Credentials creds)
		{
			if (String.IsNullOrEmpty(userName) || String.IsNullOrEmpty(hash))
				throw new ArgumentNullException("Credentials are null.");
			creds.UserName = userName;
			creds.Hash = hash;
		}
	}
}