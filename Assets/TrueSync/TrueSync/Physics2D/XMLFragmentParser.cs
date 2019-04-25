namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class XMLFragmentParser
    {
        private FileBuffer _buffer;
        private static List<char> _punctuation;
        private XMLFragmentElement _rootNode;

        static XMLFragmentParser()
        {
            List<char> list1 = new List<char> { 0x2f, 60, 0x3e, 0x3d };
            _punctuation = list1;
        }

        public XMLFragmentParser(Stream stream)
        {
            this.Load(stream);
        }

        public XMLFragmentParser(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                this.Load(stream);
            }
        }

        public void Load(Stream stream)
        {
            this._buffer = new FileBuffer(stream);
        }

        public static XMLFragmentElement LoadFromStream(Stream stream)
        {
            XMLFragmentParser parser = new XMLFragmentParser(stream);
            parser.Parse();
            return parser.RootNode;
        }

        private string NextToken()
        {
            bool flag7;
            string str = "";
            bool flag = false;
        Label_0096:
            flag7 = true;
            char next = this._buffer.Next;
            if (_punctuation.Contains(next))
            {
                if (str != "")
                {
                    this._buffer.Position--;
                    goto Label_009E;
                }
                flag = true;
            }
            else if (char.IsWhiteSpace(next))
            {
                if (str != "")
                {
                    goto Label_009E;
                }
                goto Label_0096;
            }
            str = str + next.ToString();
            if (!flag)
            {
                goto Label_0096;
            }
        Label_009E:
            str = this.TrimControl(str);
            if (str[0] == '"')
            {
                str = str.Remove(0, 1);
            }
            if (str[str.Length - 1] == '"')
            {
                str = str.Remove(str.Length - 1, 1);
            }
            return str;
        }

        private void Parse()
        {
            this._rootNode = this.TryParseNode();
            if (this._rootNode == null)
            {
                throw new XMLFragmentException("Unable to load root node");
            }
        }

        private string PeekToken()
        {
            int position = this._buffer.Position;
            string str = this.NextToken();
            this._buffer.Position = position;
            return str;
        }

        private string ReadUntil(char c)
        {
            string str = "";
            while (true)
            {
                char next = this._buffer.Next;
                if (next == c)
                {
                    this._buffer.Position--;
                    if (str[0] == '"')
                    {
                        str = str.Remove(0, 1);
                    }
                    if (str[str.Length - 1] == '"')
                    {
                        str = str.Remove(str.Length - 1, 1);
                    }
                    return str;
                }
                str = str + next.ToString();
            }
        }

        private string TrimControl(string str)
        {
            string str2 = str;
            int startIndex = 0;
            while (true)
            {
                if (startIndex == str2.Length)
                {
                    return str2;
                }
                if (char.IsControl(str2[startIndex]))
                {
                    str2 = str2.Remove(startIndex, 1);
                }
                else
                {
                    startIndex++;
                }
            }
        }

        private string TrimTags(string outer)
        {
            int startIndex = outer.IndexOf('>') + 1;
            int num2 = outer.LastIndexOf('<');
            return this.TrimControl(outer.Substring(startIndex, num2 - startIndex));
        }

        public XMLFragmentElement TryParseNode()
        {
            bool flag6;
            if (this._buffer.EndOfBuffer)
            {
                return null;
            }
            int position = this._buffer.Position;
            string str = this.NextToken();
            if (str != "<")
            {
                throw new XMLFragmentException("Expected \"<\", got " + str);
            }
            XMLFragmentElement element = new XMLFragmentElement {
                Name = this.NextToken()
            };
        Label_0138:
            flag6 = true;
            str = this.NextToken();
            switch (str)
            {
                case ">":
                    break;

                case "/":
                    this.NextToken();
                    element.OuterXml = this.TrimControl(this._buffer.Buffer.Substring(position, this._buffer.Position - position)).Trim();
                    element.InnerXml = "";
                    return element;

                default:
                {
                    XMLFragmentAttribute item = new XMLFragmentAttribute {
                        Name = str
                    };
                    str = this.NextToken();
                    if (str != "=")
                    {
                        throw new XMLFragmentException("Expected \"=\", got " + str);
                    }
                    item.Value = this.NextToken();
                    element.Attributes.Add(item);
                    goto Label_0138;
                }
            }
            while (true)
            {
                int num2 = this._buffer.Position;
                if (this.NextToken() == "<")
                {
                    if (this.PeekToken() == "/")
                    {
                        this.NextToken();
                        str = this.NextToken();
                        this.NextToken();
                        element.OuterXml = this.TrimControl(this._buffer.Buffer.Substring(position, this._buffer.Position - position)).Trim();
                        element.InnerXml = this.TrimTags(element.OuterXml);
                        if (str != element.Name)
                        {
                            string[] textArray1 = new string[] { "Mismatched element pairs: \"", element.Name, "\" vs \"", str, "\"" };
                            throw new XMLFragmentException(string.Concat(textArray1));
                        }
                        return element;
                    }
                    this._buffer.Position = num2;
                    element.Elements.Add(this.TryParseNode());
                }
                else
                {
                    this._buffer.Position = num2;
                    element.Value = this.ReadUntil('<');
                }
            }
        }

        public XMLFragmentElement RootNode
        {
            get
            {
                return this._rootNode;
            }
        }
    }
}

