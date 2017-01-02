using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lang.Lexing
{ 
    public enum TokenType
    {
        // Special
        ILLEGAL,
        EOF,

        // Identifiers and literals
        IDENT,
        INT,

        // Operators
        ASSIGN,
        PLUS,
        MINUS,
        SLASH,
        ASTERISK,

        // Comparers
        LESSTHAN,
        GREATERTHAN,
        BANG,
        EQ,
        NOT_EQ,

        // Delimiters
        COMMA,
        SEMICOLON,

        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,

        // Keywords
        FUNCTION,
        VAR,
        TRUE,
        FALSE,
        IF,
        ELSE,
        RETURN,

        // Loops
        WHILE
    }

    public class Token
    {
        public TokenType Type { get; set; }
        public string Literal { get; set; }

        public Token(TokenType type, string literal)
        {
            this.Type = type;
            this.Literal = literal;
        }

        public Token(TokenType type, char literal)
        {
            this.Type = type;
            this.Literal = literal.ToString();
        }

        public Token()
        {
            this.Type = 0;
            this.Literal = String.Empty;
        }
    }
}
