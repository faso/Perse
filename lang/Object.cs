﻿using Lang.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang.Objects
{
    public enum ObjectType
    {
        INTEGER_OBJ,
        BOOLEAN_OBJ,
        NULL_OBJ,
        RETURN_VALUE_OBJ,
        ERROR_OBJ,
        FUNCTION
    }

    public interface ILangObject
    {
        ObjectType Type();
        string Inspect();
    }

    public class LangInteger : ILangObject
    {
        public long Value { get; set; }

        public string Inspect()
            => Value.ToString();

        public ObjectType Type()
            => ObjectType.INTEGER_OBJ;
    }

    public class LangBoolean : ILangObject
    {
        public bool Value { get; set; }

        public string Inspect()
            => Value.ToString();

        public ObjectType Type()
            => ObjectType.BOOLEAN_OBJ;
    }

    public class LangNull : ILangObject
    {
        public string Inspect()
            => "null";

        public ObjectType Type()
            => ObjectType.NULL_OBJ;
    }

    public class ReturnValue : ILangObject
    {
        public ILangObject Value { get; set; }

        public string Inspect()
            => Value.Inspect();

        public ObjectType Type()
            => ObjectType.RETURN_VALUE_OBJ;
    }

    public class LangError : ILangObject
    {
        public LangError(string msg)
        {
            Message = msg;
        }

        public string Message { get; set; }

        public string Inspect()
            => $"ERROR: {Message}";

        public ObjectType Type()
            => ObjectType.ERROR_OBJ;
    }

    public class Environment
    {
        public Environment Outer { get; set; }

        public Environment()
        {
            Store = new Dictionary<string, ILangObject>();
            Outer = null;
        }

        public Environment(Environment env)
        {
            Store = new Dictionary<string, ILangObject>();
            Outer = env;
        }

        public Dictionary<string, ILangObject> Store { get; set; }

        public ILangObject Get(string name)
        {
            if (Store.ContainsKey(name))
                return Store[name];
            else
            {
                if (Outer != null)
                    return Outer.Get(name);
            }

            return null;
        }

        public void Set(string name, ILangObject val)
        {
            Store[name] = val;
        }
    }

    public class LangFunction : ILangObject
    {
        public List<Identifier> Parameters { get; set; }
        public BlockStatement Body { get; set; }
        public Environment Env { get; set; }

        public string Inspect()
        {
            var par = Parameters.Select(o => o.ToString()).ToList();
            return $"function{String.Join(",", par)}() {{\n{Body.ToString()}\n}}";
        }

        public ObjectType Type()
            => ObjectType.FUNCTION;
    }
}