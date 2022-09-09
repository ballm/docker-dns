using System.Net;
using DNS.Client.RequestResolver;
using DNS.Server;
using Microsoft.Extensions.Options;

namespace Docker.Dns.Server
{
    public class DnsService : IHostedService
    {
        private static readonly IPAddress ListenAddress = IPAddress.Any;
        private const int ListenPort = 53;
        private readonly ILogger<DnsService> _logger;
        private readonly DnsOptions _dnsOptions;
        private DnsServer? _server;

        public DnsService(ILogger<DnsService> logger, IOptions<DnsOptions> dnsOptions)
        {
            _logger = logger;
            _dnsOptions = dnsOptions.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var records = BuildMasterFile(_dnsOptions.Records, _logger);

            _server = CreateServer(records, _logger);

            await _server.Listen(ListenPort, ListenAddress);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.Dispose();

            return Task.CompletedTask;
        }

        private static MasterFile BuildMasterFile(DnsOptions.RecordsItem records, ILogger logger)
        {
            var aRecords = records.A?.Split(",") ?? Array.Empty<string>();
            logger.LogInformation("Number of A Records Defined: {Number}", aRecords.Length);

            var masterFile = new MasterFile();

            foreach (var aRecord in aRecords)
            {
                var record = aRecord.Split(':');

                if (record.Length != 2)
                {
                    logger.LogWarning("A records should be defined as <domain>:<IP>. But found '{Record}' which will be ignored.", aRecord);
                    continue;
                }

                logger.LogInformation("Adding A record definition '{Record}'.", aRecord);
                masterFile.AddIpAddressResourceRecord(record[0], record[1]);
            }

            return masterFile;
        }

        private static DnsServer CreateServer(IRequestResolver requestResolver, ILogger logger)
        {
            var server = new DnsServer(requestResolver);

            server.Listening += (_, _)
                => logger.LogInformation("DNS Server listening on {ListenAddress}:{ListenPort}", ListenAddress, ListenPort);

            server.Requested += (_, e) => logger.LogInformation("REQUEST: {Request}", e.Request);

            server.Responded += (_, e) => logger.LogInformation("RESPONSE: {Request} => {Response}", e.Request, e.Response);

            server.Errored += (_, e) => logger.LogError(e.Exception, "ERROR: {ErrorMessage}", e.Exception.Message);

            return server;
        }
    }
}
