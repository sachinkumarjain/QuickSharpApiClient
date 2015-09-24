using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickSharpApiClient
{
    public class Program
    {
        static void Main(string[] args)
        {
          var client = new ApiClient("https://api.github.com/users/sachinkumarjain", MethodType.Get);

            //client.UseDefaultCredentials = true;
            //client.UseCredentials = Credentials.Default;
            //client.UseCredentials = Credentials.Basic;
            //client.UserName = "testusername";
            //client.Password = "pwd123";
            //var response = client.SendAsync();
            //Console.WriteLine(response.Result);

            //Headers
            //client.AddHeader("ApiKey", "test key value");
            //client.AddHeader("Authorization", "Basic " + "<username>:<password>");
            //client.AddHeader("Authorization", "Bearer " + "<accessToken>"); 
            //empty request and string response as json
            //var response = client.SendAsync().Result;

            //string request and object response
            //var response = client.SendAsync<string, ResposeMessage>("");

            //get object response
            try
            {
                var response = client.SendAsync();
                Console.WriteLine(response.Result);
            }
            catch (AggregateException ex)
            {
                Console.WriteLine(ex.InnerException.Message.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            Console.ReadLine();
        }
  }
