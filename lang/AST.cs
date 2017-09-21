using lang.Lexing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lang.AST
{
    public enum Precedence
    {
        LOWEST,
        EQUALS,      // == 
        LESSGREATER, // < or >
        SUM,         // +
        PRODUCT,     // *
        PREFIX,      // -var or !var
        CALL,        // func(cuck)
        INDEX        // a[index]
    }

    public interface INode
    {
        string TokenLiteral();
        string ToString();
    }

    public interface IStatement : INode
    {
        void StatementNode();
    }

    public interface IExpression : INode
    {
        void ExpressionNode();
    }

    public class Program : INode
    {
        public List<IStatement> Statements { get; set; }

        public string TokenLiteral()
        {
            if (this.Statements.Count > 0)
                return this.Statements[0].TokenLiteral();
            else
                return String.Empty;
        }

        public override string ToString()
            => String.Join(", ", Statements.Select(o => o.ToString()).ToList());
    }

    public class VarStatement : IStatement
    {
        public Token Token { get; set; }
        public Identifier Name { get; set; }
        public IExpression Value { get; set; }

        public void StatementNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
        {
            var res = $"{TokenLiteral()} {Name.ToString()} = ";
            if (Value != null)
                res += Value.ToString();
            res += ";";
            return res;
        }
    }

    public class ReturnStatement : IStatement
    {
        public Token Token { get; set; }
        public IExpression ReturnValue { get; set; }

        public void StatementNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
        {
            var res = $"{TokenLiteral()} ";

            if (ReturnValue != null)
                res += ReturnValue.ToString();
            res += ";";

            return res;
        }
    }

    public class ExpressionStatement : IStatement
    {
        public Token Token { get; set; }
        public IExpression Expression { get; set; }

        public void StatementNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
        {
            if (Expression != null)
                return Expression.ToString();

            return String.Empty;
        }
    }

    public class Identifier : IExpression
    {
        public Token Token { get; set; }
        public string Value { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
        {
            return this.Token.Literal;
        }

        public override string ToString()
            => Value;
    }

    public class Keyword : IExpression
    {
        public Token Token { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
        {
            return this.Token.Literal;
        }

        public override string ToString()
            => this.Token.Literal;
    }

    public class Boolean : IExpression
    {
        public Token Token { get; set; }
        public bool Value { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
        {
            return this.Token.Literal;
        }

        public override string ToString()
            => this.Token.Literal;
    }

    public class IntegerLiteral : IExpression
    {
        public Token Token { get; set; }
        public int Value { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
        {
            return this.Token.Literal;
        }

        public override string ToString()
            => this.Token.Literal;
    }

    public class StringLiteral : IExpression
    {
        public Token Token { get; set; }
        public string Value { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
        {
            return this.Token.Literal;
        }

        public override string ToString()
            => this.Token.Literal;
    }

    public class PrefixExpression : IExpression
    {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public IExpression Right { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
            => $"({Operator}{Right.ToString()})";
    }

    public class InfixExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Left { get; set; }
        public string Operator { get; set; }
        public IExpression Right { get; set; }

        public void ExpressionNode() { }

        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
            => $"({Left.ToString()}{Operator}{Right.ToString()})";
    }

    public class IfExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Condition { get; set; }
        public BlockStatement Consequence { get; set; }
        public BlockStatement Alternative { get; set; }

        public void ExpressionNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
            => $"if {Condition.ToString()} {Consequence.ToString()}{(Alternative != null ? " else " + Alternative.ToString():"")}";
    }

    public class BlockStatement : IStatement
    {
        public Token Token { get; set; }
        public List<IStatement> Statements { get; set; }

        public void StatementNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
            => String.Join(", ", Statements.Select(o => o.ToString()).ToList());
    }

    public class FunctionLiteral : IExpression
    {
        public Token Token { get; set; }
        public List<Identifier> Parameters { get; set; }
        public BlockStatement Body { get; set; }

        public void ExpressionNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
        {
            var param = String.Join(",", Parameters.Select(o => o.ToString()).ToList());
            return $"{TokenLiteral()}({param}) {Body.ToString()}";
        }
    }

    public class LoopStatement : IExpression
    {
        public Token Token { get; set; }
        public Identifier LoopVariable { get; set; }
        public Identifier Target { get; set; }
        public BlockStatement Body { get; set; }

        public void ExpressionNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
        {
            return $"for({LoopVariable.ToString()} in {Target.ToString()}) {{\n{Body.ToString()}\n}}";
        }
    }

    public class ArrayLiteral : IExpression
    {
        public Token Token { get; set; }
        public List<IExpression> Elements { get; set; }

        public void ExpressionNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString() =>
            String.Join(",", Elements.Select(o => o.ToString()));
    }

    public class CallExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Function { get; set; }
        public List<IExpression> Arguments { get; set; }

        public void ExpressionNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString()
        {
            var param = String.Join(",", Arguments.Select(o => o.ToString()).ToList());
            return $"{Function.ToString()}({param})";
        }
    }

    public class IndexExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Left { get; set; }
        public IExpression Index { get; set; }

        public void ExpressionNode() { }
        public string TokenLiteral()
            => this.Token.Literal;

        public override string ToString() =>
            $"({Left.ToString()}[{Index.ToString()}]";
    }
}
