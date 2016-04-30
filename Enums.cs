
namespace QuickSharpApiClient
{
    public enum MethodType
    {
        Get,
        Post,
        Put,
        Patch,
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
}
