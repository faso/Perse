using Lang.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuiltIntFunction = System.Func<Lang.Objects.ILangObject[], Lang.Objects.ILangObject>;

namespace Lang.Builtins
{
    public static class BuiltinFunctions
    {
        public static BuiltIntFunction Len = o => 
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangString) && !(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'len'");

            if (o[0] is LangString)
                return new LangInteger() { Value = (o[0] as LangString).Value.Length };
            else
            {
                return new LangInteger() { Value = (o[0] as LangArray).Elements.Count };
            }
        };

        public static BuiltIntFunction First = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first'");

            return (o[0] as LangArray).Elements.First();
        };

        public static BuiltIntFunction Last = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first'");

            return (o[0] as LangArray).Elements.Last();
        };

        public static BuiltIntFunction Concat = o => new LangString() { Value = (o[0] as LangString).Value + (o[1] as LangString).Value };


        public static Dictionary<string, ILangObject> Builtins = new Dictionary<string, ILangObject>()
        {
            { "length", new Builtin() { Fn = Len } },
            { "concat", new Builtin() { Fn = Concat } },
            { "first", new Builtin() { Fn = First } },
            { "last", new Builtin() { Fn = Last } }
        };
    }
}
