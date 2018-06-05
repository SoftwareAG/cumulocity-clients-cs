using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Cumulocity.SDK.Microservices.FunctionalTest.Client
{
	public class ApiResponse<T>
	{
		public HttpStatusCode StatusCode { get; set; }
		public T Result { get; set; }
		public string ResultAsString { get; set; }
	}
}
