namespace OkapiLauncher.Core.Models;

public readonly record struct RecentlyOpenedFile(string FilePath, DateTime OpenedOn)
{
    public override int GetHashCode()
    {
        // Customize the hash code generation
        // Example using HashCode.Combine for better distribution
        return string.GetHashCode(FilePath, StringComparison.OrdinalIgnoreCase);
    }
    public RecentlyOpenedFile(string filePath) : this(filePath, DateTime.UtcNow)
    {

    }
    public bool Equals(RecentlyOpenedFile other)
    {
        return string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase);
    }
}
