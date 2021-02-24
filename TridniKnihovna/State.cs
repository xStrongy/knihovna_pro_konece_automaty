using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
    public class State
    {
        public int Id { get; init; }

        private string mLabel;
        public bool IsInitial { get; init; }
        public bool IsAccept { get; init; }

        public string Label
        {
            get
            {
                if (string.IsNullOrEmpty(mLabel))
                    return Id.ToString();
                return mLabel;
            }
            init
            {
                mLabel = value;
            }
        }
       public State(int Id, string Label, bool IsInitial, bool IsAccept)
        {
            this.Id = Id;
            this.Label = Label;
            this.IsInitial = IsInitial;
            this.IsAccept = IsAccept;
        }

        public State(XmlNode Node)
        {
            Id = int.Parse(Node.Attributes["Id"].Value);
            mLabel = Node.Attributes["Label"] != null ? Node.Attributes["Label"].Value.Trim() : string.Empty;
            IsInitial = Node.Attributes["IsInitial"] != null ? bool.Parse(Node.Attributes["IsInitial"].Value) : false;
            IsAccept = Node.Attributes["IsAccept"] != null ? bool.Parse(Node.Attributes["IsAccept"].Value) : false;
        }

        public bool IsCommonState
        {
            get
            {
                return !IsInitial && !IsAccept;
            }
        }

        public void Save2Xml(XmlWriter Writer)
        {
            Writer.WriteStartElement("State");
            Writer.WriteAttributeString("Id", Id.ToString());
            if (!string.IsNullOrEmpty(mLabel))
            {
                Writer.WriteAttributeString("Label", mLabel);
            }
            if (IsInitial)
            {
                Writer.WriteAttributeString("IsInitial", IsInitial.ToString());
            }
            if (IsAccept)
            {
                Writer.WriteAttributeString("IsAccept", IsAccept.ToString());
            }
            Writer.WriteEndElement();
        }

        public string GraphvizSourceCode
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Id);
                builder.Append("[");
                if (!string.IsNullOrEmpty(Label))
                {
                    builder.AppendFormat("label=\"{0}\"", Label);
                }
                if (IsAccept)
                {
                    builder.Append("shape = doublecircle");
                }
                builder.Append("];");
                return builder.ToString();
            }
        }

        public void Report()
        {
            Console.WriteLine("({0}, {1}, {2}, {3})", Id, mLabel, IsInitial, IsAccept);
        }
    }
}
