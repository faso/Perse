using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lang.AST;
using lang.Lexing;
using PrefixParseFn = System.Func<Lang.AST.IExpression>;
using InfixParseFn = System.Func<Lang.AST.IExpression, Lang.AST.IExpression>;

namespace Lang.Parsing
{
    public class Parser
    {
        #region fields
        private Lexer Lexer;

        private Token CurToken;
        private Token PeekToken;

        public List<string> Errors;
        #endregion

        public Dictionary<TokenType, PrefixParseFn> PrefixParseFns;
        public Dictionary<TokenType, InfixParseFn> InfixParseFns;

        public void RegisterPrefix(TokenType tokenType, PrefixParseFn fn)
        {
            PrefixParseFns[tokenType] = fn;
        }

        public void RegisterInfix(TokenType tokenType, InfixParseFn fn)
        {
            InfixParseFns[tokenType] = fn;
        }

        public Dictionary<TokenType, Precedence> Precedences = new Dictionary<TokenType, Precedence>()
        {
            { TokenType.EQ, Precedence.EQUALS },
            { TokenType.NOT_EQ, Precedence.EQUALS },
            { TokenType.LESSTHAN, Precedence.LESSGREATER },
            { TokenType.GREATERTHAN, Precedence.LESSGREATER },
            { TokenType.PLUS, Precedence.SUM },
            { TokenType.MINUS, Precedence.SUM },
            { TokenType.SLASH, Precedence.PRODUCT },
            { TokenType.ASTERISK, Precedence.PRODUCT },
            { TokenType.LPAREN, Precedence.CALL },
            { TokenType.LBRACKET, Precedence.INDEX }
        };

        public Precedence PeekPrecedence()
        {
            if (Precedences.ContainsKey(PeekToken.Type))
                return Precedences[PeekToken.Type];

            return Precedence.LOWEST;
        }

        public Precedence CurPrecedence()
        {
            if (Precedences.ContainsKey(CurToken.Type))
                return Precedences[CurToken.Type];

            return Precedence.LOWEST;
        }

        public Parser(Lexer lexer)
        {
            this.Lexer = lexer;
            this.Errors = new List<string>();

            PrefixParseFns = new Dictionary<TokenType, PrefixParseFn>();
            RegisterPrefix(TokenType.IDENT, ParseIdentifier);
            RegisterPrefix(TokenType.INT, ParseIntegerLiteral);

            RegisterPrefix(TokenType.BANG, ParsePrefixExpression);
            RegisterPrefix(TokenType.MINUS, ParsePrefixExpression);

            RegisterPrefix(TokenType.TRUE, ParseBoolean);
            RegisterPrefix(TokenType.FALSE, ParseBoolean);

            RegisterPrefix(TokenType.LPAREN, ParseGroupedExpression);

            RegisterPrefix(TokenType.IF, ParseIfExpression);
            RegisterPrefix(TokenType.FUNCTION, ParseFunctionLiteral);
            RegisterPrefix(TokenType.FOR, ParseForLoopLiteral);

            RegisterPrefix(TokenType.STRING, ParseStringLiteral);

            InfixParseFns = new Dictionary<TokenType, InfixParseFn>();
            RegisterInfix(TokenType.PLUS, ParseInfixExpression);
            RegisterInfix(TokenType.MINUS, ParseInfixExpression);
            RegisterInfix(TokenType.SLASH, ParseInfixExpression);
            RegisterInfix(TokenType.ASTERISK, ParseInfixExpression);
            RegisterInfix(TokenType.EQ, ParseInfixExpression);
            RegisterInfix(TokenType.NOT_EQ, ParseInfixExpression);
            RegisterInfix(TokenType.LESSTHAN, ParseInfixExpression);
            RegisterInfix(TokenType.GREATERTHAN, ParseInfixExpression);
            RegisterInfix(TokenType.LBRACKET, ParseIndexExpression);

            RegisterInfix(TokenType.LPAREN, ParseCallExpression);
            RegisterPrefix(TokenType.LBRACKET, ParseArrayLiteral);

            PeekToken = Lexer.NextToken();
            NextToken();
            NextToken();
        }

        private void NextToken()
        {
            CurToken = PeekToken;
            PeekToken = Lexer.NextToken();
        }

        public Program ParseProgram()
        {
            var program = new Program();
            program.Statements = new List<IStatement>();

            while (CurToken.Type != TokenType.EOF)
            {
                var stmt = ParseStatement();
                if (stmt != null)
                {
                    program.Statements.Add(stmt);
                }
                NextToken();
            }

            return program;
        }

        public IExpression ParseIdentifier()
            => new Identifier()
            {
                Token = CurToken,
                Value = CurToken.Literal
            };

