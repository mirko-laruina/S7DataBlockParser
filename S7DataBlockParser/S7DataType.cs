
namespace S7DataBlockParser
{
    public class S7DataType : IDataType
    {
        public string Name { get; set; }
        public int Size { get; set; }
        public int Alignment { get; set; }
        
        public char MemoryType { get; set; }
    }

    public class BoolDataType : S7DataType
    {
        public BoolDataType()
        {
            Name = "Bool";
            Size = 1;
            Alignment = 1;
            MemoryType = 'X';
        }
    }

    public class IntDataType : S7DataType
    {
        public IntDataType()
        {
            Name = "Int";
            Size = 16;
            Alignment = 16;
            MemoryType = 'W';
        }
    }
    
    public class DIntDataType : S7DataType
    {
        public DIntDataType()
        {
            Name = "DInt";
            Size = 32;
            Alignment = 16;
            MemoryType = 'D';
        }
    }
    
    public class UDIntDataType : S7DataType
    {
        public UDIntDataType()
        {
            Name = "UDInt";
            Size = 32;
            Alignment = 16;
            MemoryType = 'D';
        }
    }

    public class StringDataType : S7DataType
    {
        public StringDataType()
        {
            Name = "String";
            Size = 0;
            Alignment = 16;
            MemoryType = 'S';
        }
    }

    public class WStringDataType : S7DataType
    {
        public WStringDataType()
        {
            Name = "WString";
            Size = 0;
            Alignment = 16;
            MemoryType = 'S';
        }
    }

    public class CharDataType : S7DataType
    {
        public CharDataType()
        {
            Name = "Char";
            Size = 8;
            Alignment = 8;
            MemoryType = 'B';
        }
    }
    
    public class ByteDataType : S7DataType
    {
        public ByteDataType()
        {
            Name = "Byte";
            Size = 8;
            Alignment = 8;
            MemoryType = 'B';
        }
    }
    
    public class USIntDataType : S7DataType
    {
        public USIntDataType()
        {
            Name = "USInt";
            Size = 8;
            Alignment = 8;
            MemoryType = 'B';
        }
    }
    
    public class SIntDataType : S7DataType
    {
        public SIntDataType()
        {
            Name = "SInt";
            Size = 8;
            Alignment = 8;
            MemoryType = 'B';
        }
    }
    
    public class WordDataType : S7DataType
    {
        public WordDataType()
        {
            Name = "Word";
            Size = 16;
            Alignment = 16;
            MemoryType = 'W';
        }
    }
    
    public class DWordDataType : S7DataType
    {
        public DWordDataType()
        {
            Name = "DWord";
            Size = 32;
            Alignment = 16;
            MemoryType = 'D';
        }
    }
    
    public class RealDataType : S7DataType
    {
        public RealDataType()
        {
            Name = "Real";
            Size = 32;
            Alignment = 16;
            MemoryType = 'D';
        }
    }

    public class TimeDataType : S7DataType
    {
        public TimeDataType()
        {
            Name = "Time";
            Size = 32;
            Alignment = 16;
            MemoryType = 'D';
        }
    }
}