
using QuickSharpApiClient.Common.Enums;
using QuickSharpApiClient.Feed.Files;
using System;

namespace feed
{
    public class Program
    {
        static void Main(string[] args)
        {
            var access = new FileAccess();
            string request = string.Empty;
            //Sample

            if (args.Length > 0)
            {
                access.ApiUri = new Uri(args[0]);
            }
            if (args.Length > 1)
            {
                access.Method = (MethodType)Enum.Parse(typeof(MethodType), args[1]);
            }
            if (args.Length > 2)
            {
                access.BaseDirectory = args[2];
            }
            if (args.Length > 3)
            {
                request = args[3];
            }

            if (string.IsNullOrWhiteSpace(access.ApiUri.OriginalString))
            {
                Console.WriteLine("API Uri cannot be empty.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Set API Url: " + access.ApiUri);

            Console.WriteLine("Set API Method (Get|Post|Put|Patch|Delete): " + access.Method);

            Console.WriteLine("Set Base Directory: " + access.BaseDirectory);

            Console.WriteLine("Set API Request: " + request);
            try
            {
                var resp = access.Read(request);
                Console.WriteLine("Get API Response: " + resp.Result);
            }
            catch (Exception)
            {
                Console.ReadLine();
                throw;
            }

            Console.WriteLine("Do you want to overite the response? ");
            var response = Console.ReadLine();

#if DEBUG
        response =  "{\"login\":\"kumarsachinjain\", \"id\":14809634}"; 
#endif

            if (!string.IsNullOrWhiteSpace(response))
            {
                FileHelper.ReplaceResponse(access.FileName, request, response);
                Console.WriteLine("API Response has been modified.");
            }
            Console.ReadKey();
        }
    }
}
