using System.Collections.Generic;

namespace S7DataBlockParser
{
    public class ArrayDataType : IDataType
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int Alignment { get; set; }
        
        public List<S7Field> Fields { get; set; } = new();
    }
}