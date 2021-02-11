using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
    public class State
    {
        public int Id { get; }
        public string Label { get; }
        public bool IsInitial { get; }
        public bool IsFinal { get; }

        public State(int Id, string Label, bool IsInitial, bool IsFinal)
        {
            this.Id = Id;
            this.Label = Label;
            this.IsInitial = IsInitial;
            this.IsFinal = IsFinal;
        }

        public State(XmlReader Reader)
        {
            this.Id = int.Parse(Reader["Id"]);
            this.Label = Reader["Label"];
            this.IsInitial = bool.Parse(Reader["IsInitial"]);
            this.IsFinal = bool.Parse(Reader["IsFinal"]);
        }

        public bool IsCommonState
        {
            get
            {
                return !IsInitial && !IsFinal;
            }
        }

        public void Save2Xml(XmlWriter Writer)
        {
            Writer.WriteStartElement("State");
            Writer.WriteAttributeString("Id", Id.ToString());
            Writer.WriteAttributeString("Label", Label);        // co se stane s prazdym label
            Writer.WriteAttributeString("IsInitial", IsInitial.ToString());
            Writer.WriteAttributeString("IsFinal", IsFinal.ToString());
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
                if (IsFinal)
                {
                    builder.Append("shape = doublecircle");
                }
                builder.Append("];");
                return builder.ToString();
            }
        }
    }
}
