namespace Shared.Data
{
    public class MediaFile : Entity<Guid>
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string Mimetype { get; set; }
    }
}
