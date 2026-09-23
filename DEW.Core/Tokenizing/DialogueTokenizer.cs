using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;


namespace DEW.Core
{
    public static class DialogueTokenizer
    {
        private static readonly HashSet<char> PortraitLetters = new HashSet<char> { 'h', 's', 'u', 'l', 'a' };

        public static List<Token> Tokenize(string raw)
        {
            var tokens = new List<Token>();
            int i = 0;
            var textBuffer = new StringBuilder();

            void FlushText()
            {
                if (textBuffer.Length > 0)
                {
                    tokens.Add(new Token { Kind = SyntaxKind.Text, RawText = textBuffer.ToString() });
                    textBuffer.Clear();
                }
            }

            while (i < raw.Length)
            {
                char c = raw[i];

                if (c == '%' && i == 0)
                {
                    FlushText();
                    tokens.Add(new Token { Kind = SyntaxKind.LineLevel, RawText = "%" });
                    i++;
                    continue;
                }
                
                if (c == '@')
                {
                FlushText();
                tokens.Add(new Token { Kind = SyntaxKind.Inline, RawText = "@" });
                i++;
                continue;
                }

                if (c == '^')
                {
                    FlushText();
                    tokens.Add(new Token { Kind = SyntaxKind.Splitter, RawText = "^" });
                    i++;
                    continue;
                }

                if (c == '#')
                {
                    FlushText();
                    tokens.Add(new Token { Kind = SyntaxKind.LineLevel, RawText = "#" });
                    i++;
                    continue;
                }

                if (c == '%')
                {
                    int start = i;
                    i++;
                    while ( i < raw.Length && char.IsLetterOrDigit(raw[i]))
                        i++;
                    FlushText();
                    tokens.Add(new Token { Kind = SyntaxKind.Inline, RawText = raw.Substring(start, i - start) });
                    continue;
                }

                if (c == '$')
                {
                    if (TryMatchDollarMarker(raw, i, out Token? marker, out int length))
                    {
                        FlushText();
                        tokens.Add(marker);
                        i += length;
                        continue;
                    }
                

                //in case of unknown $ command, swallow whole line as unknown token
                //marking this for later to add arg recogniion
                int start = i;
                i++;
                while (i < raw.Length && char.IsLetter(raw[i]))
                    i++;
                FlushText();
                tokens.Add(new Token { Kind = SyntaxKind.Unknown, RawText = raw.Substring(start, i - start) });
                continue;
                }

                textBuffer.Append(c);
                i++;
            }

            FlushText();
            return tokens;
        }

        /*
        Recognizes:
            $b, $e, $k                          -Box control (LineLevel)
            $c <float>                          -Chance split (Structural)
            $action, $t, $v + args              -Argument commands, args run next to # (Structural)
            $0-$13, $h, $s, $u, $l, $a          -portrait commands (Portrait)
        Anything else starting with $ is Unknown for now.
        */
        private static bool TryMatchDollarMarker(string raw, int pos, [NotNullWhen(true)] out Token? token, out int length)
        {
            token = null;
            length = 0;

            if (pos + 1 >= raw.Length) return false;

            char next = raw[pos + 1];

            if (next == 'b' || next == 'e' || next == 'k')
            {
                token = new Token { Kind = SyntaxKind.LineLevel, RawText = raw.Substring(pos, 2) };
                length = 2;
                return true;
            }

            if (next == 'c')
            {
                int start = pos;
                int j = pos + 2;//past "$c"
                while (j < raw.Length && raw[j] == ' ') j++; //skips space
                while (j < raw.Length && (char.IsDigit(raw[j]) || raw[j] == '.')) j++; //point to float

                token = new Token { Kind = SyntaxKind.Structural, RawText = raw.Substring(start, j - start) };
                length = j - start;
                return true;
            }
            //$action MUST come before portraits since default is to pick whichever is true first. Portrait has $a and would return true first otherwise
            if (TryMatchArgCommand(raw, pos, "$action", out token, out length)) return true;
            if (TryMatchArgCommand(raw, pos, "$t", out token, out length)) return true;
            if (TryMatchArgCommand(raw, pos, "$v", out token, out length)) return true;

            if (char.IsDigit(next) || PortraitLetters.Contains(next))
            {
                token = new Token { Kind = SyntaxKind.Portrait, RawText = raw.Substring(pos, 2) };
                length = 2;
                return true;
            }

            return false;
        }

        private static bool TryMatchArgCommand(string raw, int pos, string name,
            [NotNullWhen(true)] out Token? token, out int length)
        {
            token = null;
            length = 0;

            if (!raw.Substring(pos).StartsWith(name)) return false;

            int j = pos + name.Length;
            while (j < raw.Length && raw[j] != '#') j++;

            token = new Token { Kind = SyntaxKind.Structural, RawText = raw.Substring(pos, j - pos) };
            length = j - pos;
            return true;
        }
    }
}