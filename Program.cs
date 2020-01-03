using System;
using System.Linq;

namespace Snippet
{
    internal class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Unnecessary")]
        private static void Main(string[] args)
        {
            try
            {
                if (args.Length > 2)
                {
                    throw new Exception(@"Usage: snippet [category] [snippet]
Run snippet with out arguments to list categories,
or run snippet <category> for a list of snippets for that category");
                }

                var categories = SnippetConfig.Load();

                if (args.Length == 0)
                {
                    Console.WriteLine("Snippet categories:");
                    Console.WriteLine(string.Join("\n", categories));
                    return;
                }

                var texts = categories.FirstOrDefault(cat => cat.Name.ToLowerInvariant() == args[0].ToLowerInvariant())?.SnippetTexts;

                if (texts == null)
                {
                    throw new Exception($"No such category: {args[0]}, run \"snippets\" without arguments to list categories");
                }

                if (args.Length == 1)
                {
                    Console.WriteLine($"Snippets in {args[0]}:");
                    Console.WriteLine(string.Join("\n", texts));
                    return;
                }

                var snippet = texts.FirstOrDefault(text => text.Name.ToLowerInvariant() == args[1].ToLowerInvariant())?.Content;

                if (snippet == null)
                {
                    throw new Exception($"No such snippet: {args[1]}, run \"snippets {args[0]}\" to list snippets");
                }

                Term.EnableVirtualMode();

                Console.WriteLine("Press tab to toggle between fields, enter to copy text into clipboard and escape to exit");

                var finalSnippet = EditSnippet(" > " + snippet.Trim());
                TextCopy.Clipboard.SetText(finalSnippet);
                Console.WriteLine($"Copied {finalSnippet} into clipboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static string EditSnippet(string snippet)
        {
            var top = Console.CursorTop;
            var segments = Segment.Parse(snippet).ToList();
            var inputs = segments
                .Select((segment, globalIndex) => (segment, globalIndex))
                .Where(x => x.segment is InputSegment)
                .Select((x, localIndex) => (x.globalIndex, localIndex))
                .ToDictionary(x => x.localIndex, x => x.globalIndex);

            var activeIndex = 0;
            var active = segments.FirstOrDefault(seg => seg is InputSegment) as InputSegment;
            if (active != null)
            {
                active.Active = true;
            }
            var len = 0;

            while (true)
            {
                var text = string.Join("", segments);
                if (len > text.Length)
                {
                    text += new string(' ', len - text.Length);
                }
                Console.Write(text);
                var offset = active != null ? string.Join("", segments.TakeWhile(s => s != active).Select(s => s.Value)).Length + active.Pos : 0;
                len = text.Length;
                Console.SetCursorPosition(offset, top);
                var key = Console.ReadKey(true);
                Console.SetCursorPosition(0, top);
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine();
                    throw new Exception("Exiting...");
                }
                else if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    var rawText = string.Join("", segments.Select(s => s.Value));
                    return rawText;
                }
                else if (key.Key == ConsoleKey.Tab)
                {
                    if (!inputs.Any())
                    {
                        continue;
                    }

                    if (key.Modifiers == ConsoleModifiers.Shift)
                    {
                        activeIndex--;
                        if (activeIndex < 0)
                        {
                            activeIndex = inputs.Count - 1;
                        }
                    }
                    else
                    {
                        activeIndex = (activeIndex + 1) % inputs.Count;
                    }
                    active.Active = false;
                    active = segments[inputs[activeIndex]] as InputSegment;
                    active.Active = true;
                }
                else
                {
                    active.OnKey(key);
                }
            }
        }
    }
}