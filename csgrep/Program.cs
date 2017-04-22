using System;
using CommandLine;
using System.Text.RegularExpressions;
using System.IO;

namespace csgrep
{
    class Options
    {
        [Option('r', "regexp", Required = true,
          HelpText = "Regular expression to be processed.")]
        public string Regexpr { get; set; }

        [Option('i', "input", Required = true,
          HelpText = "Input file to be processed.")]
        public string Inputfile { get; set; }

        [Option('n', "line-number", Required = false,
          HelpText = "Print the line number.")]
        public bool PrintLineNumber { get; set; }

        [Option('o', "only-matching", Required = false,
          HelpText = "Print only the matching part")]
        public bool PrintOnlymatchingPart { get; set; }

        [Option('m', "max-count", Required = false,
          HelpText = "Stop after numer of matches")]
        public long MaxMatches { get; set; } = Int64.MaxValue;

        [Option('A', "after-context", Required = false,
          HelpText = "print number of context lines trailing match")]
        public int AfterContext { get; set; } = 0;

        [Option('B', "before-context", Required = false,
          HelpText = "print number of context lines leading match")]
        public int BeforeContext { get; set; } = 0;

        [Option('C', "context", Required = false,
          HelpText = "print number of context lines leading & trailing match")]
        public int Context { get; set; } = 0;

        [Option('c', "count", Required = false,
          HelpText = "Print only count of matching lines per FILE")]
        public bool Countonly { get; set; } = false;

        [Option('H', "with-filename", Required = false,
          HelpText = "Print filename for each match")]
        public bool WithFilename { get; set; } = false;

        [Option('U', "binary", Required = false,
          HelpText = "Do not strip CR characters at EOL (MSDOS/Windows)")]
        public bool DoNotStripCR { get; set; } = false;

        // Omitting long name, default --verbose
        [Option('V', "verbose", Required = false,
          HelpText = "Prints all diagnostic messages to standard output.")]
        public bool Verbose { get; set; }
    }

    class Program
    {
        static Options options = new Options();
        static int count = 0;
        static Parser parser;

        static void PrintLeadingContext(string[] lines, int linenumber, int num_context, Match m, string filename)
        {
            int start = linenumber - num_context;

            if (start < 0)
            {
                start = 0;
            }

            Console.WriteLine();
            for(int i=start; i<linenumber; i++)
            {
                PrintMatch(lines, i, m, filename);
            }
        }

        static void PrintTrailingContext(string[] lines, int linenumber, int num_context, Match m, string filename)
        {
            int end = linenumber + num_context + 1;

            if (end > lines.Length)
            {
                end = lines.Length;
            }

            for (int i = linenumber+1; i < end; i++)
            {
                PrintMatch(lines, i, m, filename);
            }
            Console.WriteLine();
        }

        static void PrintMatch(string[] lines, int linenumber, Match m, string filename)
        {
            if (options.PrintLineNumber)
            {
                Console.Write(string.Format("[{0}] ", linenumber + 1));
            }

            if (options.WithFilename)
            {
                Console.Write(string.Format("[{0}] ", filename));
            }

            if (options.PrintOnlymatchingPart)
            {
                Console.Write(lines[linenumber].Substring(m.Index, m.Length));
            }
            else
            {
                Console.Write(lines[linenumber]);
            }
            Console.WriteLine();
        }

        static void Finalise()
        {
            if (options.Countonly)
            {
                Console.WriteLine("Number of matches: {0}", count);
            }

            if (options.Verbose)
            {
                Console.WriteLine("Done.");
            }
        }

        static void OnCommandLineParseFail()
        {
            Console.WriteLine("Command line parse failure");

            Console.WriteLine("Help: " + parser.Settings.HelpWriter.ToString());
        }

        static void PrintOptions()
        {
            Console.WriteLine("Options:");
            Console.WriteLine("After Context: "+options.AfterContext.ToString());
            Console.WriteLine("Before Context: " + options.BeforeContext.ToString());
            Console.WriteLine("Context: " + options.Context.ToString());
            Console.WriteLine("Count only: " + options.Countonly.ToString());
            Console.WriteLine("Do not strip CR: " + options.DoNotStripCR.ToString());
            Console.WriteLine("Input filename: " + options.Inputfile.ToString());
            Console.WriteLine("Max Matches: " + options.MaxMatches.ToString());
            Console.WriteLine("Print line number: " + options.PrintLineNumber.ToString());
            Console.WriteLine("Print only matching part: " + options.PrintOnlymatchingPart.ToString());
            Console.WriteLine("Regular expression: " + options.Regexpr.ToString());
            Console.WriteLine("Verbose: " + options.Verbose.ToString());
            Console.WriteLine("With filename: " + options.WithFilename.ToString());
        }

        static void Main(string[] args)
        {
            try
            {
                StringWriter t = new StringWriter();

                parser = new CommandLine.Parser(s =>
                {
                    s.CaseSensitive = true;
                    s.HelpWriter = t;
                    s.IgnoreUnknownArguments = false;
                });

                var isValid = parser.ParseArgumentsStrict(args, options, OnCommandLineParseFail);

                if (isValid)
                {
                    if (options.Verbose)
                    {
                        PrintOptions();
                    }

                    string s = File.ReadAllText(options.Inputfile);

                    Regex regex = new Regex(options.Regexpr);

                    if (options.Context > 0)
                    {
                        options.AfterContext = options.Context;
                        options.BeforeContext = options.Context;
                    }

                    var lines = s.Split(new[] { '\n' }, StringSplitOptions.None);

                    if (options.Verbose)
                    {
                        Console.WriteLine("Lines read: " + lines.Length);
                    }

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (!options.DoNotStripCR)
                        {
                            lines[i] = lines[i].Replace("\r", "");
                        }

                        foreach (Match m in regex.Matches(lines[i]))
                        {
                            if (!options.Countonly)
                            {
                                if (options.BeforeContext > 0)
                                {
                                    PrintLeadingContext(lines, i, options.BeforeContext, m, options.Inputfile);
                                }

                                PrintMatch(lines, i, m, options.Inputfile);

                                if (options.AfterContext > 0)
                                {
                                    PrintTrailingContext(lines, i, options.AfterContext, m, options.Inputfile);
                                }
                            }

                            count++;
                            if (count >= options.MaxMatches)
                            {
                                Console.WriteLine("Maximum number of matches reached ({0})", options.MaxMatches);
                                Finalise();
                                return;
                            }
                        }
                    }
                    Finalise();
                }
                else
                {
                    Console.WriteLine("Input was not valid.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error (Exception): " + ex.Message);
            }
        }
    }
}
