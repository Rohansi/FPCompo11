using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using GlitchGame.Entities;
using LoonyVM;

namespace GlitchGame.Debugger
{
    public static class WatchExpression
    {
        public static Func<Computer, int> Compile(string expression)
        {
            try
            {
                var tokens = ShuntingYard(Tokenize(expression));

                var stack = new Stack<Expression>();
                var computerParam = Expression.Parameter(typeof(Computer));
                var computerVm = Expression.PropertyOrField(computerParam, "Vm");
                var computerRegisters = Expression.PropertyOrField(computerVm, "Registers");
                var computerMemory = Expression.PropertyOrField(computerVm, "Memory");

                foreach (var t in tokens)
                {
                    if (t.Type == TokenType.Number)
                    {
                        stack.Push(Expression.Constant(int.Parse(t.Value)));
                        continue;
                    }

                    if (t.Type == TokenType.Word)
                    {
                        Register reg;
                        if (Enum.TryParse(t.Value, true, out reg))
                        {
                            stack.Push(
                                Expression.ArrayIndex(computerRegisters,
                                    Expression.Constant((int)reg)));
                            continue;
                        }

                        string funcName;
                        if (Functions.TryGetValue(t.Value.ToLower(), out funcName))
                        {
                            stack.Push(
                                Expression.Convert(
                                    Expression.Call(
                                        typeof(VmUtil),
                                        funcName,
                                        null,
                                        computerMemory,
                                        stack.Pop()),
                                    typeof(int)));
                            continue;
                        }

                        stack.Push(
                            Expression.Call(
                                typeof(WatchExpression),
                                "LookupSymbol",
                                null,
                                computerParam,
                                Expression.Constant(t.Value)));
                        continue;
                    }

                    ExpressionType op;

                    switch (t.Type)
                    {
                        case TokenType.Add:
                            op = ExpressionType.Add;
                            break;
                        case TokenType.Subtract:
                            op = ExpressionType.Subtract;
                            break;
                        case TokenType.Multiply:
                            op = ExpressionType.Multiply;
                            break;
                        case TokenType.Divide:
                            op = ExpressionType.Divide;
                            break;
                        case TokenType.Modulo:
                            op = ExpressionType.Modulo;
                            break;
                        default:
                            throw new WatchException("Bad token type");
                    }

                    var right = stack.Pop();
                    var left = stack.Pop();
                    stack.Push(Expression.MakeBinary(op, left, right));
                }

                if (stack.Count != 1)
                    throw new WatchException("Syntax error");

                return Expression.Lambda<Func<Computer, int>>(stack.Pop(), computerParam).Compile();
            }
            catch (WatchException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new WatchException("Unhandled error", e);
            }
        }

        public static int LookupSymbol(Computer computer, string name)
        {
            var debugInfo = computer.Code.DebugInfo;
            if (debugInfo == null)
                throw new WatchException(string.Format("Undefined: {0}", name));

            var symbol = debugInfo.FindSymbol(name);
            if (!symbol.HasValue)
                throw new WatchException(string.Format("Undefined: {0}", name));

            return symbol.Value.Address;
        }

        private enum Register
        {
            R0, R1, R2, R3, R4, R5, R6, R7, R8, R9, BP, SP, IP
        }

        #region Parser
        private static IEnumerable<Token> ShuntingYard(IEnumerable<Token> tokens)
        {
            var output = new Queue<Token>();
            var operators = new Stack<Token>();

            foreach (var token in tokens)
            {
                if (IsFunction(token))
                    operators.Push(token);
                else if (IsNumber(token))
                    output.Enqueue(token);

                if (IsOperator(token))
                {
                    while (operators.Count > 0 && IsOperator(operators.Peek()) &&
                           GetPrecedence(token) < GetPrecedence(operators.Peek()))
                    {
                        output.Enqueue(operators.Pop());
                    }

                    operators.Push(token);
                }

                if (token.Type == TokenType.OpenParen)
                    operators.Push(token);

                if (token.Type == TokenType.CloseParen)
                {
                    while (operators.Count > 0 && operators.Peek().Type != TokenType.OpenParen)
                    {
                        output.Enqueue(operators.Pop());
                    }

                    if (operators.Count == 0)
                        throw new WatchException("Bad parens");

                    operators.Pop();

                    if (operators.Count > 0 && IsFunction(operators.Peek()))
                        output.Enqueue(operators.Pop());
                }
            }

            while (operators.Count > 0)
            {
                var o = operators.Pop();
                if (o.Type == TokenType.OpenParen)
                    throw new WatchException("Bad parens");

                output.Enqueue(o);
            }

            return output;
        }

