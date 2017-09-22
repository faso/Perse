using Lang.AST;
using Lang.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang
{
    public class Evaluator
    {
        private readonly LangBoolean TRUE = new LangBoolean() { Value = true };
        private readonly LangBoolean FALSE = new LangBoolean() { Value = false };
        private readonly LangNull NULL = new LangNull();

        public ILangObject Eval(INode node, Objects.Environment env)
        {
            if (node is IntegerLiteral)
                return new LangInteger() { Value = (node as IntegerLiteral).Value };
            else if (node is Program)
                return EvalProgram((node as Program).Statements, env);
            else if (node is ExpressionStatement)
                return Eval((node as ExpressionStatement).Expression, env);
            else if (node is Lang.AST.Boolean)
                return BoolToLangBool((node as AST.Boolean).Value);
            else if (node is PrefixExpression)
            {
                var n = (node as PrefixExpression);
                var right = Eval(n.Right, env);

                if (IsError(right))
                    return right;

                return EvalPrefixExpression(n.Operator, right);
            }
            else if (node is ArrayLiteral)
            {
                var n = (node as ArrayLiteral);
                var elements = EvalExpressions(n.Elements, env);
                if (elements.Count == 1 && IsError(elements[0]))
                {
                    return elements[0];
                }

                return new LangArray() { Elements = elements };
            }
            else if (node is InfixExpression)
            {
                var n = (node as InfixExpression);
                var right = Eval(n.Right, env);
                var left = Eval(n.Left, env);

                if (IsError(left))
                    return left;

                if (IsError(right))
                    return right;

                return EvalInfixExpression(n.Operator, left, right);
            }
            else if (node is BlockStatement)
                return EvalBlockStatement((node as BlockStatement), env);
            else if (node is LoopStatement)
                return EvalLoopStatement((node as LoopStatement), env);
            else if (node is IfExpression)
                return EvalIfExpression((node as IfExpression), env);
            else if (node is ReturnStatement)
            {
                var val = Eval((node as ReturnStatement).ReturnValue, env);

                if (IsError(val))
                    return val;

                return new ReturnValue() { Value = val };
            }
            else if (node is VarStatement)
            {
                var val = Eval((node as VarStatement).Value, env);

                if (IsError(val))
                    return val;

                env.Set((node as VarStatement).Name.Value, val);
            }
            else if (node is AssignStatement)
            {
                var val = Eval((node as AssignStatement).Value, env);

                if (IsError(val))
                    return val;

                env.Set((node as AssignStatement).Name.Value, val);
            }
            else if (node is Identifier)
            {
                return EvalIdentifier((node as Identifier), env);
            }
            else if (node is FunctionLiteral)
            {
                var n = node as FunctionLiteral;
                return new LangFunction()
                {
                    Parameters = n.Parameters,
                    Env = env,
                    Body = n.Body
                };
            }
            else if (node is IndexExpression)
            {
                var n = node as IndexExpression;
                var left = Eval(n.Left, env);
                if (IsError(left))
                    return left;

                var index = Eval(n.Index, env);
                if (IsError(index))
                    return index;

                return EvalIndexExpression(left, index);
            }
            else if (node is CallExpression)
            {
                var function = Eval((node as CallExpression).Function, env);
                if (IsError(function))
                    return function;

                var args = EvalExpressions((node as CallExpression).Arguments, env);
                if (args.Count == 1 && IsError(args.First()))
                    return args.First();

                return ApplyFunction(function, args);
            }
            else if (node is StringLiteral)
                return new LangString() { Value = (node as StringLiteral).Value };

            return null;
        }

        public ILangObject ApplyFunction(ILangObject fn, List<ILangObject> args)
        {
            if (fn is LangFunction)
            {
                var function = fn as LangFunction;
                var extendedEnv = ExtendFunctionEnv(function, args);
                var evaluated = Eval(function.Body, extendedEnv);
                return UnwrapReturnValue(evaluated);
            }
            else if (fn is Builtin)
            {
                var function = fn as Builtin;
                var boink = function.Fn.Invoke(args.ToArray());
                return boink as ILangObject;
            }

            return new LangError($"Not a function: {fn.Type()}");
        }

        private List<ILangObject> EvalExpressions(List<IExpression> exps, Objects.Environment env)
        {
            var result = new List<ILangObject>();

            foreach (var e in exps)
            {
                var evaluated = Eval(e, env);
                if (IsError(evaluated))
                    return new List<ILangObject>() { evaluated };

                result.Add(evaluated);
            }

            return result;
        }

        private ILangObject EvalIdentifier(Identifier node, Objects.Environment env)
        {
            var val = env.Get(node.Value);

            if (val == null)
                val = Builtins.BuiltinFunctions.Builtins[node.Value];

            if (val == null)
                return new LangError($"Identifier not found: {node.Value}");

            return val;
        }

        private ILangObject EvalProgram(List<IStatement> stmts, Objects.Environment env)
        {
            ILangObject result = new LangNull();

            foreach (var statement in stmts)
            {
                result = Eval(statement, env);

                if (result is ReturnValue)
                    return (result as ReturnValue).Value;
                else if (result is LangError)
                    return (result as LangError);
            }

            return result;
        }

        private ILangObject EvalIndexExpression(ILangObject left, ILangObject index)
        {
            if (left.Type() == ObjectType.ARRAY_OBJ && index.Type() == ObjectType.INTEGER_OBJ)
            {
                return EvalArrayIndexExpression(left, index);
            }
            else
            {
                return new LangError($"index operator not supported for {left.Type()}");
            }
        }

        private ILangObject EvalArrayIndexExpression(ILangObject array, ILangObject index)
        {
            var arrayObject = array as LangArray;
            var idx = (index as LangInteger).Value;
            var max = arrayObject.Elements.Count - 1;

            if (idx < 0 || idx > max)
                return null;

            return arrayObject.Elements.ElementAt(idx);
        }

        private ILangObject EvalBlockStatement(BlockStatement block, Objects.Environment env)
        {
            ILangObject result = new LangNull();

            foreach (var statement in block.Statements)
            {
                result = Eval(statement, env);

                if (result != null)
                {
                    var rt = result.Type();
                    if (rt == ObjectType.RETURN_VALUE_OBJ || rt == ObjectType.ERROR_OBJ)
                        return result;
                }
            }

            return result;
        }

        private ILangObject EvalLoopStatement(LoopStatement loop, Objects.Environment env)
        {
            ILangObject result = new LangNull();

            var target = env.Get(loop.Target.ToString());
            if (target.Type() != ObjectType.ARRAY_OBJ)
                return new LangError("Can't iterate over an object that is not an array (for now)");

            if (env.Get(loop.LoopVariable.ToString()) != null)
                return new LangError("Loop variable identifier already in use");

            int i = 0;
            foreach (var item in ((LangArray)target).Elements)
            {
                if (loop.LoopIndex != null)
                    env.Set(loop.LoopIndex.ToString(), new LangInteger() { Value = i });
                env.Set(loop.LoopVariable.ToString(), item);
                result = EvalBlockStatement(loop.Body, env);
                i++;
            }

            return null;
        }

        private ILangObject EvalInfixExpression(string oper, ILangObject left, ILangObject right)
        {
            if (left.Type() == ObjectType.INTEGER_OBJ && right.Type() == ObjectType.INTEGER_OBJ)
                return EvalIntegerInfixExpression(oper, (left as LangInteger), (right as LangInteger));

            if (left.Type() == ObjectType.STRING_OBJ && right.Type() == ObjectType.STRING_OBJ)
                return EvalStringInfixExpression(oper, (left as LangString), (right as LangString));

            if (left.Type() == ObjectType.ARRAY_OBJ && right.Type() == ObjectType.ARRAY_OBJ)
                return EvalArrayInfixExpression(oper, (left as LangArray), (right as LangArray));

            switch (oper)
            {
                case "==":
                    return BoolToLangBool(left == right);
                case "!=":
                    return BoolToLangBool(left != right);
                default:
                    if (left.Type() != right.Type())
                        return new LangError($"Type mismatch: {left.Type()} {oper} {right.Type()}");
                    else
                        return new LangError($"Unknown operator: {left.Type()} {oper} {right.Type()}");
            }
        }

        private ILangObject EvalStringInfixExpression(string oper, LangString left, LangString right)
        {
            var leftVal = left.Value;
            var rightVal = right.Value;

            switch (oper)
            {
                case "+":
                    return new LangString() { Value = leftVal + rightVal };
                case "==":
                    return BoolToLangBool(leftVal == rightVal);
                default:
                    return new LangError($"Unknown operator: {left.Type()} {oper} {right.Type()}");
            }
        }

        private ILangObject EvalArrayInfixExpression(string oper, LangArray left, LangArray right)
        {
            var leftVal = left.Elements;
            var rightVal = right.Elements;

            switch (oper)
            {
                case "+":
                    return new LangArray() { Elements = leftVal.Concat(rightVal).ToList() };
                case "==":
                    {
                        bool res = true;
                        int i = 0;
                        foreach (var item in leftVal)
                        {
                            var eq = (LangBoolean)EvalInfixExpression("==", item, rightVal.ElementAt(i));
                            if (eq.Value == false)
                            {
                                res = false;
                                break;
                            }
                            i++;
                        }

                        return BoolToLangBool(res);
                    }
                default:
                    return new LangError($"Unknown operator: {left.Type()} {oper} {right.Type()}");
            }
        }

        private ILangObject EvalIntegerInfixExpression(string oper, LangInteger left, LangInteger right)
        {
            var leftVal = left.Value;
            var rightVal = right.Value;

            switch (oper)
            {
                case "+":
                    return new LangInteger() { Value = leftVal + rightVal };
                case "-":
                    return new LangInteger() { Value = leftVal - rightVal };
                case "*":
                    return new LangInteger() { Value = leftVal * rightVal };
                case "/":
                    return new LangInteger() { Value = leftVal / rightVal };
                case "<":
                    return BoolToLangBool(leftVal < rightVal);
                case ">":
                    return BoolToLangBool(leftVal > rightVal);
                case "==":
                    return BoolToLangBool(leftVal == rightVal);
                case "!=":
                    return BoolToLangBool(leftVal != rightVal);
                default:
                    return new LangError($"Unknown operator: {left.Type()} {oper} {right.Type()}");
            }
        }

        private ILangObject EvalPrefixExpression(string oper, ILangObject right)
        {
            switch (oper)
            {
                case "!":
                    return EvalBangOperatorExpression(right);
                case "-":
                    return EvalMinusPrefixOperatorExpression(right);
                default:
                    return new LangError($"Unknown operator: {oper}{right.Type()}");
            }
        }

        private ILangObject EvalMinusPrefixOperatorExpression(ILangObject right)
        {
            if (right.Type() != ObjectType.INTEGER_OBJ)
                return new LangError($"Unknown operator: {right.Type()}");

            var value = (right as LangInteger).Value;
            return new LangInteger() { Value = -value };
        }

        private ILangObject EvalBangOperatorExpression(ILangObject right)
        {
            if (right == TRUE)
                return FALSE;
            else if (right == FALSE)
                return TRUE;
            else if (right == NULL)
                return TRUE;
            else
                return FALSE;
        }

        private ILangObject EvalStatements(List<IStatement> stmts, Objects.Environment env)
        {
            ILangObject result = new LangNull();

            foreach (var statement in stmts)
            {
                result = Eval(statement, env);

                if (result is ReturnValue)
                    return (result as ReturnValue).Value;
            }

            return result;
        }

        private ILangObject EvalIfExpression(IfExpression ie, Objects.Environment env)
        {
            var condition = Eval(ie.Condition, env);

            if (IsError(condition))
                return condition;

            if (IsTruthy(condition))
                return Eval(ie.Consequence, env);
            else if (ie.Alternative != null)
                return Eval(ie.Alternative, env);
            else
                return null;

        }

        private bool IsTruthy(ILangObject obj)
        {
            if (obj == TRUE)
                return true;
            else if (obj == FALSE || obj == NULL)
                return false;

            return true;
        }

        // Helpers
        private LangBoolean BoolToLangBool(bool input)
        {
            if (input)
                return TRUE;
            return FALSE;
        }

        private bool IsError(ILangObject obj)
        {
            if (obj != null)
                return obj.Type() == ObjectType.ERROR_OBJ;

            return false;
        }

        private Objects.Environment ExtendFunctionEnv(LangFunction fn, List<ILangObject> args)
        {
            var env = new Objects.Environment(fn.Env);

            var i = 0;
            foreach (var param in fn.Parameters)
            {
                env.Set(param.Value, args.ElementAt(i));
                i++;
            }

            return env;
        }

        private ILangObject UnwrapReturnValue(ILangObject obj)
        {
            if (obj is ReturnValue)
                return (obj as ReturnValue).Value;

            return obj;
        }
    }
}
