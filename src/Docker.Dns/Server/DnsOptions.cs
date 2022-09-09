namespace Docker.Dns.Server;

public class DnsOptions
{
    public const string Dns = nameof(Dns);

    public RecordsItem Records { get; set; } = new();

    public class RecordsItem
    {
        public string? A { get; set; }
    }
}