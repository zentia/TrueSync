namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class XMLFragmentElement
    {
        private List<XMLFragmentAttribute> _attributes = new List<XMLFragmentAttribute>();
        private List<XMLFragmentElement> _elements = new List<XMLFragmentElement>();
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <InnerXml>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <Name>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <OuterXml>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string <Value>k__BackingField;

        public IList<XMLFragmentAttribute> Attributes
        {
            get
            {
                return this._attributes;
            }
        }

        public IList<XMLFragmentElement> Elements
        {
            get
            {
                return this._elements;
            }
        }

        public string InnerXml { get; set; }

        public string Name { get; set; }

        public string OuterXml { get; set; }

        public string Value { get; set; }
    }
}

