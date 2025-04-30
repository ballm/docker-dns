using Docker.Dns.Server;

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