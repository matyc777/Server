using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class CommandTranslator
    {
        public static List<string> Parse(string message)// "signup:matyc:123qwe456"   "login:matyc:123qwe456"
        {
            List<string> InstructionArray = new List<string>();
            String[] substrings = message.Split(':');
            foreach(string substring in substrings)
            {
                InstructionArray.Add(substring);
            }
            return InstructionArray;
        }

        public static string Encode(List<string> ClientsNames)
        {
            StringBuilder str = new StringBuilder();
            foreach(string Name in ClientsNames)
            {
                str.Append(":" + Name);
            }
            return str.ToString();
        }
    }
}
