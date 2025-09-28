using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;

namespace Evaluator.UI.Windows
{
    public class CalculatorForm : Form
    {
        private TextBox txtExpr = null!;
        private TextBox txtResult = null!;
        private TableLayoutPanel keys = null!;

        // Style
        private static readonly Color BG = Color.FromArgb(245, 246, 248);
        private static readonly Color Card = Color.White;
        private static readonly Color CardBorder = Color.FromArgb(210, 214, 220);
        private static readonly Color Btn = Color.FromArgb(248, 249, 251);
        private static readonly Color BtnHover = Color.FromArgb(236, 239, 243);
        private static readonly Color TextCol = Color.FromArgb(33, 37, 41);

        private const int Radius = 12;

        public CalculatorForm()
        {
            BuildUi();
            KeyPreview = true;
            KeyDown += OnKeyDown;
            DoubleBuffered = true;
        }

        private void BuildUi()
        {
            
            Text = "Funtions Evaluator";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(340, 560);
            BackColor = BG;

            var outer = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = BG };
            Controls.Add(outer);

            var card = new Panel { Dock = DockStyle.Fill, BackColor = Card };
            card.Paint += (s, e) =>
            {
                using var pen = new Pen(CardBorder);
                var r = card.ClientRectangle; r.Width -= 1; r.Height -= 1;
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.DrawRectangle(pen, r);
            };
            outer.Controls.Add(card);

