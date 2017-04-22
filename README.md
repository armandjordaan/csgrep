# csgrep
C# minimal grep implementation, with .NET regex

To print lines matching a pattern.

# Overview

csgrep.exe [options] pattern FILE

# Description

csgrep searches the named input FILE for lines containing a match to the given PATTERN. By default, grep prints the matching lines.

Command line options:

  -r, --regexp            Required. Regular expression to be processed.

  -i, --input             Required. Input file to be processed.

  -n, --line-number       Print the line number.

  -o, --only-matching     Print only the matching part

  -m, --max-count         Stop after numer of matches

  -A, --after-context     print number of context lines trailing match

  -B, --before-context    print number of context lines leading match

  -C, --context           print number of context lines leading & trailing
                          match

  -c, --count             Print only count of matching lines per FILE

  -H, --with-filename     Print filename for each match

  -U, --binary            Do not strip CR characters at EOL (MSDOS/Windows)

  -V, --verbose           Prints all diagnostic messages to standard output.




