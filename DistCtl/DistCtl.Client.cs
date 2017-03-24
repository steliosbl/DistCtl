namespace DistCtl
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    internal sealed class Client
    {
        private IPEndPoint remoteEP;
        private int timeout;

        public Client(IPAddress address, int port, EndPointUnreachableHandler unreachableHandler)
        {
            this.remoteEP = new IPEndPoint(address, port);
            this.EndPointUnreachable += unreachableHandler;
        }

        public delegate void EndPointUnreachableHandler(EventArgs e);

        public event EndPointUnreachableHandler EndPointUnreachable;

        public async Task<string> Send(string message)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var task = client.ConnectAsync(this.remoteEP.Address, this.remoteEP.Port);
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
                this.OnEndPointUnreachable(EventArgs.Empty);
                return null;
            }
        }

        private void OnEndPointUnreachable(EventArgs e)
        {
            if (this.EndPointUnreachable != null)
            {
                this.EndPointUnreachable(e);
            }
        }
    }
}
