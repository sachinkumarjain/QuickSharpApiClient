using System;
using System.IO;
using System.Threading.Tasks;

namespace QuickSharpApiClient
{
    public class FileHelper
    {
        public static string CreateTempFile(Uri uri, MethodType method)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), ReplaceSpecialChars(uri.Host));

            if (!Directory.Exists(tempPath))
            {
                try
                {
                    var dir = new DirectoryInfo(tempPath);
                    dir.Create();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Unable to temp file path to storage for temp api", ex);
                }
            }

            var fileName = Path.Combine(tempPath, string.Concat(ReplaceSpecialChars(uri.AbsolutePath), "_", method.ToString(), ".temp.apix"));

            if (!File.Exists(fileName))
            {
                try
                {
                    System.IO.FileStream s = System.IO.File.Create(fileName);
                    s.Close();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Unable to create file to storage for temp api", ex);
                }
            }

            return fileName;

        }

        public static async Task<string> ReadResponse(string filename)
        {
            using (var r = new StreamReader(filename))
            {
                var task = r.ReadToEndAsync();
                return await task;
            }
        }

        public static async Task<string> ReadResponse(string filename, string request)
        {
            using (var r = new StreamReader(filename))
            {
                var task = r.ReadToEndAsync();
                return await task;
            }
        }

        public static async void SaveResponse(string filename, Task<string> response)
        {
            using (var w = File.AppendText(filename))
            {
                w.Write(await response);
            }
        }

        public static async void SaveResponse(string filename, string request, Task<string> response)
        {
            using (var w = File.AppendText(filename))
            {
                w.Write(PrepareFakeResponseToSave(request, await response));
            }
        }

        private static string PrepareFakeResponseToSave(string request, string response)
        {
            return string.Concat("{\"request\": \"", request, "\", \"response\": \"", response, "\"}");
        }

        private static string PrepareResponseToRead(string request, string fakeResponse)
        {
            var fake = ApiClient.ConvertFromJson<FakeApiData>(fakeResponse);
            return request.Equals(fake.Request) ? fake.Response : string.Empty;
        }

        private static string ReplaceSpecialChars(string input)
        {
            return input.Replace("/", "_").Replace(":", "__");
        }
    }

    public class FakeApiData
    {
        public string Request { get; set; }
        public string Response { get; set; }
    }
}
