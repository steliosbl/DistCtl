namespace DistCtl
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    internal sealed class Client
    {
        private IPAddress address;
        private int port;
        private int timeout;

        public Client(IPEndPoint address, int timeout)
        {
            this.address = address.Address;
            this.port = address.Port;
            this.timeout = timeout;
        }

        public async Task<string> Send(string message)
        {
            var task = this.SendMsg(message);
            var timeout = Task.Delay(this.timeout);
            var res = await Task.WhenAny(task, timeout);
            return res == timeout ? null : task.Result;
        }

        private async Task<string> SendMsg(string message)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync(this.address, this.port);
                    using (var stream = client.GetStream())
                    {
                        var data = System.Text.Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);

                        data = new byte[DistCommon.Constants.Comm.StreamSize];
                        int bytes = stream.Read(data, 0, data.Length);
                        string response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                        return response;
                    }

                    throw new SocketException();
                }
            }
            catch (Exception e)
            {
                if (e is SocketException || e is InvalidOperationException)
                {
                    return null;
                }

                throw;
            }
        }
    }
}
