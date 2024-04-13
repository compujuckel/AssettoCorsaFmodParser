using System.Text;

namespace FmodParser;

[AttributeUsage(AttributeTargets.Class)]
public class DataChunkAttribute : Attribute
{
    public byte[] Identifier { get; }

    public DataChunkAttribute(string identifier)
    {
        Identifier = Encoding.ASCII.GetBytes(identifier);
    }
}