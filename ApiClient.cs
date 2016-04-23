using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace QuickSharpApiClient
{
    public enum MethodType
    {
        Get,
        Post,
        Put,
        Delete
    }

    public enum HttpContentType
    {
        Json,
        Xml,
        Text
    }

    public enum Credentials
    {
        None = 0,
        Default = 1,
        Basic = 2,
        OAuth = 3
    }

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
        public MethodType Method { get; private set; }
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

        public ApiClient(string uri, MethodType method)
        {
            MaxResponseContentBufferSize = 256000;
            DefaultContentType = HttpContentType.Json;
            Method = method;
            ApiUrl = uri;
            this.AddHeader("User-Agent", "Mozilla /5.0 (Compatible MSIE 9.0;Windows NT 6.1;WOW64; Trident/5.0)");
        }

        public Task<string> SendAsync(string contentRequest = "")
        {
            try
            {
                var uri = new Uri(ApiUrl);
                Task<string> resultAsync;

                string filename = null;
                if (IsVirualApiEnable)
                {
                    //Create and get file name 
                    filename = FileHelper.CreateTempFile(uri, MethodType.Get);
                    Debug.Print("fake file path: " + filename);

                    var fakeResponse = FileHelper.ReadResponse(filename, contentRequest);

                    if (!string.IsNullOrWhiteSpace(fakeResponse.Result))
                    {
                        Debug.Print("fakeResponse: " + fakeResponse.Result);
                        return fakeResponse;
                    }
                }

                var httpContentRequest = new StringContent(contentRequest, System.Text.Encoding.UTF8, ContentType);

                AllowUnsafeHeaderParsing(true);
                var response = SendAsync(uri, httpContentRequest, Method);
                AllowUnsafeHeaderParsing(false);

                resultAsync = response.Result.Content.ReadAsStringAsync();

                //save fake response
                if (IsVirualApiEnable && !string.IsNullOrWhiteSpace(filename))
                {
                    FileHelper.SaveResponse(filename, string.Empty, resultAsync);
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
            var contentRequest = ConvertToJson(request);
            var response = SendAsync(contentRequest);
            return ConvertFromJson<TResponse>(response.Result);
        }

        public TResponse SendAsync<TResponse>()
        {
            var response = SendAsync();
            return ConvertFromJson<TResponse>(response.Result);
        }

        private async Task<HttpResponseMessage> SendAsync(Uri uri, HttpContent content, MethodType method)
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
                case MethodType.Delete:
                    message = client.DeleteAsync(uri);
                    break;
            }

            return await message;
        }

        public static string ConvertToJson<TRequest>(TRequest request)
        {
            var serializer = new JavaScriptSerializer();
            var result = serializer.Serialize(request);
            return result;
        }

        public static TResponse ConvertFromJson<TResponse>(string request)
        {
            var serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<TResponse>(request);
            return result;
        }

        // Enable/disable useUnsafeHeaderParsing.
        private static bool AllowUnsafeHeaderParsing(bool enable)
        {
            //Get the assembly that contains the internal class
            var assembly = Assembly.GetAssembly(typeof(SettingsSection));
            if (assembly != null)
            {
                //Use the assembly in order to get the internal type for the internal class
                Type settingsSectionType = assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
                if (settingsSectionType != null)
                {
                    //Use the internal static property to get an instance of the internal settings class.
                    //If the static instance isn't created already invoking the property will create it for us.
                    object anInstance = settingsSectionType.InvokeMember("Section",
                    BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
                    if (anInstance != null)
                    {
                        //Locate the private bool field that tells the framework if unsafe header parsing is allowed
                        FieldInfo aUseUnsafeHeaderParsing = settingsSectionType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (aUseUnsafeHeaderParsing != null)
                        {
                            aUseUnsafeHeaderParsing.SetValue(anInstance, enable);
                            return true;
                        }

                    }
                }
            }
            return false;
        }
    }
}
