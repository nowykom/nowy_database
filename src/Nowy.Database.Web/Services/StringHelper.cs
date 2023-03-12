namespace Nowy.Database.Web.Services;

internal static class StringHelper
{
    public static string MakeRandomUuid()
    {
        return Guid.NewGuid().ToString("D");
    }
}
