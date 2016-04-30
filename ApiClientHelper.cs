using System;
using System.Net.Configuration;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace QuickSharpApiClient
{
    public static class ApiClientHelper
    {
        // Enable/disable useUnsafeHeaderParsing.
        public static bool AllowUnsafeHeaderParsing(bool enable)
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

        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, Uri uri, HttpContent content) 
        { 
             HttpRequestMessage request = new HttpRequestMessage 
             { 
                 Method = new HttpMethod("PATCH"), 
                 RequestUri = uri, 
                 Content = content, 
             }; 

             return client.SendAsync(request); 
         } 

    }
}
