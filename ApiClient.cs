using QuickSharpApiClient.Common.Enums;
using QuickSharpApiClient.Common.Json;
using QuickSharpApiClient.Feed.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace QuickSharpApiClient
{
    public class ApiClient
    {
        private IDictionary<string, string> _headers = new Dictionary<string, string>();
        private IDictionary<HttpContentType, string> _contentTypes = new Dictionary<HttpContentType, string> 
        { 
            { HttpContentType.Json, "application/json" }, 
            { HttpContentType.Xml, "application/xml" }, 
            { HttpContentType.Text, "application/text" } 
        };
        public bool UseDefaultCredentials { get; set; }
        public Credentials UseCredentials { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public int MaxResponseContentBufferSize { get; set; }
        public string ApiUrl { get; private set; }
        public HttpContentType DefaultContentType { get; set; }
        public MethodType? Method { get; private set; }
        public FileAccess FileAccess { get; set; }
        public string ContentType
        {
            get
            {
                string contentType;
                _contentTypes.TryGetValue(DefaultContentType, out contentType);
                return contentType;
            }
        }
        public bool IsVirualApiEnable { get; set; }
        public void AddHeader(string key, string value)
        {
            _headers.Add(key, value);
        }

        public ApiClient(string uri, MethodType method, FileAccess fileAccess = null)
        {
            Init(new Uri(uri), method, fileAccess);
        }

        private void Init(Uri uri, MethodType? method, FileAccess fileAccess)
        {
            MaxResponseContentBufferSize = 256000;
            DefaultContentType = HttpContentType.Json;
            Method = method;
            ApiUrl = uri.OriginalString;
            this.AddHeader("User-Agent", "Mozilla /5.0 (Compatible MSIE 9.0;Windows NT 6.1;WOW64; Trident/5.0)");
            FileAccess = fileAccess ?? new FileAccess() { ApiUri = uri, Method = method };
        }

        public ApiClient(FileAccess fileAccess)
        {
            Init(fileAccess.ApiUri, fileAccess.Method, fileAccess);
        }

        public Task<string> SendAsync(string contentRequest = "")
        {
            try
            {
                var uri = new Uri(ApiUrl);
                Task<string> resultAsync;

                if (IsVirualApiEnable)
                {
                    //Create and get file name 
                    Debug.Print("fake file path: " + FileAccess.FileName);

                    var fakeResponse = FileAccess.Read(contentRequest);

                    if (!string.IsNullOrWhiteSpace(fakeResponse.Result))
                    {
                        Debug.Print("fakeResponse: " + fakeResponse.Result);
                        return fakeResponse;
                    }
                }

                var httpContentRequest = new StringContent(contentRequest, System.Text.Encoding.UTF8, ContentType);

                ApiClientHelper.AllowUnsafeHeaderParsing(true);
                var response = SendAsync(uri, httpContentRequest, Method);
                ApiClientHelper.AllowUnsafeHeaderParsing(false);

                resultAsync = response.Result.Content.ReadAsStringAsync();

                //save fake response
                if (IsVirualApiEnable && !string.IsNullOrWhiteSpace(FileAccess.FileName))
                {
                    FileAccess.Save(contentRequest, resultAsync.Result);
                    Debug.Print("Save api response: " + resultAsync.Result);
                }

                return resultAsync;
            }
            catch (HttpRequestException hre)
            {
                throw hre;
            }
            catch (Exception ex)
            {
                // For debugging
                throw ex;
            }
        }

        public TResponse SendAsync<TRequest, TResponse>(TRequest request)
        {
            var contentRequest = request.ToJson();
            var response = SendAsync(contentRequest);
            return response.Result.FromJson<TResponse>();
        }

        public TResponse SendAsync<TResponse>()
        {
            var response = SendAsync();
            return response.Result.FromJson<TResponse>();
        }

        private async Task<HttpResponseMessage> SendAsync(Uri uri, HttpContent content, MethodType? method)
        {
            if (UseDefaultCredentials)
            {
                UseCredentials = Credentials.Default;
            }
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = UseCredentials == Credentials.Default
            };
            var client = new HttpClient(handler, true) { MaxResponseContentBufferSize = 256000 };

            if (UseCredentials == Credentials.Basic)
            {
                if ((string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password)))
                {
                    throw new InvalidOperationException("Basic Authentication required.");
                }
                var byteArray = Encoding.ASCII.GetBytes(string.Concat(UserName, ":", Password));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            else if (UseCredentials == Credentials.OAuth)
            {
                if (string.IsNullOrWhiteSpace(AccessToken))
                {
                    throw new InvalidOperationException("Access token required.");
                }
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            }

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ContentType));

            foreach (var header in _headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            Task<HttpResponseMessage> message = null;

            switch (Method)
            {
                case MethodType.Get:
                    message = client.GetAsync(uri);
                    break;
                case MethodType.Post:
                    message = client.PostAsync(uri, content);
                    break;
                case MethodType.Put:
                    message = client.PutAsync(uri, content);
                    break;
                case MethodType.Patch:
                    message = client.PatchAsync(uri, content);
                    break;
                case MethodType.Delete:
                    message = client.DeleteAsync(uri);
                    break;
            }

            return await message;
        }
    }
}
