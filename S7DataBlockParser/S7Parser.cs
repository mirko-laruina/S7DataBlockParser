using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace S7DataBlockParser
{
    public class S7Parser
    {
        private const string TypeDelimiter = "TYPE";
        private const string TypeEndDelimiter = "END_TYPE";
        private const string DataBlockDelimiter = "DATA_BLOCK";
        private const string DataBlockEndDelimiter = "END_DATA_BLOCK";
        private const string VersionIdentifier = "VERSION";
        private const string StructDelimiter = "STRUCT";
        private const string StructEndDelimiter = "END_STRUCT";
        private const string InnerStructDelimiter = "Struct";
        private const string InnerStructEndDelimiter = "END_STRUCT";
        
        private readonly Dictionary<string, IDataType> _definedUserDataTypes;


        public S7Parser()
        {
            _definedUserDataTypes = new Dictionary<string, IDataType>();
            List<S7DataType> basicDataTypes = new();
            basicDataTypes.Add(new BoolDataType());
            basicDataTypes.Add(new ByteDataType());
            basicDataTypes.Add(new CharDataType());
            basicDataTypes.Add(new IntDataType());
            basicDataTypes.Add(new DIntDataType());
            basicDataTypes.Add(new UDIntDataType());
            basicDataTypes.Add(new WordDataType());
            basicDataTypes.Add(new USIntDataType());
            basicDataTypes.Add(new SIntDataType());
            basicDataTypes.Add(new DWordDataType());
            basicDataTypes.Add(new RealDataType());
            basicDataTypes.Add(new TimeDataType());
            basicDataTypes.Add(new StringDataType());
            basicDataTypes.ForEach(d => _definedUserDataTypes.Add(d.Name, d));
        }

        /// <summary>
        /// Parses the given file using the given types.
        /// All the types found are added to definedTypes
        /// All the datablocks found are returned
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<DataBlock> ParseFile(string filePath)
        {
            var dataBlocks = new List<DataBlock>();
            string[] lines;
            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch
            {
                Console.WriteLine($"Unable to read the provided file");
                return null;
            }

            List<DefinitionSection> definitionSections = new();
            DefinitionSection definitionSection = new DefinitionSection();
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(TypeDelimiter))
                {
                    if (definitionSection.Type != DefinitionType.Unknown)
                    {
                        Console.WriteLine($"Error parsing line \"{line}\"");
                    }

                    definitionSection.Type = DefinitionType.UserDefinedType;
                }

                if (trimmedLine.StartsWith("DATA_BLOCK"))
                {
                    if (definitionSection.Type != DefinitionType.Unknown)
                    {
                        Console.WriteLine($"Error parsing line \"{line}\"");
                    }

                    definitionSection.Type = DefinitionType.DataBlock;
                }

                if (definitionSection.Type != DefinitionType.Unknown)
                {
                    definitionSection.Raw += line + "\n";
                }

                if (trimmedLine.StartsWith(TypeEndDelimiter) || trimmedLine.StartsWith("END_DATA_BLOCK"))
                {
                    if (definitionSection.Type == DefinitionType.Unknown)
                    {
                        Console.WriteLine($"Error parsing line \"{line}\"");
                    }

                    definitionSections.Add(definitionSection);
                    definitionSection = new();
                }
            }

            foreach (var defSection in definitionSections)
            {
                var definition = Parse(defSection, _definedUserDataTypes);
                if (definition is UserDataType udt)
                {
                    _definedUserDataTypes[udt.Name] = udt;
                }
                else if(definition is DataBlock db)
                {
                    dataBlocks.Add(db);
                }
            }

            return dataBlocks;
        }

        public static void PrintOffsets(string dbName, int baseOffset, List<S7Field> dbFields)
        {
            foreach (var field in dbFields)
            {
                var offset = baseOffset + field.Offset;
                var path = $"{dbName}.{field.Name}";
                Console.WriteLine($"{path} \t\t\t{offset/8}.{offset%8}");
                if (field.Type is UserDataType udt)
                {
                    PrintOffsets($"{path}", offset, udt.Fields);
                }
                if (field.Type is ArrayDataType array)
                {
                    PrintOffsets($"{path}", offset, array.Fields);
                }
            }
        }

        public static IDefinition Parse(DefinitionSection definitionSection,
            Dictionary<string, IDataType> definedUserDataTypes)
        {
            return definitionSection.Type switch
            {
                DefinitionType.UserDefinedType => ParseUdt(definitionSection, definedUserDataTypes),
                DefinitionType.DataBlock => ParseDB(definitionSection, definedUserDataTypes),
                _ => null
            };
        }

        /// <summary>
        /// This function expect a block with a similar schema
        /// <example><code>
        /// TYPE "UserDefinedTypeName"
        /// VERSION : 0.1
        ///     STRUCT
        ///         Field1 : Type1;
        ///         Field2 : Type2;
        ///         Field3 : Type3;
        ///         Field4 : Type4;
        ///     END_STRUCT;
        ///
        /// END_TYPE
        /// </code></example>
        /// </summary>
        /// <param name="definitionSection"></param>
        /// <returns></returns>
        public static UserDataType ParseUdt(DefinitionSection definitionSection,
            Dictionary<string, IDataType> definedTypes)
        {
            var userDataType = new UserDataType();
            ParseDefinition(definitionSection, TypeDelimiter, TypeEndDelimiter, definedTypes, userDataType);
            return userDataType;
        }

        private static List<S7Field> ParseStruct(List<string> lines, Dictionary<string, IDataType> definedTypes)
        {
            lines = lines.GetRange(1, lines.Count - 2);

            List<S7Field> fields = new List<S7Field>();
            var currentOffset = 0;

            int insideInPlaceStruct = 0;
            List<string> inPlaceStructLines = new();
            UserDataType currentInPlaceStruct = null;
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                var isStructStart = trimmedLine.Contains($": {InnerStructDelimiter}");
                var isStructEnd = trimmedLine.StartsWith(InnerStructEndDelimiter);
                
                if (isStructStart)
                {
                    insideInPlaceStruct += 1;
                }
                
                if (insideInPlaceStruct > 0)
                {
                    inPlaceStructLines.Add(trimmedLine);
                    if (isStructEnd)
                    {
                        insideInPlaceStruct -= 1;
                        if (insideInPlaceStruct == 0)
                        {
                            currentInPlaceStruct = new UserDataType();
                            currentInPlaceStruct.Name = inPlaceStructLines[0].Split(":")[0].Trim();
                            currentInPlaceStruct.Fields = ParseStruct(inPlaceStructLines, definedTypes);
                            var lastField = currentInPlaceStruct.Fields.LastOrDefault();
                            if (lastField is not null)
                            {
                                currentInPlaceStruct.Size = lastField.Offset + lastField.Type.Size;
                            }

                            var field = new S7Field();
                            field.Name = currentInPlaceStruct.Name.Replace("\"", "");
                            field.Type = currentInPlaceStruct;
                            field.Offset = GetAlignedOffset(currentOffset, field.Type.Alignment);
                            currentOffset = field.Offset + field.Type.Size;
                            fields.Add(field);
                            inPlaceStructLines = new();
                        }
                    }
                }
                else if (insideInPlaceStruct == 0 && isStructEnd)
                {
                    throw new InvalidOperationException("Trying to end a struct which was never started");
                }
                else if(!isStructStart)
                {
                    if (trimmedLine.Contains("S7_SetPoint"))
                    {
                        // a default value is specified, we ignore it
                        trimmedLine = Regex.Replace(trimmedLine, @"{ S7_SetPoint.*?}", "");
                    }

                    if (trimmedLine.Contains("//"))
                    {
                        // ignore the comments
                        trimmedLine = Regex.Replace(trimmedLine, @"//.*", "");
                    }

                    var splits = trimmedLine.Split(":");
                    if (splits.Length < 2)
                    {
                        throw new IndexOutOfRangeException(
                            $"Expect a field in the format Name : Type, obtained {trimmedLine}");
                    }
                    var field = new S7Field();
                    field.Name = splits[0].Trim().Replace("\"", "");
                    var typeIdentifier = splits[1].Trim().Replace(";", "").Replace("\"", "");
                    field.Type = GetDataType(definedTypes, typeIdentifier);
                    if (field.Type is null)
                    {
                        throw new NotSupportedException($"The type {typeIdentifier} is not recognized.");
                    }

                    var alignedOffset = GetAlignedOffset(currentOffset, field.Type.Alignment);
                    field.Offset = alignedOffset;
                    currentOffset = alignedOffset + field.Type.Size;
                    fields.Add(field);
                }
            }

            return fields;
        }
        
        private static string GetVersion(string line)
        {
            line = line.Trim();
            var splits = line.Split(":");
            if (splits.Length > 1)
            {
                return splits[1].Trim();
            }

            return "";
        }

        private static string GetNameFromDefinition(string identifier, string line)
        {
            line = line.Trim();
            var index = line.IndexOf(identifier, StringComparison.Ordinal);
            var name = line.Substring(index + identifier.Length).Trim();
            // remove the " from the name
            if (name.Length > 2)
            {
                name = name.Substring(1, name.Length - 2);
            }

            return name;
        }

        private static int GetAlignedOffset(int currentSize, int typeAlignment)
        {
            var remainder = currentSize % typeAlignment;
            if (currentSize % typeAlignment == 0)
            {
                // already aligned
                return currentSize;
            }

            // eg size is 29, alignement to 16
            // 29 % 16 = 13
            // result is 29 + (16 - 13) = 32
            return currentSize + typeAlignment - remainder;
        }

        private static IDataType GetDataType(Dictionary<string, IDataType> definedTypes, string typeString)
        {
            if (typeString.StartsWith("String"))
            {
                // Could be something in the form String[20] or a simple String (meaning 256 bytes long)
                var splits = typeString.Split("[");
                var length = splits.Length == 1 ? 254 : int.Parse(splits[1].Replace("]", ""));
                var stringType = new StringDataType
                {
                    Size = (length + 2) * 8 // we use bits
                };
                return stringType;
            } 
            
            if (typeString.StartsWith("Array"))
            {
                // Something in the form of Array[0..3] of "User_data_type_1"
                // OR Array[1..10] of "User_data_type_2"
                var splits = typeString.Split(" of ");
                var arrayString = splits[0].Trim();
                var dataTypeString = splits[1].Replace("\"", "").Trim();
                var dataType = definedTypes.FirstOrDefault(t => t.Key == dataTypeString).Value;

                var matches = new Regex(@"Array\[(?<startIndex>.*)\.\.(?<endIndex>.*)\]").Match(arrayString);
                if (matches.Success)
                {
                    var startIndex = int.Parse(matches.Groups["startIndex"].Value);
                    var endIndex = int.Parse(matches.Groups["endIndex"].Value);
                    var array = new ArrayDataType()
                    {
                        Alignment = 16,
                        Size = dataType.Size * (1 + endIndex - startIndex),
                        Name = typeString
                    };
                    for(var i = startIndex; i <= endIndex; ++i)
                    {
                        array.Fields.Add(new S7Field()
                        {
                            Name = $"[{i}]",
                            Offset = (i - startIndex) * dataType.Size,
                            Type = dataType
                        });
                    }

                    return array;
                }
                

            }

            return definedTypes.FirstOrDefault(t => t.Key == typeString).Value;
        }

        public static DataBlock ParseDB(DefinitionSection definitionSection,
            Dictionary<string, IDataType> definedTypes)
        {
            var dataBlock = new DataBlock();
            ParseDefinition(definitionSection, DataBlockDelimiter, DataBlockEndDelimiter, definedTypes, dataBlock);
            return dataBlock;
        }

        private static IDefinition ParseDefinition(DefinitionSection definitionSection, string startDelimiter, string endDelimiter, Dictionary<string, IDataType> definedTypes, IDefinition definition)
        {
            var lines = definitionSection.Raw.Split("\n").ToList();
            lines = lines.Select(l => l.Trim()).Where(l => l != "").ToList();

            // We perform a scan to find the keywords.
            // This forces us to iterate multiple times over the lines list, but it is more readable
            int startIndex = -1;
            int endIndex = -1;
            int versionIndex = -1;
            int structStartIndex = -1;
            int structEndIndex = -1;
            for (var index = 0; index < lines.Count; index++)
            {
                var line = lines[index];
                var trimmedLine = line.Trim();
                if (trimmedLine.StartsWith(startDelimiter))
                {
                    startIndex = index;
                }
                else if (trimmedLine.StartsWith(endDelimiter))
                {
                    endIndex = index;
                }
                else if (trimmedLine.StartsWith(VersionIdentifier))
                {
                    versionIndex = index;
                }
                else if (trimmedLine.StartsWith(StructDelimiter))
                {
                    structStartIndex = index;
                }
                else if (trimmedLine.StartsWith(StructEndDelimiter))
                {
                    structEndIndex = index;
                }
            }

            // Check everything is correct
            if (endIndex <= startIndex || structEndIndex <= structStartIndex)
            {
                throw new ArgumentException("An unexpected definition has been encountered");
            }

            definition.Name = GetNameFromDefinition(startDelimiter, lines[startIndex]);
            definition.Version = GetVersion(lines[versionIndex]);
            definition.Fields = ParseStruct(lines.GetRange(structStartIndex, structEndIndex - structStartIndex + 1),
                definedTypes);
            var lastField = definition.Fields.LastOrDefault();
            if (lastField is not null)
            {
                definition.Size = definition.Size + lastField.Offset + lastField.Type.Size;
            }

            return definition;
        }
    }
}