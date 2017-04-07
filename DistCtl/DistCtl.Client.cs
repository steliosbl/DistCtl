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
            try
            {
                using (var client = new TcpClient())
                {
                    
                    //var task = client.ConnectAsync(this.address, this.port);
                    //if (await Task.WhenAny(task, Task.Delay(this.timeout)) == task)
                    if (true)
                    {
                        await client.ConnectAsync(this.address, this.port);
                        using (var stream = client.GetStream())
                        {
                            stream.ReadTimeout = this.timeout;
                            var data = System.Text.Encoding.ASCII.GetBytes(message);
                            await stream.WriteAsync(data, 0, data.Length);
                            //stream.Write(data, 0, data.Length);

                            byte[] resp = new byte[DistCommon.Constants.Comm.StreamSize];
                            var memStream = new System.IO.MemoryStream();
                            int bytesread = await stream.ReadAsync(resp, 0, resp.Length);
                            while (bytesread > 0)
                            {
                                memStream.Write(resp, 0, bytesread);
                                bytesread = await stream.ReadAsync(resp, 0, resp.Length);
                            }

                            return System.Text.Encoding.ASCII.GetString(memStream.ToArray());

                            //int bytes = 0;
                            //data = new byte[DistCommon.Constants.Comm.StreamSize];
                            //bytes = stream.Read(data, 0, data.Length);
                            //string response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                            //return response;
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
