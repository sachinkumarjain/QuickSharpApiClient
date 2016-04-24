using System.Web.Script.Serialization;

namespace QuickSharpApiClient
{
    public static class JsonHelper
    {
        public static string ToJson<TRequest>(this TRequest request)
        {
            var serializer = new JavaScriptSerializer();
            var result = serializer.Serialize(request);
            return result;
        }

        public static TResponse FromJson<TResponse>(this string request)
        {
            var serializer = new JavaScriptSerializer();
            var result = serializer.Deserialize<TResponse>(request);
            return result;
        }
    }
}
