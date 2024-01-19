using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbQLCSharp.Transpiler
{
    public enum TokenType
    {
        UNKNOWN,
        ID,
        PLUS,
        NEGATIVE,
        MULTIPLY,
        DIVIDE,
        POWER,
        EXP,
        LN,
        SQRT,
        SIGN,
        OPENP,
        CLOSEP,
        NUMBER,
        MODEL,
        TOP,
        OUT,
        OF,
        BATCH,
        LINEAR,
        SOFTMAX,
        BARRIER,
        DATA,
        RAW,
        FILE,
        DESC,
        AS,
        BINARY,
        UNARY,
        VARIABLE,
        CONSTANT,
        ENDOFFILE
    }

    public class Token
    {
        public TokenType Type { get; set; }

        public string Value { get; set; }

        public double NumberValue { get; set; }
    }

    public class SymbQLTokenizer
    {
        StreamReader _reader;

        bool _first = true;
        int _next;

        public SymbQLTokenizer(StreamReader reader)
        {
            _reader = reader;
        }

        public Token NextToken()
        {
            if (_first)
            {
                _first = false;
                _next = _reader.Read();
            }

            while (char.IsWhiteSpace((char)_next))
            {
                _next = _reader.Read();
            }

            if (_next == -1)
                return new Token() { Type = TokenType.ENDOFFILE };

            char ch = (char)_next;

            StringBuilder sb = new StringBuilder();

            if (char.IsLetter(ch))
            {
                while (char.IsLetterOrDigit(ch) || ch == '_')
                {
                    sb.Append(ch);
                    _next = _reader.Read();
                    ch = (char)_next;
                }

                string tokenValue = sb.ToString().ToUpper();

                if (tokenValue == "MODEL")
                {
                    return new Token { Type = TokenType.MODEL };
                }

                if (tokenValue == "TOP")
                {
                    return new Token { Type = TokenType.TOP };
                }

                if (tokenValue == "OUT")
                {
                    return new Token { Type = TokenType.OUT };
                }

                if (tokenValue == "OF")
                {
                    return new Token { Type = TokenType.OF };
                }

                if (tokenValue == "BATCH")
                {
                    return new Token { Type = TokenType.BATCH };
                }

                if (tokenValue == "LINEAR")
                {
                    return new Token { Type = TokenType.LINEAR };
                }

                if (tokenValue == "BARRIER")
                {
                    return new Token { Type = TokenType.BARRIER };
                }

                if (tokenValue == "SOFTMAX")
                {
                    return new Token { Type = TokenType.SOFTMAX };
                }

                if (tokenValue == "DATA")
                {
                    return new Token { Type = TokenType.DATA };
                }

                if (tokenValue == "RAW")
                {
                    return new Token { Type = TokenType.RAW };
                }

                if (tokenValue == "FILE")
                {
                    return new Token { Type = TokenType.FILE };
                }

                if (tokenValue == "DESC")
                {
                    return new Token { Type = TokenType.DESC };
                }

                if (tokenValue == "AS")
                {
                    return new Token { Type = TokenType.AS };
                }

                if (tokenValue == "BINARY")
                {
                    return new Token { Type = TokenType.BINARY };
                }

                if (tokenValue == "UNARY")
                {
                    return new Token { Type = TokenType.UNARY };
                }

                if (tokenValue == "VARIABLE")
                {
                    return new Token { Type = TokenType.VARIABLE };
                }

                if (tokenValue == "CONSTANT")
                {
                    return new Token { Type = TokenType.CONSTANT };
                }

                if (tokenValue == "EXP")
                {
                    return new Token { Type = TokenType.EXP };
                }

                if (tokenValue == "SQRT")
                {
                    return new Token { Type = TokenType.SQRT };
                }

                if (tokenValue == "LN")
                {
                    return new Token { Type = TokenType.LN };
                }

                if (tokenValue == "SIGN")
                {
                    return new Token { Type = TokenType.SIGN };
                }

                return new Token { Type = TokenType.ID, Value = tokenValue };
            }

            if (char.IsDigit(ch) || ch == '.')
            {
                bool foundDecimal = false;
                
                while (char.IsDigit(ch) || ch == '.')
                {
                    if (ch == '.')
                    {
                        if (foundDecimal)
                        {
                            return new Token { Type = TokenType.UNKNOWN };
                        }

                        foundDecimal = true;
                    }

                    sb.Append(ch);
                    _next = _reader.Read();
                    ch = (char)_next;
                }

                string tokenValue = sb.ToString();
                double value = double.Parse(tokenValue);

                return new Token { Type = TokenType.NUMBER, NumberValue = value, Value = tokenValue };
            }

            return new Token { Type = TokenType.UNKNOWN };
        }
    }
}
