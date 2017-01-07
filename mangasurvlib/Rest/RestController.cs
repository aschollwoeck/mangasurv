﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace mangasurvlib.Rest
{
    public static class HttpVerbs
    {
        public static readonly string Get = "GET";
        public static readonly string Post = "POST";
        public static readonly string Put = "PUT";
        public static readonly string Delete = "DELETE";
    }

    public class RestController
    {
        public const string API_URL = "http://localhost:50107/api";

        private readonly Uri Url;

        public static RestController GetRestController()
        {
            return GetRestController(API_URL);
        }

        public static RestController GetRestController(string sUrl)
        {
            if (String.IsNullOrEmpty(sUrl))
                return new RestController(API_URL);

            return new RestController(sUrl);
        }

        /// <summary>
        /// Instanziate REST client object.
        /// </summary>
        /// <param name="sUrl">Endpoint-Url which the object calls (e.g. "http://localhost:8080/api/test").</param>
        public RestController(string sUrl)
        {
            if (sUrl.Length == 0)
                throw new ArgumentException("Endpoint-Url for REST API is empty!");

            if (sUrl.EndsWith("/"))
                sUrl = sUrl.Substring(0, sUrl.Length - 1);

            this.Url = new Uri(sUrl);
        }

        /// <summary>
        /// Calls Endpoint-Url with "GET" method.
        /// </summary>
        /// <returns>Returns usually a list of objects.</returns>
        public Tuple<HttpStatusCode, string> Get()
        {
            Tuple<HttpWebResponse, string> result = this.DoRequest(this.Url, HttpVerbs.Get);

            return new Tuple<HttpStatusCode, string>(result.Item1.StatusCode, result.Item2);
        }

        /// <summary>
        /// Call Endpoint-Url with "GET" method and an additional parameter (e.g. "http://localhost:8080/api/test/1").
        /// </summary>
        /// <param name="sParam">Additional parameter which will be appended to Endpoint-Url.</param>
        /// <returns>Returns usually a single object.</returns>
        public Tuple<HttpStatusCode, string> Get(string sParam)
        {
            return this.Get(sParam, new List<KeyValuePair<string, string>>());
        }

        /// <summary>
        /// Call Endpoint-Url with "GET" method and an additional parameter and a query string.(e.g. "http://localhost:8080/api/test/1?sort=asc").
        /// </summary>
        /// <param name="sParam">Additional parameter which will be appended to Endpoint-Url.</param>
        /// <param name="Query">Querystring which will be appended to Endpoint-Url.</param>
        /// <returns>Returns objects of API.</returns>
        public Tuple<HttpStatusCode, string> Get(string sParam, List<KeyValuePair<string, string>> Query)
        {
            string sQueryString = String.Empty;
            for (int i = 0; i < Query.Count; i++)
            {
                if(i == 0)
                    sQueryString = String.Format("?{0}={1}", Query[i].Key, Query[i].Value);
                else
                    sQueryString = String.Format("{0}&{1}={2}", sQueryString, Query[i].Key, Query[i].Value);
            }

            UriBuilder ub = new UriBuilder(this.Url.AbsoluteUri + "/" + sParam);
            ub.Query = sQueryString;

            Tuple<HttpWebResponse, string> result = this.DoRequest(ub.Uri, HttpVerbs.Get);

            return new Tuple<HttpStatusCode, string>(result.Item1.StatusCode, result.Item2);
        }

        /// <summary>
        /// Call Endpoint-Url with "POST" method to create new objects.
        /// </summary>
        /// <param name="sParam">Additional parameter which will be appended to Endpoint-Url.</param>
        /// <param name="o">The object which will be added to the POST call and which shall be created.</param>
        /// <returns>Newly created object of Endpoint.</returns>
        public Tuple<HttpStatusCode, string> Post(string sParam, object o)
        {
            UriBuilder ub = new UriBuilder(this.Url.AbsoluteUri + "/" + sParam);

            HttpWebRequest request = this.CreateHttpWebRequest(ub.Uri, HttpVerbs.Post);
            request.ContentType = "application/json";

            UTF8Encoding encoding = new UTF8Encoding();
            var bytes = Encoding.GetEncoding(encoding.CodePage).GetBytes(o.ToString());

            HttpWebResponse response = this.GetHttpWebResponse(request);

            return new Tuple<HttpStatusCode, string>(response.StatusCode, this.ExtractResponseContent(response));
        }

        /// <summary>
        /// Call Endpoint-Url with "PUT" method to create new objects.
        /// </summary>
        /// <param name="sParam">Additional parameter which will be appended to Endpoint-Url.</param>
        /// <param name="o">The object which will be added to the PUT call and which shall be updated.</param>
        /// <returns>Newly created object of Endpoint.</returns>
        public Tuple<HttpStatusCode, string> Put(string sParam, object o)
        {
            UriBuilder ub = new UriBuilder(this.Url.AbsoluteUri + "/" + sParam);

            HttpWebRequest request = this.CreateHttpWebRequest(ub.Uri, HttpVerbs.Put);
            request.ContentType = "application/json";

            UTF8Encoding encoding = new UTF8Encoding();
            var bytes = Encoding.GetEncoding(encoding.CodePage).GetBytes(o.ToString());

            HttpWebResponse response = this.GetHttpWebResponse(request);

            return new Tuple<HttpStatusCode, string>(response.StatusCode, this.ExtractResponseContent(response));
        }

        /// <summary>
        /// Call Endpoint-Url with "DELETE" method. The object to delete has to be specified via the sParam parametre.
        /// </summary>
        /// <param name="sParam">The identifier which will be appended to Endpoint-Url to delete the ressource at the Endpoint.</param>
        /// <returns></returns>
        public Tuple<HttpStatusCode, string> Delete(string sParam)
        {
            UriBuilder ub = new UriBuilder(this.Url.AbsoluteUri + "/" + sParam);
            Tuple<HttpWebResponse, string> result = this.DoRequest(ub.Uri, HttpVerbs.Delete);

            return new Tuple<HttpStatusCode, string>(result.Item1.StatusCode, result.Item2);
        }

        /// <summary>
        /// Not implemented yet.
        /// Call Endpoint-Url with "PUT" method and update an existing ressource.
        /// </summary>
        /// <param name="o">Object with the updated attributes.</param>
        /// <returns>Returns the updated object.</returns>
        public object Update(object o)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create a HttpWebRequest gets the HttpWebResponse and extracts the response content.
        /// </summary>
        /// <param name="Uri">Endpoint-Uri which shall be called.</param>
        /// <param name="sMethod">HTTP method with which the Endpoint should be called.</param>
        /// <returns>Returns the HttpWebResponse object and the content (if exists).</returns>
        private Tuple<HttpWebResponse, string> DoRequest(Uri Uri, string sMethod)
        {
            HttpWebRequest request = this.CreateHttpWebRequest(Uri, sMethod);
            HttpWebResponse response = this.GetHttpWebResponse(request);

            return new Tuple<HttpWebResponse, string>(response, this.ExtractResponseContent(response));
        }

        /// <summary>
        /// Extracts the content of a HttpWebResponse.
        /// </summary>
        /// <param name="response">Response object which contains the content.</param>
        /// <returns>Extracted content of response.</returns>
        private string ExtractResponseContent(HttpWebResponse response)
        {
            if (response == null)
                return String.Empty;

            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
                return String.Empty;

            Stream stream = response.GetResponseStream();
            using (StreamReader sr = new StreamReader(stream))
            {
                string result = sr.ReadToEnd();
                sr.Dispose();

                return result;
            }
        }

        /// <summary>
        /// Create a HttpWebRequest with predefined settings.
        /// </summary>
        /// <param name="Uri">Endpoint-Url which shall be called.</param>
        /// <param name="sMethod">HTTP method which should be used.</param>
        /// <returns>Returns a newly created HttpWebRequest object.</returns>
        private HttpWebRequest CreateHttpWebRequest(Uri Uri, string sMethod)
        {
            HttpWebRequest request = WebRequest.Create(Uri) as HttpWebRequest;
            request.Method = sMethod;
            request.ContinueTimeout = 5000;

            return request;
        }

        /// <summary>
        /// Returns the HttpWebResponse object of a HttpWebRequest.
        /// </summary>
        /// <param name="request">HttpWebRequest from which a response is requiered.</param>
        /// <returns>Returns the HttpWebResponse object.</returns>
        private HttpWebResponse GetHttpWebResponse(HttpWebRequest request)
        {
            try
            {
                return request.GetResponseAsync().Result as HttpWebResponse;
            }
            catch (WebException ex)
            {
                return (ex.Response as HttpWebResponse);
            }
        }
    }
}
