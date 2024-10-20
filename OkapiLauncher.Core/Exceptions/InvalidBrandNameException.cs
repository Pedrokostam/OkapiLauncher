namespace OkapiLauncher.Core.Exceptions;

public class InvalidBrandNameException : Exception
{
    public InvalidBrandNameException(string name) : base($"Given name does not match any brand: {name}")
    {
        Name = name;
    }

    public string Name { get; }
}