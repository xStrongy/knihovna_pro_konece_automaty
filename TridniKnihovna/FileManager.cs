using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Text;
using Newtonsoft.Json;

namespace TridniKnihovna
{
    public class FileManager
    {
        private const string dataPath = @"C:\Users\Strongy\source\repos\Knihovna_pro_praci_s_konecnymi_automaty\saved\";
        public void saveToJson(DFA automata)
        {
            string context = JsonConvert.SerializeObject(automata);
            File.WriteAllText(dataPath + "dfa\\" + automata.Name + ".json", context);
        }

        public void saveToJson(NFA automata)
        {
            string context = JsonConvert.SerializeObject(automata);
            File.WriteAllText(dataPath + "nfa\\" + automata.Name + ".json", context);
        }

        public DFA loadDFAFromJson(string name)
        {
            if (!(File.Exists(dataPath + "dfa\\" + name + ".json")))
            {
                Console.WriteLine("Soubor neexistuje");
                return null;
            }

            DFA dfa = JsonConvert.DeserializeObject<DFA>(File.ReadAllText(dataPath + "dfa\\" + name + ".json"));
            return dfa;
        }

        public NFA loadNFAFromJson(string name)
        {
            if (!(File.Exists(dataPath + "mfa\\" + name + ".json")))
            {
                Console.WriteLine("Soubor neexistuje");
                return null;
            }

            NFA nfa = JsonConvert.DeserializeObject<NFA>(File.ReadAllText(dataPath + "nfa\\" + name + ".json"));
            return nfa;
        }
    }
}
