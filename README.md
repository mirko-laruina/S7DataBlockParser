# S7 Data Block (DB) Parser
A simple application developed to parse Data Block files (.db) exported using TIA Portal for the Siemens Simatic S7 PLC family.
It can parse type definitions, structures and arrays and it can calculate the (relative) offset and the size of the fields.

Currently supported data types are: `Bool`,`Byte`,`Char`, `SInt`, `USint`, `Int`, `DInt`, `UDInt`, `Word`, `DWord`, `Real`, `Time`, `String`.

Additional data types can be easily defined (see `S7DataType.cs`).

For what concerns the `String` data type, the size is considered to be `string.Length + 2` since a `string[n]` is usually represented in memory as `| MaxLen (1 byte) | Actual Len (1 byte) | string (n bytes) |`. 