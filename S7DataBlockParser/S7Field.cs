namespace S7DataBlockParser
{
    public class S7Field
    {
        public string Name { get; set; }
        public IDataType Type { get; set; }
        public int Offset { get; set; }
    }
}