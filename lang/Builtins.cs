using Lang.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang.Builtins
{
    public static class BuiltinFunctions
    {
        public static Func<ILangObject[], ILangObject> Len = o => 
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangString))
                return new LangError($"Wrong argument type for 'len'");

            return new LangInteger() { Value = (o[0] as LangString).Value.Length };
        };

        public static Func<ILangObject[], ILangObject> Concat = o => new LangString() { Value = (o[0] as LangString).Value + (o[1] as LangString).Value };


        public static Dictionary<string, ILangObject> Builtins = new Dictionary<string, ILangObject>()
        {
            { "len", new Builtin() { Fn = Len } },
            { "concat", new Builtin() { Fn = Concat } }
        };
    }
}
