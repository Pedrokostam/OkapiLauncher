﻿namespace AuroraVisionLauncher.Core.Models.Apps;

public interface IAppSource
{
    string? Description { get; }
    string SourcePath { get; }
}