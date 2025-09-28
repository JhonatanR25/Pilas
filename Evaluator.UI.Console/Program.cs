using System.Globalization;
using Evaluator.Core;

namespace Evaluator.UI.Console
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            global::System.Console.WriteLine("Evaluator (+ - * / ^ y parentesis)");
            global::System.Console.Write("Expr: ");
            string expr = global::System.Console.ReadLine() ?? "";

            try
            {
                double result = ExpressionEvaluator.Evaluate(expr);
                global::System.Console.WriteLine("= " + result.ToString(CultureInfo.InvariantCulture));
            }
            catch
            {
                global::System.Console.WriteLine("Invalid expression");
            }
        }
    }
}

