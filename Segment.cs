using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snippet
{
    internal abstract class Segment
    {
        public string Value { get; set; }

        public Segment(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }

        public static IEnumerable<Segment> Parse(string text)
        {
            var buffer = new StringBuilder();
            var data = new Queue<char>(text);
            while (data.Any())
            {
                var c = data.Dequeue();
                switch (c)
                {
                    case '{':
                        Require(data, c);
                        if (buffer.Length > 0)
                        {
                            yield return new StaticSegment(buffer.ToString());
                            buffer.Clear();
                        }
                        break;

                    case '}':
                        Require(data, c);
                        yield return new InputSegment(buffer.ToString());
                        buffer.Clear();
                        break;

                    default:
                        buffer.Append(c);
                        break;
                }
            }
            yield return new StaticSegment(buffer.ToString());
        }

        private static void Require(Queue<char> q, char c)
        {
            if (!q.Any())
            {
                throw new Exception($"Unexpected EOF looking for {c}");
            }

            if (q.Peek() != c)
            {
                throw new Exception($"Unexpected {q.Peek()} looking for {c}");
            }

            q.Dequeue();
        }
    }

    internal class StaticSegment : Segment
    {
        public StaticSegment(string value) : base(value)
        {
        }
    }

    internal class InputSegment : Segment
    {
        private const char esc = (char)27;
        private readonly string original;
        private bool firstInput = true;
        public int Pos { get; set; }

        public InputSegment(string value) : base(value)
        {
            original = value;
            Pos = 0;
        }

        public bool Active { get; set; }

        public void OnKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Backspace)
            {
                if (Pos == 0)
                {
                    return;
                }
                Value = Value.Remove(Pos - 1, 1);
                Pos--;
            }
            else if (keyInfo.Key == ConsoleKey.LeftArrow)
            {
                Pos = Math.Max(0, Pos - 1);
            }
            else if (keyInfo.Key == ConsoleKey.RightArrow)
            {
                Pos = Math.Min(Value.Length, Pos + 1);
            }
            else if (keyInfo.Key == ConsoleKey.V && keyInfo.Modifiers == ConsoleModifiers.Control)
            {
                if (firstInput)
                {
                    firstInput = false;
                    Value = "";
                }
                Value += TextCopy.Clipboard.GetText();
                Pos = Value.Length;
            }
            else
            {
                var k = keyInfo.KeyChar;
                if (k == '\0')
                {
                    return;
                }

                if (firstInput)
                {
                    firstInput = false;
                    if (Pos == 0)
                    {
                        Value = "";
                    }
                }
                Value = Value.Insert(Pos, k.ToString());
                Pos++;
            }
        }

        public override string ToString()
        {
            var swap = $"{esc}[7m";
            var reset = $"{esc}[27m";
            var ul = $"{esc}[4m";
            var no_ul = $"{esc}[24m";
            return $"{(Active ? swap : "")}{ul}{(string.IsNullOrWhiteSpace(Value) ? original : Value)}{no_ul}{(Active ? reset : "")}";
        }
    }
}