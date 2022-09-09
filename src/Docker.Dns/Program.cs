using Docker.Dns.Server;

namespace Docker.Dns
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(hostConfig =>
                {
                    hostConfig.AddEnvironmentVariables(prefix: "DNS_");
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<DnsOptions>(context.Configuration.GetSection(DnsOptions.Dns));

                    services.AddHostedService<DnsService>();
                })
                .Build()
                .RunAsync();
        }
    }
}