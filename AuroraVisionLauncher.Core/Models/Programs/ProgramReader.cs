using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AuroraVisionLauncher.Core.Models.Programs;
public static class ProgramReader
{
    static readonly Encoding _encoder = new UTF8Encoding(false);
    static readonly Dictionary<ProgramType, byte[]> _headers = new(){
        { ProgramType.AuroraVisionProject,_encoder.GetBytes("<AuroraVisionProject")},
        { ProgramType.AdaptiveVisionProject,_encoder.GetBytes("<AdaptiveVisionProject")},
        { ProgramType.FabImageProject,_encoder.GetBytes("<FabImageProject")},
        { ProgramType.AuroraVisionRuntime,_encoder.GetBytes("AVEXE")},
        { ProgramType.FabImageRuntime,_encoder.GetBytes("FIEXE")},

    };
    static readonly byte[] Utf8Preamble = Encoding.UTF8.GetPreamble();
    public static ProgramType CheckFile(string path)
    {
        var finfo = new FileInfo(path);
        if (!finfo.Exists)
        {
            throw new FileNotFoundException();
        }
        if (finfo.Length < 250)
        {
            throw new InvalidDataException("The specified file is too small");
        }
        using var stream = finfo.OpenRead();
        Span<byte> preambleBuffer = stackalloc byte[3];
        Span<byte> headerBuffer = stackalloc byte[25];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(preambleBuffer);
        if (!preambleBuffer.SequenceEqual(Utf8Preamble))
        {
            // File does not start with utf8 preamble, either ??exe or semi-malformed ??proj file.
            // go to start, since those bytes are important
            stream.Seek(0, SeekOrigin.Begin);
        }
        stream.Read(headerBuffer);
        foreach (var entry in _headers)
        {
            if (EqualsHeader(headerBuffer, entry.Value))
            {
                return entry.Key;
            }
        }
        return ProgramType.None;
    }

    private static bool EqualsHeader(Span<byte> span, byte[] header)
    {
#if DEBUG
        string spanstr = _encoder.GetString(span[..header.Length]);
        string headerstr = _encoder.GetString(header);
#endif
        return span[..header.Length].SequenceEqual(header);
    }
    private static AvVersion GetVersionFromXml(string filepath)
    {
        using var reader = XmlReader.Create(filepath);
        reader.Read();
        string? versionStart = reader.GetAttribute("Version");
        string? versionEnd = reader.GetAttribute("Revision");
        if (string.IsNullOrWhiteSpace(versionStart) || string.IsNullOrWhiteSpace(versionEnd))
        {
            return AvVersion.MissingVersion;
        }
        return AvVersion.Parse(versionStart + '.' + versionEnd) ?? AvVersion.MissingVersion;
    }

    public static ProgramInformation GetInformation(string filepath)
    {
        var programType = CheckFile(filepath);
        IAvVersion version = programType switch
        {
            ProgramType.None => throw new InvalidDataException("Format does not match any headers."),
            ProgramType.FabImageRuntime or ProgramType.AuroraVisionRuntime => AvVersion.MissingVersion,
            _ => GetVersionFromXml(filepath)
        };
        return new ProgramInformation(programType, version);
    }
}