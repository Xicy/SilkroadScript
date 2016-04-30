using System.Collections.Generic;

namespace Shared.Structs.Data
{
    public struct DivisionServer
    {
        public DivisionServer(string name,byte locale)
        {
            LoginServers = new List<string>();
            Name = name;
            Locale = locale;
        }

        public List<string> LoginServers { get; private set; }
        public string Name { get; private set; }
        public byte Locale { get; private set; }
    }
}