using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TridniKnihovna
{
    public class XmlAutomataReader
    {
       public static NondeterministicFiniteAutomaton ReadFromXml(string xmlPath)
        {

            StreamReader xmlStreamReader = new StreamReader(xmlPath);
            XmlDocument xmlDoc = new XmlDocument();

            xmlDoc.Load(xmlStreamReader);
            XmlNodeList nodes = xmlDoc.DocumentElement.ChildNodes;

            string type = xmlDoc.DocumentElement.GetAttribute("Type");

            if(!type.Equals("Nondeterministic"))
            {
                Console.WriteLine("Automaton cannot be read, because it is not nondeterministic");
                return null;
            }

            List<State> states = new List<State>();

            foreach (XmlNode stateNode in nodes.Item(0))
            {
                State s = new State(stateNode);
                states.Add(s);
            }

            XmlNode alphabetNode = nodes.Item(1);
            string Alphabet = alphabetNode.InnerText;

            List<DeltaFunctionTriplet> dft = new List<DeltaFunctionTriplet>();
            foreach(XmlNode dftNode in nodes.Item(2))
            {
                int From = int.Parse(dftNode.Attributes["From"].Value);
                char By = char.Parse(dftNode.Attributes["By"].Value);
                int To = int.Parse(dftNode.Attributes["To"].Value);

                dft.Add(new DeltaFunctionTriplet(From, By, To));
            }

            SortedList<int, List<int>> EpsilonTransition = new SortedList<int, List<int>>();
            foreach(XmlNode edfp in nodes.Item(3))
            {
                int From = int.Parse(edfp.Attributes["From"].Value);
                int To = int.Parse(edfp.Attributes["To"].Value);

                if(EpsilonTransition.TryGetValue(From, out List<int> value))
                {
                    value.Add(To);
                }
                else
                {
                    value = new List<int>();
                    value.Add(To);
                    EpsilonTransition.Add(From, value);
                }

            }
            return new NondeterministicFiniteAutomaton(states, Alphabet, dft, EpsilonTransition);
        }
    }
}
