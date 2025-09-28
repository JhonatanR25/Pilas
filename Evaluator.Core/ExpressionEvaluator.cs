using System;
using System.Collections.Generic;
using System.Globalization;

namespace Evaluator.Core
{
    
    /// Expression evaluator using Shunting-yard to convert to RPN and a stack to evaluate.
    /// Supports + - * / ^, parentheses and decimals with '.'. '^' is right-associative.
 
    public static class ExpressionEvaluator
    {
        // Supported operators: + - * / ^
        // Note: ^ is right-associative
        // Parentheses () are supported
        // Decimal numbers must use '.' as separator

        public static double Evaluate(string expr)
        {
            // Convert expression to RPN (Reverse Polish Notation) using a Queue (assignment requirement)
            var outputQueue = ToRpn(expr);
            var value = EvalRpn(outputQueue);
            return value;
        }

        // ---------------- Tokenization ----------------
        private enum TokenType { Number, Operator, LParen, RParen }

        private readonly struct Token
        {
            public TokenType Type { get; }
            public string Text { get; }
            public double Number { get; }
            public Token(TokenType type, string text, double number = 0)
            {
                Type = type; Text = text; Number = number;
            }
        }

       
        /// Splits the input string into tokens (numbers, operators, parentheses).
       
        private static IEnumerable<Token> Tokenize(string s)
        {
            int i = 0; int n = s.Length;
            while (i < n)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }

                // Parse number (can include decimal point or unary minus)
                if (char.IsDigit(c) || c == '.' || (c == '-' && IsUnaryMinus(s, i)))
                {
                    int start = i; i++;
                    while (i < n && (char.IsDigit(s[i]) || s[i] == '.')) i++;
                    string numTxt = s.Substring(start, i - start);
                    if (!double.TryParse(numTxt, NumberStyles.Float, CultureInfo.InvariantCulture, out double val))
                        throw new Exception("Invalid number");
                    yield return new Token(TokenType.Number, numTxt, val);
                    continue;
                }

                // Operators
                if (c is '+' or '-' or '*' or '/' or '^')
                {
                    yield return new Token(TokenType.Operator, c.ToString());
                    i++; continue;
                }

                // Parentheses
                if (c == '(') { yield return new Token(TokenType.LParen, "("); i++; continue; }
                if (c == ')') { yield return new Token(TokenType.RParen, ")"); i++; continue; }

                throw new Exception($"Invalid character '{c}'");
            }
        }

        /// <summary>
        /// Determines if the minus sign at the given index is unary.
        /// True at start or after ( + - * / ^
        /// </summary>
        private static bool IsUnaryMinus(string s, int idx)
        {
            if (idx == 0) return true;
            int j = idx - 1;
            while (j >= 0 && char.IsWhiteSpace(s[j])) j--;
            if (j < 0) return true;
            char p = s[j];
            return p == '(' || p == '+' || p == '-' || p == '*' || p == '/' || p == '^';
        }

        // -------------- Shunting-yard algorithm -> RPN Queue --------------
        private static Queue<Token> ToRpn(string expr)
        {
            var output = new Queue<Token>();      // Queue required by assignment
            var ops = new Stack<Token>();

            foreach (var t in Tokenize(expr))
            {
                switch (t.Type)
                {
                    case TokenType.Number:
                        output.Enqueue(t);
                        break;

                    case TokenType.Operator:
                        while (ops.Count > 0 && ops.Peek().Type == TokenType.Operator)
                        {
                            var top = ops.Peek();
                            if ((IsLeftAssoc(t.Text) && Precedence(t.Text) <= Precedence(top.Text)) ||
                                (!IsLeftAssoc(t.Text) && Precedence(t.Text) < Precedence(top.Text)))
                                output.Enqueue(ops.Pop());
                            else break;
                        }
                        ops.Push(t);
                        break;

                    case TokenType.LParen:
                        ops.Push(t);
                        break;

                    case TokenType.RParen:
                        while (ops.Count > 0 && ops.Peek().Type != TokenType.LParen)
                            output.Enqueue(ops.Pop());
                        if (ops.Count == 0) throw new Exception("Mismatched ')'");
                        ops.Pop(); // discard '('
                        break;
                }
            }

            while (ops.Count > 0)
            {
                var x = ops.Pop();
                if (x.Type is TokenType.LParen or TokenType.RParen)
                    throw new Exception("Mismatched parentheses");
                output.Enqueue(x);
            }

            return output;
        }

        private static int Precedence(string op) => op switch
        {
            "^" => 3,
            "*" or "/" => 2,
            "+" or "-" => 1,
            _ => 0
        };

        private static bool IsLeftAssoc(string op) => op != "^"; // ^ is right-associative

        // ---------------- Evaluate RPN ----------------
        private static double EvalRpn(Queue<Token> rpn)
        {
            var stack = new Stack<double>();
            while (rpn.Count > 0)
            {
                var t = rpn.Dequeue();
                if (t.Type == TokenType.Number)
                {
                    stack.Push(t.Number);
                }
                else if (t.Type == TokenType.Operator)
                {
                    if (stack.Count < 2) throw new Exception("Invalid expression");
                    double b = stack.Pop();
                    double a = stack.Pop();
                    stack.Push(t.Text switch
                    {
                        "+" => a + b,
                        "-" => a - b,
                        "*" => a * b,
                        "/" => a / b,
                        "^" => Math.Pow(a, b),
                        _ => throw new Exception("Unknown operator")
                    });
                }
            }
            if (stack.Count != 1) throw new Exception("Invalid expression");
            return stack.Pop();
        }
    }
}
