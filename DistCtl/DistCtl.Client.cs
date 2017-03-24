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

        public Client(IPEndPoint address)
        {
            this.address = address.Address;
            this.port = address.Port;
        }

        public async Task<string> Send(string message)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(this.address, this.port);
                    if (await Task.WhenAny(task, Task.Delay(this.timeout)) == task)
                    {
                        await task;
                        using (var stream = client.GetStream())
                        {
                            var data = System.Text.Encoding.ASCII.GetBytes(message);
                            stream.Write(data, 0, data.Length);

                            data = new byte[DistCommon.Constants.Comm.StreamSize];
                            int bytes = stream.Read(data, 0, data.Length);
                            string response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                            return response;
                        }
                    }

                    throw new SocketException();
                }
            }
            catch (SocketException)
            {
                return null;
            }
        }
    }
}