            // Layout
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10),
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));   
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  
            card.Controls.Add(layout);

            
            var display = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderColor = CardBorder,
                CornerRadius = 12,
                Padding = new Padding(10, 8, 10, 8)
            };
            layout.Controls.Add(display, 0, 0);

            var displayGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            displayGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 44)); //Operation
            displayGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); //Result
            display.Controls.Add(displayGrid);

            txtExpr = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 12.5f),
                ForeColor = TextCol,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Multiline = false
            };
            displayGrid.Controls.Add(txtExpr, 0, 0);

            txtResult = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11f),
                ForeColor = Color.FromArgb(108, 117, 125),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                TabStop = false,
                Multiline = false
            };
            displayGrid.Controls.Add(txtResult, 0, 1);

            keys = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Card,
                ColumnCount = 4,
                RowCount = 0,
                Padding = new Padding(0)
            };
            for (int i = 0; i < 4; i++)
                keys.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            layout.Controls.Add(keys, 0, 1);

            
            AddRow("7", "8", "9", "+");
            AddRow("4", "5", "6", "-");
            AddRow("1", "2", "3", "/");
            AddRow("0", ".", "(", ")");

            AddRow("Del", "Clr", "*", "^");

            
            var btnEq = CreateButton("=", OnEqualsClick);
            AddSpanned(btnEq, row: keys.RowCount, col: 0, colSpan: 4);
            btnEq.Margin = new Padding(2, 6, 2, 2);
            btnEq.Height += 8;
        }

        // ---- helpers UI ----
        private void AddRow(string c1, string c2, string c3, string c4)
        {
            int r = keys.RowCount;
            keys.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
            keys.RowCount++;

            AddCell(c1, r, 0);
            AddCell(c2, r, 1);
            AddCell(c3, r, 2);
            AddCell(c4, r, 3);
        }

        private void AddCell(string label, int row, int col)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                keys.Controls.Add(new Panel { Dock = DockStyle.Fill }, col, row);
                return;
            }

            Button btn = label switch
            {
                "Del" => CreateButton(label, OnDeleteClick),
                "Clr" => CreateButton(label, OnClearClick),
                "=" => CreateButton(label, OnEqualsClick),
                _ => CreateButton(label, OnKeyClick),
            };
            keys.Controls.Add(btn, col, row);
        }

        private void AddSpanned(Control ctl, int row, int col, int colSpan)
        {
            keys.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
            keys.RowCount++;
            keys.Controls.Add(ctl, col, row);
            keys.SetColumnSpan(ctl, colSpan);
        }

        private Button CreateButton(string text, EventHandler onClick)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                BackColor = Btn,
                ForeColor = TextCol,
                FlatStyle = FlatStyle.Flat,
                TabStop = false
            };
            btn.FlatAppearance.BorderSize = 0;

            btn.Resize += (s, e) => { if (s is Button bb) ApplyRoundRegion(bb); };
            ApplyRoundRegion(btn);

            btn.MouseEnter += (s, e) => btn.BackColor = BtnHover;
            btn.MouseLeave += (s, e) => btn.BackColor = Btn;

            btn.Click += onClick;
            return btn;
        }

        private void ApplyRoundRegion(Button btn)
        {
            var r = btn.ClientRectangle;
            if (r.Width < 2 || r.Height < 2) return;

            using var path = new GraphicsPath();
            int d = Radius * 2;
            path.StartFigure();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            btn.Region = new Region(path);
        }

        private void OnKeyClick(object? sender, EventArgs e)
        {
            if (sender is Button b) txtExpr.AppendText(b.Text);
        }

        private void OnDeleteClick(object? sender, EventArgs e)
        {
            var t = txtExpr.Text;
            if (!string.IsNullOrEmpty(t))
                txtExpr.Text = t[..^1];
            txtExpr.SelectionStart = txtExpr.Text.Length;
        }

        private void OnClearClick(object? sender, EventArgs e)
        {
            txtExpr.Clear();
            txtResult.Clear();
        }

        private void OnEqualsClick(object? sender, EventArgs e)
        {
            try
            {
                double value = ExpressionEvaluator.Evaluate(txtExpr.Text);
                txtResult.Text = value.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                txtResult.Text = "Invalid expression";
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OnEqualsClick(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Back)
            {
                OnDeleteClick(this, EventArgs.Empty);
                e.SuppressKeyPress = true;
            }
        }

        private class RoundedPanel : Panel
        {
            public int CornerRadius { get; set; } = 12;
            public Color BorderColor { get; set; } = Color.Silver;

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                using var path = BuildPath(ClientRectangle, CornerRadius);
                Region = new Region(path);
                Invalidate();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = ClientRectangle; rect.Width -= 1; rect.Height -= 1;
                using var path = BuildPath(rect, CornerRadius);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawPath(pen, path);
            }

            private static GraphicsPath BuildPath(Rectangle r, int radius)
            {
                var gp = new GraphicsPath();
                int d = radius * 2;
                gp.StartFigure();
                gp.AddArc(r.X, r.Y, d, d, 180, 90);
                gp.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                gp.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                gp.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
                gp.CloseFigure();
                return gp;
            }
        }

        private static class ExpressionEvaluator
        {
            private static readonly Dictionary<string, (int prec, bool rightAssoc)> Ops = new()
            {
                { "+", (1, false) },
                { "-", (1, false) },
                { "*", (2, false) },
                { "/", (2, false) },
                { "^", (3, true)  },
            };

            public static double Evaluate(string expr)
            {
                if (expr is null) throw new ArgumentNullException(nameof(expr));
                expr = PreprocessUnaryMinus(expr.Trim());
                var rpn = ToRpn(expr);
                return EvalRpn(rpn);
            }

            private static string PreprocessUnaryMinus(string s)
            {
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    if (c == '-' && (i == 0 || s[i - 1] == '(')) sb.Append('0');
                    sb.Append(c);
                }
                return sb.ToString();
            }

            private static Queue<string> ToRpn(string s)
            {
                var output = new Queue<string>();
                var stack = new Stack<string>();
                int i = 0;
                while (i < s.Length)
                {
                    char ch = s[i];
                    if (char.IsWhiteSpace(ch)) { i++; continue; }

                    if (char.IsDigit(ch) || ch == '.')
                    {
                        int j = i + 1;
                        while (j < s.Length && (char.IsDigit(s[j]) || s[j] == '.')) j++;
                        output.Enqueue(s.Substring(i, j - i));
                        i = j; continue;
                    }

                    string t = ch.ToString();

                    if (Ops.ContainsKey(t))
                    {
                        var (p1, r1) = Ops[t];
                        while (stack.Count > 0 && Ops.TryGetValue(stack.Peek(), out var top))
                        {
                            var (p2, _) = top;
                            if ((!r1 && p1 <= p2) || (r1 && p1 < p2)) output.Enqueue(stack.Pop());
                            else break;
                        }
                        stack.Push(t); i++; continue;
                    }

                    if (t == "(") { stack.Push(t); i++; continue; }
                    if (t == ")")
                    {
                        while (stack.Count > 0 && stack.Peek() != "(") output.Enqueue(stack.Pop());
                        if (stack.Count == 0 || stack.Pop() != "(") throw new Exception("Mismatched parentheses");
                        i++; continue;
                    }

                    throw new Exception($"Unexpected token '{t}'");
                }

                while (stack.Count > 0)
                {
                    var t = stack.Pop();
                    if (t == "(" || t == ")") throw new Exception("Mismatched parentheses");
                    output.Enqueue(t);
                }
                return output;
            }

            private static double EvalRpn(Queue<string> rpn)
            {
                var st = new Stack<double>();
                while (rpn.Count > 0)
                {
                    string t = rpn.Dequeue();
                    if (Ops.ContainsKey(t))
                    {
                        if (st.Count < 2) throw new Exception("Invalid expression");
                        double b = st.Pop(), a = st.Pop();
                        st.Push(t switch
                        {
                            "+" => a + b,
                            "-" => a - b,
                            "*" => a * b,
                            "/" => b == 0 ? throw new DivideByZeroException() : a / b,
                            "^" => Math.Pow(a, b),
                            _ => throw new Exception("Op")
                        });
                    }
                    else
                    {
                        if (!double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                            throw new Exception("Number format");
                        st.Push(v);
                    }
                }
                if (st.Count != 1) throw new Exception("Invalid expression");
                return st.Pop();
            }
        }
    }
}


