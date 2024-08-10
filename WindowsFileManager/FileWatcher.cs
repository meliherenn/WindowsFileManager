using System;
using System.IO;

public class FileWatcher
{
    private FileSystemWatcher watcher;

    public string Path { get; }

    public event FileSystemEventHandler OnFileChanged;

    public FileWatcher(string path)
    {
        Path = path;
        watcher = new FileSystemWatcher(path);
        watcher.EnableRaisingEvents = true;
        watcher.Changed += (s, e) => OnFileChanged?.Invoke(s, e);
        watcher.Created += (s, e) => OnFileChanged?.Invoke(s, e);
        watcher.Deleted += (s, e) => OnFileChanged?.Invoke(s, e);
        watcher.Renamed += (s, e) => OnFileChanged?.Invoke(s, e);
    }
}
