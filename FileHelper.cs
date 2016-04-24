﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace QuickSharpApiClient
{
    public static class FileHelper
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

        public static async Task<string> ReadResponse(string filename, string request)
        {
            string nextline;
            string result = string.Empty;
            using (var r = new StreamReader(filename))
            {
                while((nextline = await r.ReadLineAsync()) != null)
                {
                    var respo = ReadReponseFromRequest(request, nextline);
                    if (!string.IsNullOrWhiteSpace(respo)) { result = respo; break; }
                }
                return result.Decode();
            }
        }

        private static string ReadReponseFromRequest(string request, string nextline) 
        {
            if (string.IsNullOrWhiteSpace(nextline)) { return string.Empty; }
            var fake = nextline.FromJson<FakeApiData>();
            return request.Equals(fake.Request.Decode(), StringComparison.OrdinalIgnoreCase) ? fake.Response : string.Empty;
        }

        public static async void SaveResponse(string filename, string request, Task<string> response)
        {
            using (var w = File.AppendText(filename))
            {
                await w.WriteLineAsync(PrepareFakeResponseToSave(request, await response));
            }
        }

        private static string PrepareFakeResponseToSave(string request, string response)
        {
            var respo = string.Concat("{\"request\": \"", request.Encode(), "\", \"response\": \"", response.Encode(), "\"}");
            return respo;
        }

        //private static string PrepareResponseToRead(string request, string fakeResponse)
        //{
        //    var fake = ApiClient.ConvertFromJson<FakeApiData>(fakeResponse);
        //    return request.Equals(fake.Request) ? fake.Response : string.Empty;
        //}

        private static string ReplaceSpecialChars(string input)
        {
            return input.Replace("/", "_").Replace(":", "__");
        }

        public static string Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }

    public class FakeApiData
    {
        public string Request { get; set; }
        public string Response { get; set; }
    }
}
