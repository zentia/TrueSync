using System;
using System.Collections.Generic;
using System.IO;

namespace TrueSync.Physics2D
{
	internal class XMLFragmentParser
	{
		private static List<char> _punctuation = new List<char>
		{
			'/',
			'<',
			'>',
			'='
		};

		private FileBuffer _buffer;

		private XMLFragmentElement _rootNode;

		public XMLFragmentElement RootNode
		{
			get
			{
				return this._rootNode;
			}
		}

		public XMLFragmentParser(Stream stream)
		{
			this.Load(stream);
		}

		public XMLFragmentParser(string fileName)
		{
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				this.Load(fileStream);
			}
		}

		public void Load(Stream stream)
		{
			this._buffer = new FileBuffer(stream);
		}

		public static XMLFragmentElement LoadFromStream(Stream stream)
		{
			XMLFragmentParser xMLFragmentParser = new XMLFragmentParser(stream);
			xMLFragmentParser.Parse();
			return xMLFragmentParser.RootNode;
		}

		private string NextToken()
		{
			string text = "";
			bool flag = false;
			while (true)
			{
				char next = this._buffer.Next;
				bool flag2 = XMLFragmentParser._punctuation.Contains(next);
				if (flag2)
				{
					bool flag3 = text != "";
					if (flag3)
					{
						break;
					}
					flag = true;
				}
				else
				{
					bool flag4 = char.IsWhiteSpace(next);
					if (flag4)
					{
						bool flag5 = text != "";
						if (flag5)
						{
							goto Block_4;
						}
						continue;
					}
				}
				text += next.ToString();
				bool flag6 = flag;
				if (flag6)
				{
					goto Block_5;
				}
			}
			FileBuffer expr_43 = this._buffer;
			int position = expr_43.Position;
			expr_43.Position = position - 1;
			Block_4:
			Block_5:
			text = this.TrimControl(text);
			bool flag7 = text[0] == '"';
			if (flag7)
			{
				text = text.Remove(0, 1);
			}
			bool flag8 = text[text.Length - 1] == '"';
			if (flag8)
			{
				text = text.Remove(text.Length - 1, 1);
			}
			return text;
		}

		private string PeekToken()
		{
			int position = this._buffer.Position;
			string result = this.NextToken();
			this._buffer.Position = position;
			return result;
		}

		private string ReadUntil(char c)
		{
			string text = "";
			while (true)
			{
				char next = this._buffer.Next;
				bool flag = next == c;
				if (flag)
				{
					break;
				}
				text += next.ToString();
			}
			FileBuffer expr_25 = this._buffer;
			int position = expr_25.Position;
			expr_25.Position = position - 1;
			bool flag2 = text[0] == '"';
			if (flag2)
			{
				text = text.Remove(0, 1);
			}
			bool flag3 = text[text.Length - 1] == '"';
			if (flag3)
			{
				text = text.Remove(text.Length - 1, 1);
			}
			return text;
		}

		private string TrimControl(string str)
		{
			string text = str;
			int num = 0;
			while (true)
			{
				bool flag = num == text.Length;
				if (flag)
				{
					break;
				}
				bool flag2 = char.IsControl(text[num]);
				if (flag2)
				{
					text = text.Remove(num, 1);
				}
				else
				{
					num++;
				}
			}
			return text;
		}

		private string TrimTags(string outer)
		{
			int num = outer.IndexOf('>') + 1;
			int num2 = outer.LastIndexOf('<');
			return this.TrimControl(outer.Substring(num, num2 - num));
		}

		public XMLFragmentElement TryParseNode()
		{
			bool endOfBuffer = this._buffer.EndOfBuffer;
			XMLFragmentElement result;
			if (endOfBuffer)
			{
				result = null;
			}
			else
			{
				int position = this._buffer.Position;
				string text = this.NextToken();
				bool flag = text != "<";
				if (flag)
				{
					throw new XMLFragmentException("Expected \"<\", got " + text);
				}
				XMLFragmentElement xMLFragmentElement = new XMLFragmentElement();
				xMLFragmentElement.Name = this.NextToken();
				while (true)
				{
					text = this.NextToken();
					bool flag2 = text == ">";
					if (flag2)
					{
						break;
					}
					bool flag3 = text == "/";
					if (flag3)
					{
						goto Block_4;
					}
					XMLFragmentAttribute xMLFragmentAttribute = new XMLFragmentAttribute();
					xMLFragmentAttribute.Name = text;
					bool flag4 = (text = this.NextToken()) != "=";
					if (flag4)
					{
						goto Block_5;
					}
					xMLFragmentAttribute.Value = this.NextToken();
					xMLFragmentElement.Attributes.Add(xMLFragmentAttribute);
				}
				while (true)
				{
					int position2 = this._buffer.Position;
					text = this.NextToken();
					bool flag5 = text == "<";
					if (flag5)
					{
						text = this.PeekToken();
						bool flag6 = text == "/";
						if (flag6)
						{
							break;
						}
						this._buffer.Position = position2;
						xMLFragmentElement.Elements.Add(this.TryParseNode());
					}
					else
					{
						this._buffer.Position = position2;
						xMLFragmentElement.Value = this.ReadUntil('<');
					}
				}
				this.NextToken();
				text = this.NextToken();
				this.NextToken();
				xMLFragmentElement.OuterXml = this.TrimControl(this._buffer.Buffer.Substring(position, this._buffer.Position - position)).Trim();
				xMLFragmentElement.InnerXml = this.TrimTags(xMLFragmentElement.OuterXml);
				bool flag7 = text != xMLFragmentElement.Name;
				if (flag7)
				{
					throw new XMLFragmentException(string.Concat(new string[]
					{
						"Mismatched element pairs: \"",
						xMLFragmentElement.Name,
						"\" vs \"",
						text,
						"\""
					}));
				}
				result = xMLFragmentElement;
				return result;
				Block_4:
				this.NextToken();
				xMLFragmentElement.OuterXml = this.TrimControl(this._buffer.Buffer.Substring(position, this._buffer.Position - position)).Trim();
				xMLFragmentElement.InnerXml = "";
				result = xMLFragmentElement;
				return result;
				Block_5:
				throw new XMLFragmentException("Expected \"=\", got " + text);
			}
			return result;
		}

		private void Parse()
		{
			this._rootNode = this.TryParseNode();
			bool flag = this._rootNode == null;
			if (flag)
			{
				throw new XMLFragmentException("Unable to load root node");
			}
		}
	}
}
