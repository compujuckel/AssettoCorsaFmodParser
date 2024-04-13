namespace FmodParser.Utils;

public static class GuidExtensions
{
    public static string ToKnownString(this Guid guid)
    {
        if (GuidCache.KnownGuids.TryGetValue(guid, out var name))
        {
            return $"{guid} => {name}";
        }

        return guid.ToString();
    }
}