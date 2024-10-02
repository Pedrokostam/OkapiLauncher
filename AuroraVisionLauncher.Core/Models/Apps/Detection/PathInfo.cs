namespace AuroraVisionLauncher.Core.Models.Apps.Detection;

internal record struct PathInfo(DirectoryInfo BasePath, AvType Type)
{
    public static implicit operator (DirectoryInfo basePath, AvType type)(PathInfo value)
    {
        return (value.BasePath, value.Type);
    }

    public static implicit operator PathInfo((DirectoryInfo basePath, AvType type) value)
    {
        return new PathInfo(value.basePath, value.type);
    }
    public static PathInfo FromFilepath(string filepath)
    {
        var typ = GetTypeFromFilename(filepath);

        return new PathInfo();
    }
    private static AvType GetTypeFromFilename(string filepath)
    {
        string name = Path.GetFileName(filepath).ToLowerInvariant();
        return name switch
        {
            "AdaptiveVisionStudio.exe" => AvType.Professional,
            "AuroraVisionStudio.exe" => AvType.Professional,
            "FabImageStudio.exe" => AvType.Professional,
            //
            "AdaptiveVisionExecutor.exe" => AvType.Runtime,
            "AuroraVisionExecutor.exe" => AvType.Runtime,
            "FabImageExecutor.exe" => AvType.Runtime,
            //
            "AVL.dll" => AvType.Library,
            "FIL.dll" => AvType.Library,
            //
            "DeepLearningEditor.exe" => AvType.DeepLearning,
            _ => throw new NotSupportedException()
        };
    }
}