        public IExpression ParsePrefixExpression()
        {
            var exp = new PrefixExpression()
            {
                Token = CurToken,
                Operator = CurToken.Literal
            };

            NextToken();

            exp.Right = ParseExpression(Precedence.PREFIX);

            return exp;
        }

        public IExpression ParseGroupedExpression()
        {
            NextToken();

            var exp = ParseExpression(Precedence.LOWEST);
            if (!ExpectPeek(TokenType.RPAREN))
                return null;

            return exp;
        }

        public IExpression ParseCallExpression(IExpression function)
        {
            var exp = new CallExpression()
            {
                Token = CurToken,
                Function = function
            };
            exp.Arguments = ParseExpressionList(TokenType.RPAREN);

            return exp;
        }

        public List<IExpression> ParseCallArguments()
        {
            var args = new List<IExpression>();

            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return args;
            }

            NextToken();
            args.Add(ParseExpression(Precedence.LOWEST));

            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken(); NextToken();
                args.Add(ParseExpression(Precedence.LOWEST));
            }

            if (!ExpectPeek(TokenType.RPAREN))
                return null;

            return args;
        }

        public IExpression ParseStringLiteral()
        {
            return new StringLiteral() { Token = CurToken, Value = CurToken.Literal };
        }

        public IExpression ParseIfExpression()
        {
            var exp = new IfExpression()
            {
                Token = CurToken
            };

            if (!ExpectPeek(TokenType.LPAREN))
                return null;

            NextToken();
            exp.Condition = ParseExpression(Precedence.LOWEST);

            NextToken();

            if (!ExpectPeek(TokenType.LBRACE))
                return null;

            exp.Consequence = ParseBlockStatement();

            if (PeekTokenIs(TokenType.ELSE))
            {
                NextToken();

                if (!ExpectPeek(TokenType.LBRACE))
                {
                    return null;
                }

                exp.Alternative = ParseBlockStatement();
            }

            return exp;
        }

        public IExpression ParseFunctionLiteral()
        {
            var lit = new FunctionLiteral()
            {
                Token = CurToken
            };

            if (!ExpectPeek(TokenType.LPAREN))
                return null;

            lit.Parameters = ParseFunctionParameters();

            if (!ExpectPeek(TokenType.LBRACE))
                return null;

            lit.Body = ParseBlockStatement();

            return lit;
        }

        public IExpression ParseForLoopLiteral()
        {
            var lit = new LoopStatement()
            {
                Token = CurToken
            };

            if (!ExpectPeek(TokenType.LPAREN))
                return null;

            var identifiers = new List<Identifier>();
            
            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return null;
            }
            NextToken();

            var ident = new Identifier()
            {
                Token = CurToken,
                Value = CurToken.Literal
            };
            lit.LoopVariable = ident;
            NextToken();

            if (CurTokenIs(TokenType.COMMA))
            {
                NextToken();
                if (!CurTokenIs(TokenType.IDENT))
                    return null;

                lit.LoopIndex = new Identifier()
                {
                    Token = CurToken,
                    Value = CurToken.Literal
                };
            }
            NextToken();

            if (!CurTokenIs(TokenType.IN))
                return null;
            NextToken();

            lit.Target = new Identifier()
            {
                Token = CurToken,
                Value = CurToken.Literal
            };
            
            if (!ExpectPeek(TokenType.RPAREN))
                return null;

            if (!ExpectPeek(TokenType.LBRACE))
                return null;

            lit.Body = ParseBlockStatement();

            return lit;
        }

        public IExpression ParseArrayLiteral()
        {
            var array = new ArrayLiteral() { Token = CurToken };

            array.Elements = ParseExpressionList(TokenType.RBRACKET);

            return array;
        }

        public List<IExpression> ParseExpressionList(TokenType end)
        {
            var list = new List<IExpression>();

            if (PeekTokenIs(end))
            {
                NextToken();
                return list;
            }

            NextToken();
            list.Add(ParseExpression(Precedence.LOWEST));

            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken();
                NextToken();
                list.Add(ParseExpression(Precedence.LOWEST));
            }

            if (!ExpectPeek(end))
            {
                return null;
            }

            return list;
        }

        public List<Identifier> ParseFunctionParameters()
        {
            var identifiers = new List<Identifier>();

            if (PeekTokenIs(TokenType.RPAREN))
            {
                NextToken();
                return identifiers;
            }

            NextToken();

            var ident = new Identifier()
            {
                Token = CurToken,
                Value = CurToken.Literal
            };
            identifiers.Add(ident);

            while (PeekTokenIs(TokenType.COMMA))
            {
                NextToken(); NextToken();
                ident = new Identifier()
                {
                    Token = CurToken,
                    Value = CurToken.Literal
                };
                identifiers.Add(ident);
            }

            if (!ExpectPeek(TokenType.RPAREN))
                return null;

            return identifiers;
        }

        public BlockStatement ParseBlockStatement()
        {
            var block = new BlockStatement()
            {
                Token = CurToken
            };
            block.Statements = new List<IStatement>();

            NextToken();

            while (!CurTokenIs(TokenType.RBRACE))
            {
                var stmt = ParseStatement();
                if (stmt != null)
                {
                    block.Statements.Add(stmt);
                }
                NextToken();
            }

            return block;
        }

        public IExpression ParseBoolean()
        {
            return new AST.Boolean()
            {
                Token = CurToken,
                Value = CurTokenIs(TokenType.TRUE)
            };
        }

        public IExpression ParseIntegerLiteral()
        {
            var lit = new IntegerLiteral() { Token = CurToken };

            int value;
            bool res = Int32.TryParse(CurToken.Literal, out value);

            if (!res)
            {
                Errors.Add($"Could not parse {CurToken.Literal} as integer");
                return null;
            }

            lit.Value = value;
            return lit;
        }

        public IStatement ParseStatement()
        {
            switch (CurToken.Type)
            {
                case TokenType.VAR:
                    return ParseVarStatement();
                case TokenType.RETURN:
                    return ParseReturnStatement();
                case TokenType.IDENT:
                    {
                        if (PeekTokenIs(TokenType.ASSIGN))
                            return ParseAssignStatement();
                        else
                            return ParseExpressionStatement();
                    }
                default:
                    return ParseExpressionStatement();
            }
        }

        public ExpressionStatement ParseExpressionStatement()
        {
            var stmt = new ExpressionStatement()
            {
                Token = CurToken
            };

            stmt.Expression = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
                NextToken();

            return stmt;
        }

        public IExpression ParseExpression(Precedence prec)
        {
            var prefix = PrefixParseFns[CurToken.Type];
            if (prefix == null)
            {
                NoPrefixParseFnError(CurToken.Type);
                return null;
            }

            var leftExp = prefix();

            while (!PeekTokenIs(TokenType.SEMICOLON) && prec < PeekPrecedence())
            {
                var infix = InfixParseFns[PeekToken.Type];
                if (infix == null)
                    return leftExp;

                NextToken();

                leftExp = infix(leftExp);
            }

            return leftExp;
        }

        public IExpression ParseIndexExpression(IExpression left)
        {
            var exp = new IndexExpression()
            {
                Token = CurToken,
                Left = left
            };

            NextToken();
            exp.Index = ParseExpression(Precedence.LOWEST);

            if (!ExpectPeek(TokenType.RBRACKET))
                return null;

            return exp;
        }

        public ReturnStatement ParseReturnStatement()
        {
            var stmt = new ReturnStatement()
            {
                Token = CurToken
            };

            NextToken();

            stmt.ReturnValue = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }

        public VarStatement ParseVarStatement()
        {
            var stmt = new VarStatement();
            stmt.Token = CurToken;

            if (!ExpectPeek(TokenType.IDENT))
                return null;

            stmt.Name = new Identifier()
            {
                Token = CurToken,
                Value = CurToken.Literal
            };

            if (!ExpectPeek(TokenType.ASSIGN))
            {
                return null;
            }

            NextToken();

            stmt.Value = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }

        public AssignStatement ParseAssignStatement()
        {
            var stmt = new AssignStatement();
            stmt.Token = CurToken;

            stmt.Name = new Identifier()
            {
                Token = CurToken,
                Value = CurToken.Literal
            };

            if (!ExpectPeek(TokenType.ASSIGN))
            {
                return null;
            }

            NextToken();

            stmt.Value = ParseExpression(Precedence.LOWEST);

            if (PeekTokenIs(TokenType.SEMICOLON))
            {
                NextToken();
            }

            return stmt;
        }

        private IExpression ParseInfixExpression(IExpression left)
        {
            var exp = new InfixExpression()
            {
                Token = CurToken,
                Operator = CurToken.Literal,
                Left = left
            };

            var prec = CurPrecedence();
            NextToken();
            exp.Right = ParseExpression(prec);

            return exp;
        }

        private bool CurTokenIs(TokenType t)
            => (CurToken.Type == t);

        private bool PeekTokenIs(TokenType t)
            => (PeekToken.Type == t);

        private bool ExpectPeek(TokenType t)
        {
            if (PeekTokenIs(t))
            {
                NextToken();
                return true;
            }
            else
            {
                PeekError(t);
                return false;
            }
        }

        private void NoPrefixParseFnError(TokenType token)
        {
            var msg = $"No prefix parse function for {Enum.GetName(typeof(TokenType), token)}";
            Errors.Add(msg);
        }

        private void PeekError(TokenType t)
        {
            var msg = $"Expected token {Enum.GetName(typeof(TokenType), t)}, got {Enum.GetName(typeof(TokenType), PeekToken.Type)} instead";
            Errors.Add(msg);
        }
    }
}
