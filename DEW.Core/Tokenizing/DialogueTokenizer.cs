using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace DEW.Core
{
    public static class DialogueTokenizer
    {
        private static readonly HashSet<char> PortraitLetters = new HashSet<char> { 'h', 's', 'u', 'l', 'a' };

        private static readonly string[] ArgCommands = //readonly makes this run only once
            new[] { "$action", "$t", "$v" }
                .OrderByDescending(name => name.Length) //orders the string array by lamda order (name length big to small)
                .ToArray();

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

                // check for %revealtaste which is helped by TryMatchArgCommand.
                // propery syntax is %revealtaste either at end of dialogue text or up against # separator, so TryMatchArgCommand fits
                // update: You can have one %reveal command butt against the other, added % next to # to help delineate this syntax further.
                if (c == '%' && TryMatchArgCommand(raw, i, "%revealtaste", out Token? cmd, out int cmdLength, "#%"))
                {
                    FlushText();
                    tokens.Add(cmd);
                    i += cmdLength;
                    continue;
                }

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

                if (c == '[' && TryMatchBracketed(raw, i, out Token? pool, out int poolLength))
                {
                    FlushText();
                    tokens.Add(pool);
                    i += poolLength;
                    continue;
                }

                textBuffer.Append(c);
                i++;
            }

            FlushText();
            return tokens;
        }

        /*
        Helper that Recognizes:
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

            //piroritizes via lambda, so $action doesn't stop reading at $a (the portrait call for angry)
            foreach (var name in ArgCommands)
                if (TryMatchArgCommand(raw, pos, name, out token, out length)) return true;

            bool startOfPiece = pos == 0 || raw[pos - 1] == '#'; //checks to see if raw is the start of dialogue

            if (startOfPiece && TryMatchArgCommand(raw, pos, "$1", out token, out length)) //if start of dialogue and tryargcommand returns true, return trydollarmarker returns true;
                return true;

            if (char.IsDigit(next)) 
            {
                int j = pos + 1;
                while (j < raw.Length && char.IsDigit(raw[j]))
                    j++;

                token = new Token { Kind = SyntaxKind.Portrait, RawText = raw.Substring(pos, j - pos) };
                length = j - pos;
                return true;
            }

            if (PortraitLetters.Contains(next))
            {
                token = new Token { Kind = SyntaxKind.Portrait, RawText = raw.Substring(pos, 2) };
                length = 2;
                return true;
            }

            return false;
        }

        private static bool TryMatchArgCommand(string raw, int pos, string name,
            [NotNullWhen(true)] out Token? token, out int length,
            string stopChars = "#")
        {
            token = null;
            length = 0;

            if (!raw.Substring(pos).StartsWith(name)) return false;

            int end = pos + name.Length; //matches name of $ arg
            if (end < raw.Length && char.IsLetter(raw[end])) return false; //checks the next character so it doesn't immediately match

            //args run until next or until stopchar
            int j = end;
            while (j < raw.Length && stopChars.IndexOf(raw[j]) < 0) j++;

            token = new Token { Kind = SyntaxKind.Structural, RawText = raw.Substring(pos, j - pos) };
            length = j - pos;
            return true;
        }

        private static bool TryMatchBracketed(string raw, int pos,
            [NotNullWhen(true)] out Token? token, out int length)
        {
            token = null;
            length = 0;
            
            if (raw[pos] != '[') return false; //if raw doesn't start with [, return false for this check

            int j = pos +1;
            while (j < raw.Length && raw[j] != ']' && raw[j] != '#')
                j++; //walk through raw while current character is not ] or #

            bool closed = j < raw.Length && raw[j] == ']'; //true if ] is present
            if (closed) j++; //walks past the closed bracket into a new token

            token = new Token
            {
                Kind = closed ? SyntaxKind.Structural : SyntaxKind.Unknown,
                RawText = raw.Substring(pos, j - pos) //if closed, this is structural. If not, it is unknown. Either way, return raw text
            };
            length = j - pos; //resets the current position
            return true;
        }
    }
}