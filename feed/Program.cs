
using QuickSharpApiClient;
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

            while (access.ApiUri == null || string.IsNullOrWhiteSpace(access.ApiUri.OriginalString))
            {
                Console.WriteLine("Enter API Url: ");
                try
                {
                    access.ApiUri = new Uri(Console.ReadLine());
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid format of api url.");
                    access.ApiUri = null;
                }

            }

            while (access.Method == null)
            {
                Console.WriteLine("Enter API Method (Get | Post | Put | Patch | Delete): ");
                try
                {
                    access.Method = (MethodType)Enum.Parse(typeof(MethodType), Console.ReadLine(), true);
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid api method.");
                    access.Method = null;
                }

            }

            if (access.IsDefaultTempPath)
            {
                Console.WriteLine("Enter base directory to store or use temp (leave blank): ");
                try
                {
                    access.BaseDirectory = Console.ReadLine();
                    if (!System.IO.Directory.Exists(access.BaseDirectory)) { throw new System.IO.DirectoryNotFoundException(); }
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid path.");
                }

            }

            Console.WriteLine("Enter the request (optional): ");
            request = Console.ReadLine();

            Console.WriteLine("Set API Url: " + access.ApiUri);

            Console.WriteLine("Set API Method (Get|Post|Put|Patch|Delete): " + access.Method);

            Console.WriteLine("Set Base Directory: " + access.BaseDirectory);

            Console.WriteLine("Set API Request: " + request);

            bool HasResponse;
            try
            {
                var resp = access.Read(request);
                Console.WriteLine("Get API Response: " + resp.Result);
                HasResponse = !string.IsNullOrWhiteSpace(resp.Result);
            }
            catch (Exception)
            {
                Console.ReadLine();
                throw;
            }

            if (!HasResponse)
            {
                Console.WriteLine("\n\rNo response found for given api.");
            }

            ///////////////////////////////////////////////////////////////////
            Console.WriteLine("\n\rDo you want to record for this api? (Y | N)");

            if (Console.ReadLine().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Enter the request (optional): ");
                request = Console.ReadLine();

                var vClient = new ApiClient(access) { IsVirualApiEnable = true, ForceToRecord = true };
                var recordResponse = vClient.SendAsync();

                access.Replace(request, recordResponse.Result);

                Console.WriteLine("\n\rResponse is recorded / saved to use: ");
                Console.WriteLine(recordResponse.Result);
            }

            ////////////////////////////////////////////////////////////////////////
            Console.WriteLine("\n\rDo you want to overite the response? (Y | N) ");
            if (Console.ReadLine().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Enter to replace response: ");
                var response = Console.ReadLine();

#if DEBUG
                response = "{\"login\":\"kumarsachinjain\", \"id\":14809634}";
#endif

                if (!string.IsNullOrWhiteSpace(response))
                {
                    access.Replace(request, response);
                    Console.WriteLine("API Response has been modified.");
                }
            }

            ///////////////////////////////////////////////////////////////////////
            Console.WriteLine("\n\rDo you want to integrate with real call? (Y | N) ");
            if (Console.ReadLine().Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                var virtualResponse = access.Read(request);

                var client = new ApiClient(access) { IsVirualApiEnable = false };
                var reatimeResponse = client.SendAsync();


                Console.WriteLine("\n\rRecorded Response: " + virtualResponse.Result);
                Console.WriteLine("\n\rReal-time Response: " + reatimeResponse.Result);

                var compare = string.Compare(reatimeResponse.Result, virtualResponse.Result, true);

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Result: " + (compare == 0 ? "Both result are equal" : "Found some differences"));
                Console.ResetColor();
            }

            Console.ReadKey();
        }
    }
}
