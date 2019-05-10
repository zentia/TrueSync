using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class XMLFragmentElement
	{
		private List<XMLFragmentAttribute> _attributes = new List<XMLFragmentAttribute>();

		private List<XMLFragmentElement> _elements = new List<XMLFragmentElement>();

		public IList<XMLFragmentElement> Elements
		{
			get
			{
				return this._elements;
			}
		}

		public IList<XMLFragmentAttribute> Attributes
		{
			get
			{
				return this._attributes;
			}
		}

		public string Name
		{
			get;
			set;
		}

		public string Value
		{
			get;
			set;
		}

		public string OuterXml
		{
			get;
			set;
		}

		public string InnerXml
		{
			get;
			set;
		}
	}
}
