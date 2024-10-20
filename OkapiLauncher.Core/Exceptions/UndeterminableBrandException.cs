using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuroraVisionLauncher.Core.Models;

namespace AuroraVisionLauncher.Core.Exceptions;

public class UndeterminableBrandException:Exception
{
    public enum BrandSource
    {
        Exe,
        Header,
        License,
        AllApplicable
    }
    public UndeterminableBrandException(string filepath, AvType? type, BrandSource checkedSource):
        base($"Cannot determine brand for this filepath: {filepath}")
    {
        Filepath = filepath;
        Type = type;
        CheckedSource = checkedSource;
    }
    public static UndeterminableBrandException ForAllApplicable(string filepath, AvType? type)
    {
        return new UndeterminableBrandException(filepath, type, BrandSource.AllApplicable);
    }
    public static UndeterminableBrandException ForHeader(string filepath)
    {
        return new UndeterminableBrandException(filepath, AvType.Library, BrandSource.Header);
    }
    public static UndeterminableBrandException ForLicense(string filepath, AvType? type)
    {
        return new UndeterminableBrandException(filepath, type, BrandSource.License);
    }
    public static UndeterminableBrandException ForExe(string filepath, AvType? type)
    {
        return new UndeterminableBrandException(filepath, type, BrandSource.Exe);
    }

    public string Filepath { get; }
    public AvType? Type { get; }
    public BrandSource CheckedSource { get; }
}
