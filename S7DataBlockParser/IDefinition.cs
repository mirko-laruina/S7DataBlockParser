using System.Collections.Generic;

namespace S7DataBlockParser
{
    public interface IDefinition
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public List<S7Field> Fields { get; set; }
        public int Size { get; set; }
    }
}