using System.Linq;
using System.Reflection;
using System.Text;

public static partial class Extensions
{
    public static dynamic AsDynamic(this object obj)
    {
        dynamic d = obj;
        return d;
    }
}