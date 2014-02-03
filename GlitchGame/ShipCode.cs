using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace GlitchGame
{
    public class ShipCode
    {
        private readonly byte[] _program;

        public int Length
        {
            get { return _program.Length; }
        }

        public byte this[int i]
        {
            get { return _program[i]; }
        }

        public readonly ShipDebugInfo DebugInfo;

        public ShipCode(string fileName)
        {
            _program = File.ReadAllBytes(fileName);

            try
            {
                DebugInfo = new ShipDebugInfo(Path.ChangeExtension(fileName, ".dbg"));
            }
            catch
            {
                DebugInfo = null;
            }
        }
    }

    public class ShipDebugInfo
    {
        public struct Symbol
        {
            public readonly int NameOffset;
            public readonly int Address;

            public readonly string Name;

            public Symbol(int nameOffset, int address)
            {
                NameOffset = nameOffset;
                Address = address;
                Name = null;
            }

            public Symbol(Symbol symbol, string name)
            {
                NameOffset = symbol.NameOffset;
                Address = symbol.Address;
                Name = name;
            }
        }

        public struct Line
        {
            public readonly int FileNameOffset;
            public readonly int LineNumber;
            public readonly int Address;

            public readonly string FileName;

            public Line(int fileNameOffset, int lineNumber, int address)
            {
                FileNameOffset = fileNameOffset;
                LineNumber = lineNumber;
                Address = address;
                FileName = null;
            }

            public Line(Line line, string fileName)
            {
                FileNameOffset = line.FileNameOffset;
                LineNumber = line.LineNumber;
                Address = line.Address;
                FileName = fileName;
            }
        }

        private List<Symbol> _symbols;
        private List<Line> _lines;

        public ReadOnlyCollection<Symbol> Symbols
        {
            get { return _symbols.AsReadOnly(); }
        }

        public ReadOnlyCollection<Line> Lines
        {
            get { return _lines.AsReadOnly(); }
        }

        public ShipDebugInfo(string fileName)
        {
            using (var file = File.OpenRead(fileName))
            using (var reader = new BinaryReader(file))
            {
                var header = reader.ReadInt32();
                if (header != 0x30474244)
                    throw new Exception("Invalid header");

                var symbolsOffset = reader.ReadInt32();
                var symbolsCount = reader.ReadInt32();
                var linesOffset = reader.ReadInt32();
                var linesCount = reader.ReadInt32();
                
                _symbols = new List<Symbol>(symbolsCount);
                for (var i = 0; i < symbolsCount; i++)
                {
                    var symbol = new Symbol(reader.ReadInt32(), reader.ReadInt32());
                    _symbols.Add(symbol);
                }

                _lines = new List<Line>(linesCount);
                for (var i = 0; i < linesCount; i++)
                {
                    var line = new Line(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
                    _lines.Add(line);
                }

                var stringTable = new Dictionary<int, string>();
                while (file.Position < file.Length)
                {
                    var offset = (int)file.Position;
                    var str = ReadNullTerminated(reader);

                    stringTable.Add(offset, str);
                }

                for (var i = 0; i < _symbols.Count; i++)
                {
                    var s = _symbols[i];
                    _symbols[i] = new Symbol(s, stringTable[s.NameOffset]);
                }

                for (var i = 0; i < _lines.Count; i++)
                {
                    var l = _lines[i];
                    _lines[i] = new Line(l, stringTable[l.FileNameOffset]);
                }
            }
        }

        public Symbol? FindSymbol(int address)
        {
            for (var i = _symbols.Count - 1; i >= 0; i--)
            {
                var s = _symbols[i];
                if (s.Name.Contains('.'))
                    continue;
                if (s.Address <= address)
                    return s;
            }

            return null;
        }

        public Line? FindLine(int address)
        {
            foreach (var l in _lines)
            {
                if (l.Address == address)
                    return l;
            }

            return null;
        }

        private static string ReadNullTerminated(BinaryReader reader)
        {
            var result = new StringBuilder();
            byte value;

            while ((value = reader.ReadByte()) != 0)
            {
                result.Append((char)value);
            }

            return result.ToString();
        }
    }
}
