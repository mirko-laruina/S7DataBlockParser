namespace S7DataBlockParser
{
    public interface IDataType
    {
        public string Name { get; set; }
        
        /// <summary>
        /// Size in BITS. Bits are used in order to store an integer (easier math operation without precision issues) 
        /// </summary>
        public int Size { get; set; }
        
        /// <summary>
        /// Alignment in BITS. Bits are used in order to store an integer (easier math operation without precision issues) 
        /// </summary>
        public int Alignment { get; set; }
    }
}