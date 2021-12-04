using System.Collections.Generic;

namespace S7DataBlockParser
{
    public class UserDataType : IDefinition, IDataType
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public List<S7Field> Fields { get; set; } = new();
        public int Size { get; set; }
        public int Alignment { get; set; } = 16;
    }
}