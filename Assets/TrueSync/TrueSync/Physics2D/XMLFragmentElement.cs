// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.XMLFragmentElement
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

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
                return (IList<XMLFragmentElement>)this._elements;
            }
        }

        public IList<XMLFragmentAttribute> Attributes
        {
            get
            {
                return (IList<XMLFragmentAttribute>)this._attributes;
            }
        }

        public string Name { get; set; }

        public string Value { get; set; }

        public string OuterXml { get; set; }

        public string InnerXml { get; set; }
    }
}
