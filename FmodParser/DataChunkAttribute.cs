using System.Text;
using JetBrains.Annotations;

namespace FmodParser;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class DataChunkAttribute : Attribute
{
    public byte[] Identifier { get; }

    public DataChunkAttribute(string identifier)
    {
        Identifier = Encoding.ASCII.GetBytes(identifier);
    }
}