using System.IO;
using System.Text;

namespace LiteDB
{

    #region JsonToken

    internal enum JsonTokenType
    {
        BeginDoc,
        EndDoc,
        BeginArray,
        EndArray,
        Comma,
        Colon,
        String,
        Number,
        Word,
        EOF
    }

    internal class JsonToken
    {
        public string Token { get; set; }
        public JsonTokenType TokenType { get; set; }

        public void Expect(JsonTokenType type)
        {
            if (TokenType != type)
            {
                throw LiteException.UnexpectedToken(Token);
            }
        }

        public void Expect(JsonTokenType type1, JsonTokenType type2)
        {
            if (TokenType != type1 && TokenType != type2)
            {
                throw LiteException.UnexpectedToken(Token);
            }
        }
    }

    #endregion JsonToken

    /// <summary>
    ///     Class that parse a json string and returns in json token
    /// </summary>
    internal class JsonTokenizer
    {
        private readonly TextReader _reader;
        private char _current = '\0';

        public JsonTokenizer(TextReader reader)
        {
            _reader = reader;
            Position = 0;
            Read();
        }

        public bool EOF { get; private set; }
        public long Position { get; private set; }

        /// <summary>
        ///     Read next char in stream and set in _current
        /// </summary>
        private char Read()
        {
            if (EOF) return '\0';

            var c = _reader.Read();

            Position++;

            if (c == -1)
            {
                _current = '\0';
                EOF = true;
            }

            _current = (char) c;

            return _current;
        }

        /// <summary>
        ///     Read next json token
        /// </summary>
        public JsonToken ReadToken()
        {
            EatWhitespace();

            if (EOF)
            {
                return new JsonToken {TokenType = JsonTokenType.EOF};
            }

            JsonToken token = null;

            switch (_current)
            {
                case '[':
                    token = new JsonToken {TokenType = JsonTokenType.BeginArray, Token = "["};
                    Read();
                    break;

                case ']':
                    token = new JsonToken {TokenType = JsonTokenType.EndArray, Token = "]"};
                    Read();
                    break;

                case '{':
                    token = new JsonToken {TokenType = JsonTokenType.BeginDoc, Token = "{"};
                    Read();
                    break;

                case '}':
                    token = new JsonToken {TokenType = JsonTokenType.EndDoc, Token = "}"};
                    Read();
                    break;

                case ':':
                    token = new JsonToken {TokenType = JsonTokenType.Colon, Token = ":"};
                    Read();
                    break;

                case ',':
                    token = new JsonToken {TokenType = JsonTokenType.Comma, Token = ","};
                    Read();
                    break;

                case '\"':
                    token = new JsonToken {TokenType = JsonTokenType.String, Token = ReadString()};
                    break;

                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    token = new JsonToken {TokenType = JsonTokenType.Number, Token = ReadNumber()};
                    break;

                default:
                    token = new JsonToken {TokenType = JsonTokenType.Word, Token = ReadWord()};
                    break;
            }

            return token;
        }

        /// <summary>
        ///     Eat all whitespaces - used before a valid token
        /// </summary>
        private void EatWhitespace()
        {
            while (char.IsWhiteSpace(_current) && !EOF)
            {
                Read();
            }
        }

        /// <summary>
        ///     Read a word without "
        /// </summary>
        private string ReadWord()
        {
            var sb = new StringBuilder();
            sb.Append(_current);

            Read();

            while (!EOF &&
                   (char.IsLetterOrDigit(_current) || _current == '_' || _current == '$'))
            {
                sb.Append(_current);
                Read();
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Read a number - it's accepts all number char, but not validate. When run Convert, .NET will check if number is
        ///     correct
        /// </summary>
        private string ReadNumber()
        {
            var sb = new StringBuilder();
            sb.Append(_current);

            Read();

            while (!EOF &&
                   (char.IsDigit(_current) || _current == '+' || _current == '-' || _current == '.' || _current == 'e' ||
                    _current == 'E'))
            {
                sb.Append(_current);
                Read();
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Read a string removing open and close "
        /// </summary>
        private string ReadString()
        {
            var sb = new StringBuilder();
            Read(); // remove first "

            while (_current != '\"' && !EOF)
            {
                if (_current == '\\')
                {
                    Read();

                    switch (_current)
                    {
                        case '"':
                            sb.Append('"');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '/':
                            sb.Append('/');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'u':
                            var codePoint = ParseUnicode(Read(), Read(), Read(), Read());
                            sb.Append((char) codePoint);
                            break;
                    }
                }
                else
                {
                    sb.Append(_current);
                }

                Read();
            }

            Read(); // read last "

            return sb.ToString();
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            var p1 = ParseSingleChar(c1, 0x1000);
            var p2 = ParseSingleChar(c2, 0x100);
            var p3 = ParseSingleChar(c3, 0x10);
            var p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private uint ParseSingleChar(char c1, uint multipliyer)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint) (c1 - '0')*multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint) (c1 - 'A' + 10)*multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint) (c1 - 'a' + 10)*multipliyer;
            return p1;
        }
    }
}