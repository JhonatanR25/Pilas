using System;
using System.Windows.Forms;

namespace Evaluator.UI.Windows
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new CalculatorForm());
        }
    }
}