        private static bool IsNumber(Token token)
        {
            return token.Type == TokenType.Number || token.Type == TokenType.Word;
        }

        private static bool IsOperator(Token token)
        {
            return token.Type == TokenType.Add ||
                   token.Type == TokenType.Subtract ||
                   token.Type == TokenType.Multiply ||
                   token.Type == TokenType.Divide ||
                   token.Type == TokenType.Modulo;
        }

        private static bool IsFunction(Token token)
        {
            return token.Type == TokenType.Word && Functions.ContainsKey(token.Value);
        }

        private static int GetPrecedence(Token token)
        {
            switch (token.Type)
            {
                case TokenType.Add:
                case TokenType.Subtract:
                    return 1;
                case TokenType.Multiply:
                case TokenType.Divide:
                case TokenType.Modulo:
                    return 2;
                default:
                    throw new WatchException("Operator precedence");
            }
        }

        private static readonly Dictionary<string, string> Functions = new Dictionary<string, string>
        {
            { "byte", "ReadSByte" },
            { "word", "ReadShort" },
            { "dword", "ReadInt" },
        };
        #endregion

        #region Tokenizer
        private enum TokenType
        {
            Number, Word, Add, Subtract, Multiply, Divide, Modulo, OpenParen, CloseParen
        }

        private struct Token
        {
            public readonly TokenType Type;
            public readonly string Value;

            public Token(TokenType type, string value = null)
            {
                Type = type;
                Value = value;
            }
        }

        private static IEnumerable<Token> Tokenize(string str)
        {
            var pos = 0;
            var prev = new Token(TokenType.Add);
            var build = new StringBuilder(32);

            while (pos < str.Length)
            {
                while (pos < str.Length && char.IsWhiteSpace(str[pos]))
                {
                    pos++;
                }

                var isValidNegative = prev.Type != TokenType.Number && prev.Type != TokenType.Word;
                if (char.IsDigit(str[pos]) || (isValidNegative && str[pos] == '-'))
                {
                    if (prev.Type == TokenType.Number || prev.Type == TokenType.Word)
                        throw new WatchException("Syntax error");

                    int? number = null;

                    if (pos < str.Length - 1 && str[pos] == '0' && str[pos + 1] == 'x')
                    {
                        pos += 2;

                        while (pos < str.Length && ((str[pos] >= '0' && str[pos] <= '9') ||
                                                    (str[pos] >= 'a' && str[pos] <= 'f') ||
                                                    (str[pos] >= 'A' && str[pos] <= 'F')))
                        {
                            build.Append(str[pos++]);
                        }

                        int v;
                        if (int.TryParse(build.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out v))
                            number = v;
                    }
                    else
                    {
                        build.Append(str[pos++]);

                        while (pos < str.Length && char.IsDigit(str[pos]))
                        {
                            build.Append(str[pos++]);
                        }

                        int v;
                        if (int.TryParse(build.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v))
                            number = v;
                    }

                    if (!number.HasValue)
                        throw new WatchException(string.Format("Bad number: {0}", build));

                    prev = new Token(TokenType.Number, number.Value.ToString());
                    yield return prev;

                    build.Clear();
                    continue;
                }

                if (char.IsLetter(str[pos]) || str[pos] == '_' || str[pos] == '.' || str[pos] == '@')
                {
                    if (prev.Type == TokenType.Number || prev.Type == TokenType.Word)
                        throw new WatchException("Syntax error");

                    build.Append(str[pos++]);

                    while (pos < str.Length && (char.IsLetterOrDigit(str[pos]) || str[pos] == '_' || str[pos] == '.' || str[pos] == '@'))
                    {
                        build.Append(str[pos++]);
                    }

                    prev = new Token(TokenType.Word, build.ToString());
                    yield return prev;

                    build.Clear();
                    continue;
                }

                TokenType op;
                if (Operators.TryGetValue(str[pos], out op))
                {
                    pos++;
                    prev = new Token(op);
                    yield return prev;
                    continue;
                }

                throw new WatchException(string.Format("Bad token: {0}", str[pos]));
            }
        }

        private static readonly Dictionary<char, TokenType> Operators = new Dictionary<char, TokenType>
        {
            { '+', TokenType.Add },
            { '-', TokenType.Subtract },
            { '*', TokenType.Multiply },
            { '/', TokenType.Divide },
            { '%', TokenType.Modulo },
            { '(', TokenType.OpenParen },
            { ')', TokenType.CloseParen },
        };
        #endregion
    }

    public class WatchException : Exception
    {
        public WatchException(string message)
            : base(message)
        {

        }

        public WatchException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
