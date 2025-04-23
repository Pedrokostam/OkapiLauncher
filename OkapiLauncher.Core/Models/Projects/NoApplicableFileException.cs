namespace OkapiLauncher.Core.Models.Projects;

public class NoApplicableFileException(string folderPath) : Exception($"Could not find applicable files in {folderPath}")
{
    public string FolderPath { get; } = folderPath;
}