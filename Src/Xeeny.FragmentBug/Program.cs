using System;
using System.Threading.Tasks;

using Xeeny.Api.Client;
using Xeeny.Api.Server;
using Xeeny.Connections;
using Xeeny.Dispatching;
using Xeeny.Server;
using Xeeny.Sockets;

namespace Xeeny.FragmentBug
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Program
    {
        private static readonly Uri _uri = new UriBuilder("tcp", "localhost", 12345, "FragmentBug").Uri;

        private static readonly byte[] _largeArray = new byte[100000];

        private static void Main()
        {
            var i = 0;

            while (true)
            {
                MainAsync().GetAwaiter().GetResult();
                Console.WriteLine("Success " + (++i));
            }
        }

        private static async Task MainAsync()
        {
            ServiceHost<Service> host = null;
            IService client = null;

            try
            {
                host = new ServiceHostBuilder<Service>(InstanceMode.Single)
                    .AddTcpServer(
                        _uri.AbsoluteUri,
                        options => options.ReceiveTimeout = TimeSpan.FromSeconds(10.0))
                    .WithJsonSerializer()
                    .CreateHost();

                await host.Open().ConfigureAwait(false);
                
                client = await new ConnectionBuilder<IService>()
                    .WithTcpTransport(
                        _uri.AbsoluteUri,
                        options => options.KeepAliveInterval = TimeSpan.FromSeconds(5.0))
                    .WithJsonSerializer()
                    .CreateConnection(true)
                    .ConfigureAwait(false);

                client.Accept(_largeArray);
            }
            finally
            {
                ((IConnection)client)?.Close();
                ((IConnection)client)?.Dispose();

                if (host?.Status == HostStatus.Opened)
                {
                    await host.Close().ConfigureAwait(false);
                }
            }
        }
    }
}
