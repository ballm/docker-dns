using DNS.Protocol.ResourceRecords;
using DNS.Protocol;
using System.Net;
using System.Text.RegularExpressions;
using DNS.Client.RequestResolver;

namespace Docker.Dns.Server
{
    public class MasterFile : IRequestResolver
    {
        protected static readonly TimeSpan DefaultTtl = new(0);

        protected static bool Matches(Domain domain, Domain entry)
        {
            var labels = entry.ToString().Split('.');
            var patterns = new string[labels.Length];

            for (var i = 0; i < labels.Length; i++)
            {
                var label = labels[i];
                patterns[i] = label == "*" ? "(?!-)[a-zA-Z0-9-]+(?<=[^-])" : Regex.Escape(label);
            }

            var re = new Regex("^" + string.Join("\\.", patterns) + "$", RegexOptions.IgnoreCase);
            return re.IsMatch(domain.ToString());
        }

        protected static void Merge<T>(IList<T> l1, IList<T> l2)
        {
            foreach (var item in l2)
            {
                l1.Add(item);
            }
        }

        protected IList<IResourceRecord> Entries = new List<IResourceRecord>();
        protected TimeSpan Ttl = DefaultTtl;

        public MasterFile() { }

        public MasterFile(TimeSpan ttl)
        {
            Ttl = ttl;
        }

        public void Add(IResourceRecord entry)
        {
            Entries.Add(entry);
        }

        public void AddIpAddressResourceRecord(string domain, string ip)
        {
            AddIpAddressResourceRecord(new Domain(domain), IPAddress.Parse(ip));
        }

        public void AddIpAddressResourceRecord(Domain domain, IPAddress ip)
        {
            Add(new IPAddressResourceRecord(domain, ip, Ttl));
        }

        public void AddNameServerResourceRecord(string domain, string nsDomain)
        {
            AddNameServerResourceRecord(new Domain(domain), new Domain(nsDomain));
        }

        public void AddNameServerResourceRecord(Domain domain, Domain nsDomain)
        {
            Add(new NameServerResourceRecord(domain, nsDomain, Ttl));
        }

        public void AddCanonicalNameResourceRecord(string domain, string cname)
        {
            AddCanonicalNameResourceRecord(new Domain(domain), new Domain(cname));
        }

        public void AddCanonicalNameResourceRecord(Domain domain, Domain cname)
        {
            Add(new CanonicalNameResourceRecord(domain, cname, Ttl));
        }

        public void AddPointerResourceRecord(string ip, string pointer)
        {
            AddPointerResourceRecord(IPAddress.Parse(ip), new Domain(pointer));
        }

        public void AddPointerResourceRecord(IPAddress ip, Domain pointer)
        {
            Add(new PointerResourceRecord(ip, pointer, Ttl));
        }

        public void AddMailExchangeResourceRecord(string domain, int preference, string exchange)
        {
            AddMailExchangeResourceRecord(new Domain(domain), preference, new Domain(exchange));
        }

        public void AddMailExchangeResourceRecord(Domain domain, int preference, Domain exchange)
        {
            Add(new MailExchangeResourceRecord(domain, preference, exchange));
        }

        public void AddTextResourceRecord(string domain, string attributeName, string attributeValue)
        {
            Add(new TextResourceRecord(new Domain(domain), attributeName, attributeValue, Ttl));
        }

        public void AddServiceResourceRecord(Domain domain, ushort priority, ushort weight, ushort port, Domain target)
        {
            Add(new ServiceResourceRecord(domain, priority, weight, port, target, Ttl));
        }

        public void AddServiceResourceRecord(string domain, ushort priority, ushort weight, ushort port, string target)
        {
            AddServiceResourceRecord(new Domain(domain), priority, weight, port, new Domain(target));
        }

        public Task<IResponse> Resolve(IRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            IResponse response = Response.FromRequest(request);

            foreach (var question in request.Questions)
            {
                var answers = Get(question);

                if (answers.Count > 0)
                {
                    Merge(response.AnswerRecords, answers);
                }
                else
                {
                    response.ResponseCode = ResponseCode.NameError;
                }
            }

            return Task.FromResult(response);
        }

        protected IList<IResourceRecord> Get(Domain domain, RecordType type)
        {
            return Entries.Where(e => Matches(domain, e.Name) && (e.Type == type || type == RecordType.ANY)).ToList();
        }

        protected IList<IResourceRecord> Get(Question question)
        {
            return Get(question.Name, question.Type);
        }
    }
}
