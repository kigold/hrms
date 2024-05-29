namespace Shared.FileStorage
{
    public interface IFileStorageSetting 
    {
        string BaseDirectory { get; set; }
        string[] AllowedExtensions { get; set; }
    }


    public class FileStorageSetting : IFileStorageSetting
    {
        public string BaseDirectory { get; set; } = string.Empty;
        public string[] AllowedExtensions { get; set; } = new string[] { };
    }
}
