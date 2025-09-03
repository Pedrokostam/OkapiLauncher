using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OkapiLauncher.Core.Models.Projects;
public static class ProjectReader
{
    static readonly Encoding _encoder = new UTF8Encoding(false);
    private class Header
    {
        public byte[] MagicBytes { get; }
        public ProductType Type { get; }
        public ProductBrand Brand { get; }
        public bool IsXml { get; }
        public Header(ReadOnlySpan<byte> magicByte, ProductType type, ProductBrand brand, bool isXml)
        {
            MagicBytes = magicByte.ToArray();
            Type = type;
            Brand = brand;
            IsXml = isXml;
        }
        public bool MatchesHeader(ReadOnlySpan<byte> headerBytes)
        {
            return headerBytes[..MagicBytes.Length].SequenceEqual(MagicBytes);
        }
    }
    static readonly Header[] _headers = [
            new("<AuroraVisionProject"u8,ProductType.Professional,ProductBrand.Aurora,true),
            new("<AdaptiveVisionProject"u8,ProductType.Professional,ProductBrand.Adaptive,true),
            new("<FabImageProject"u8,ProductType.Professional,ProductBrand.FabImage,true),
            new("AVEXE"u8,ProductType.Runtime,ProductBrand.Aurora,false),
            new("FIEXE"u8,ProductType.Runtime,ProductBrand.FabImage,false),
        ];

    static readonly byte[] _utf8Preamble = Encoding.UTF8.GetPreamble();
    private static Header GetHeader(string path)
    {
        var finfo = new FileInfo(path);
        if (!finfo.Exists)
        {
            throw new FileNotFoundException();
        }
        if (finfo.Length < 100)
        {
            throw new InvalidDataException("The specified file is too small");
        }
        using var stream = finfo.OpenRead();
        Span<byte> preambleBuffer = stackalloc byte[3];
        Span<byte> headerBuffer = stackalloc byte[25];
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(preambleBuffer);
        if (!preambleBuffer.SequenceEqual(_utf8Preamble))
        {
            // File does not start with utf8 preamble, either ??exe or semi-malformed ??proj file.
            // go to start, since those bytes are important
            stream.Seek(0, SeekOrigin.Begin);
        }
        stream.Read(headerBuffer);
        foreach (var entry in _headers)
        {
            if (entry.MatchesHeader(headerBuffer))
            {
                return entry;
            }
        }
        throw new UnknownProjectTypeException(path, nameof(path));
    }

    private static (AvVersion Version, string Name) GetVersionFromXml(string filepath)
    {
        using var reader = XmlReader.Create(filepath);
        reader.Read();
        string? versionStart = reader.GetAttribute("Version");
        string? versionEnd = reader.GetAttribute("Revision");
        AvVersion version;
        if (string.IsNullOrWhiteSpace(versionStart) || string.IsNullOrWhiteSpace(versionEnd))
        {
            version = AvVersion.MissingVersion;
        }
        version = AvVersion.Parse(versionStart + '.' + versionEnd) ?? AvVersion.MissingVersion;
        var name = reader.GetAttribute("Name") ?? Path.GetFileNameWithoutExtension(filepath);
        return (version, name);
    }

    public static VisionProject OpenProject(string filepath)
    {
        var dateModified = File.GetLastWriteTimeUtc(filepath);
        var header = GetHeader(filepath);
        AvVersion version;
        string name;
        if (header.IsXml)
        {
            var xml = GetVersionFromXml(filepath);
            name = xml.Name;
            version = xml.Version;
        }
        else
        {
            // must be avexe, cant read the version
            version = AvVersion.MissingVersion;
            name = Path.GetFileNameWithoutExtension(filepath);
        }

        VisionProject project = new(
            path: filepath,
            brand: header.Brand,
            type: header.Type,
            name: name,
            version: version,
            dateModified:dateModified
            );
        return project;
    }
}