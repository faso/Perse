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

        public static BuiltIntFunction Puts = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            Console.WriteLine(o[0].Inspect());

            return null;
        };

        public static BuiltIntFunction Read = o =>
        {
            if (o.Length != 0)
                return new LangError($"Wrong number of arguments. Expected 0, got {o.Length}");

            var input = Console.ReadLine();

            return new LangString() { Value = input };
        };

        public static BuiltIntFunction Last = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first'");

            return (o[0] as LangArray).Elements.Last();
        };

        public static BuiltIntFunction ParseInt = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangString))
                return new LangError($"Wrong argument type for 'first', expected string");

            int output;
            if (Int32.TryParse(((LangString)o[0]).Value, out output))
            {
                return new LangInteger() { Value = output };
            }

            return new LangError("Could not parse integer");
        };

        public static BuiltIntFunction ListLength = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first', expected array");

            return new LangInteger() { Value = ((LangArray)o[0]).Elements.Count };
        };

        public static BuiltIntFunction ListReverse = o =>
        {
            if (o.Length != 1)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first', expected array");

            return new LangArray() { Elements = ((LangArray)o[0]).Elements.Select(x => x).Reverse().ToList() };
        };

        public static BuiltIntFunction ListConcat = o =>
        {
            if (o.Length != 2)
                return new LangError($"Wrong number of arguments. Expected 2, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first', expected array");
            if (!(o[1] is LangArray))
                return new LangError($"Wrong argument type for 'second', expected array");

            return new LangArray() { Elements = ((LangArray)o[0]).Elements.Concat(((LangArray)o[1]).Elements).ToList() };
        };

        public static BuiltIntFunction ListPush = o =>
        {
            if (o.Length != 2)
                return new LangError($"Wrong number of arguments. Expected 2, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first', expected array");

            var newList = ((LangArray)o[0]).Elements.Select(x => x).ToList();
            newList.Add(o[1]);

            return new LangArray() { Elements = newList };
        };

        public static BuiltIntFunction Part = o =>
        {
            if (o.Length != 2)
                return new LangError($"Wrong number of arguments. Expected 1, got {o.Length}");

            if (!(o[0] is LangArray))
                return new LangError($"Wrong argument type for 'first'");

            if (!(o[1] is LangFunction))
                return new LangError($"Wrong argument type for 'second'");

            var ev = new Evaluator();
            var first = new LangArray()
            {
                Elements = new List<ILangObject>()
            };

            var second = new LangArray()
            {
                Elements = new List<ILangObject>()
            };

            foreach (var el in (o[0] as LangArray).Elements)
            {
                var res = ev.ApplyFunction((o[1] as LangFunction), new List<ILangObject>() { el });
                if (!(res is LangBoolean))
                    return new LangError("Invalid return type of the filter function!");

                var bres = (res as LangBoolean).Value;

                if (bres)
                    first.Elements.Add(el);
                else
                    second.Elements.Add(el);
            }

            return new LangArray()
            {
                Elements = new List<ILangObject>()
                {
                    first,
                    second
                }
            };
        };

        public static BuiltIntFunction Concat = o => new LangString() { Value = (o[0] as LangString).Value + (o[1] as LangString).Value };

        public static Dictionary<string, ILangObject> Builtins = new Dictionary<string, ILangObject>()
        {
            { "str.length", new Builtin() { Fn = Len } },
            { "str.concat", new Builtin() { Fn = Concat } },
            { "list.first", new Builtin() { Fn = First } },
            { "list.last", new Builtin() { Fn = Last } },
            { "list.length", new Builtin() { Fn = ListLength } },
            { "list.reverse", new Builtin() { Fn = ListReverse } },
            { "list.concat", new Builtin() { Fn = ListConcat } },
            { "list.push", new Builtin() { Fn = ListPush } },
            { "puts", new Builtin() { Fn = Puts } },
            { "list.part", new Builtin() { Fn = Part } },
            { "read", new Builtin() { Fn = Read } },
            { "int.parse", new Builtin() { Fn = ParseInt } }
        };
    }
}
