namespace Streamnesia.Configuration;

public class FileSystemPaths : IFileSystemPaths
{
    public FileSystemPaths()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        ApplicationSettingsDirectory = new(Path.Combine(appData, "Streamnesia"));
        Directory.CreateDirectory(ApplicationSettingsDirectory.FullName);
    }

    public DirectoryInfo ApplicationSettingsDirectory { get; private set; }
}
