namespace S7DataBlockParser
{
    public class DefinitionSection
    {
        public DefinitionType Type { get; set; }
        public string Raw { get; set; } = "";
    }

    public enum DefinitionType
    {
        Unknown,
        UserDefinedType,
        DataBlock
    }
}