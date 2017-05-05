using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lang.Lexing;

namespace lang.Lexing
{
    public class Lexer
    {
        public string input;
        public int position = 0;
        public int readPosition = 0;
        public char ch;

        private Dictionary<string, TokenType> Keywords = new Dictionary<string, TokenType>()
        {
            { "fn", TokenType.FUNCTION },
            { "var", TokenType.VAR },
            { "true", TokenType.TRUE },
            { "false", TokenType.FALSE },
            { "if", TokenType.IF },
            { "else", TokenType.ELSE },
            { "return", TokenType.RETURN },
            { "while", TokenType.WHILE },
        };

        private TokenType LookupIdentifier(string ident)
            => Keywords.Keys.Contains(ident) ? Keywords[ident] : TokenType.IDENT;

        private bool IsLetter(char ch)
            => (('a' <= ch && ch <= 'z') || ('A' <= ch && ch <= 'Z') || (ch == '_'));

        private bool IsDigit(char cr)
            => '0' <= ch && ch <= '9';

        private void SkipWhitespace()
        {
            while (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r')
                ReadChar();
        }

        private string ReadNumber()
        {
            int i = 0;
            int pos = position;
            while (IsDigit(ch))
            {
                ReadChar();
                i++;
            }

            return input.Substring(pos, i);
        }

        private string ReadIdentifier()
        {
            int i = 0;
            int pos = position;
            while (IsLetter(ch))
            {
                ReadChar();
                i++;
            }

            return input.Substring(pos, i);
        }

        private void ReadChar()
        {
            if (readPosition >= input.Length)
                ch = (char)0;
            else
                ch = input[readPosition];

            position = readPosition;
            readPosition++;
        }

        private char PeekChar()
        {
            if (readPosition >= input.Length)
                return (char)0;
            else
                return input[readPosition];
        }

        public Token NextToken()
        {
            Token tok = new Token();

            SkipWhitespace();

            switch (ch)
            {
                case '=':
                    if (PeekChar() == '=')
                    {
                        char first = ch;
                        ReadChar();
                        tok = new Token(TokenType.EQ, first.ToString() + ch.ToString());
                    }
                    else
                        tok = new Token(TokenType.ASSIGN, ch);
                    break;
                case ';':
                    tok = new Token(TokenType.SEMICOLON, ch);
                    break;
                case '(':
                    tok = new Token(TokenType.LPAREN, ch);
                    break;
                case ')':
                    tok = new Token(TokenType.RPAREN, ch);
                    break;
                case ',':
                    tok = new Token(TokenType.COMMA, ch);
                    break;
                case '+':
                    tok = new Token(TokenType.PLUS, ch);
                    break;
                case '{':
                    tok = new Token(TokenType.LBRACE, ch);
                    break;
                case '}':
                    tok = new Token(TokenType.RBRACE, ch);
                    break;
                case '-':
                    tok = new Token(TokenType.MINUS, ch);
                    break;
                case '/':
                    tok = new Token(TokenType.SLASH, ch);
                    break;
                case '*':
                    tok = new Token(TokenType.ASTERISK, ch);
                    break;
                case '<':
                    tok = new Token(TokenType.LESSTHAN, ch);
                    break;
                case '>':
                    tok = new Token(TokenType.GREATERTHAN, ch);
                    break;
                case '"':
                    tok = new Token(TokenType.STRING, ReadString());
                    break;
                case '[':
                    tok = new Token(TokenType.LBRACKET, ch);
                    break;
                case ']':
                    tok = new Token(TokenType.RBRACKET, ch);
                    break;
                case '!':
                    if (PeekChar() == '=')
                    {
                        char first = ch;
                        ReadChar();
                        tok = new Token(TokenType.NOT_EQ, first.ToString() + ch.ToString());
                    }
                    else
                        tok = new Token(TokenType.BANG, ch);
                    break;
                case (char)0:
                    tok = new Token(TokenType.EOF, String.Empty);
                    break;
                default:
                    if (IsLetter(ch))
                    {
                        tok.Literal = ReadIdentifier();
                        tok.Type = LookupIdentifier(tok.Literal);
                        return tok;
                    }
                    else if (IsDigit(ch))
                    {
                        tok.Type = TokenType.INT;
                        tok.Literal = ReadNumber();
                        return tok;
                    }
                    else
                        tok = new Token(TokenType.ILLEGAL, ch);
                    break;
            }

            ReadChar();
            return tok;
        }

        private string ReadString()
        {
            var pos = position + 1;
            while (true)
            {
                ReadChar();
                if (ch == '"')
                    break;
            }

            return input.Substring(pos, position - pos);
        }

        public List<Token> Lex()
        {
            var result = new List<Token>();

            ReadChar();
            var token = NextToken();
            result.Add(token);

            while (token.Type != TokenType.EOF)
            {
                token = NextToken();
                result.Add(token);
            }

            return result;
        }
    }
}
