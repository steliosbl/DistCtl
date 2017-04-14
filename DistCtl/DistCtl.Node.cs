namespace DistCtl
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DistCommon.Constants;
    using Newtonsoft.Json;
    using Comm = DistCommon.Comm;

    internal sealed class Node
    {
        public readonly DistCommon.Schema.Node Schematic;
        private Client client;
        private Timer reportTimer;
        private int retryCounter;
        private int reportDelay;

        public Node(DistCommon.Schema.Node schematic, int reportDelay, int timeout, LostNodeHandler lostNodeHandler, RecoveredNodeHandler recoveredNodeHandler, WorkerExitedHandler workerExitedHandler)
        {
            this.Reachable = false;
            this.reportDelay = reportDelay;
            this.Schematic = schematic;
            this.retryCounter = 0;
            this.LostNode += lostNodeHandler;
            this.RecoveredNode += recoveredNodeHandler;
            this.WorkerExited += workerExitedHandler;
            this.client = new Client(this.Schematic.Address, timeout);
        }

        public delegate void LostNodeHandler(int id);

        public delegate void RecoveredNodeHandler(int id);

        public delegate void WorkerExitedHandler(int jobID);

        public event LostNodeHandler LostNode;

        public event RecoveredNodeHandler RecoveredNode;

        public event WorkerExitedHandler WorkerExited;

        public bool Reachable { get; private set; }

        public async Task<int> Assign(Job job)
        {
            var res = await this.SendRequest(new Comm.Requests.Assign(job.Blueprint));
            return res != null ? res.ResponseCode : Results.Unreachable;
        }

        public async Task<int> Construct()
        {
            var res = await this.SendRequest(new Comm.Requests.Construct(this.Schematic));
            return res != null ? res.ResponseCode : Results.Unreachable;
        }

        public async Task<int> Remove(int id)
        {
            var res = await this.SendRequest(new Comm.Requests.Remove(id));
            return res != null ? res.ResponseCode : Results.Unreachable;
        }

        public async Task<int> Reset()
        {
            var res = await this.SendRequest(new Comm.Requests.Reset());
            return res != null ? res.ResponseCode : Results.Unreachable;
        }

        public async Task<int> Sleep(int id)
        {
            var res = await this.SendRequest(new Comm.Requests.Sleep(id));
            return res != null ? res.ResponseCode : Results.Unreachable;
        }

        public async Task<int> Wake(int id)
        {
            var res = await this.SendRequest(new Comm.Requests.Wake(id));
            return res != null ? res.ResponseCode : Results.Unreachable;
        }

        public async Task<bool> Test()
        {
            var res = await this.SendRequest(new Comm.Requests.Base());
            return res != null ? res.ResponseCode == Results.Success : false;
        }

        public void StartReportTimer()
        {
            this.reportTimer = new Timer(this.CheckReports, null, this.reportDelay, this.reportDelay);
        }

        public void StopReportTimer()
        {
            this.reportTimer.Dispose();
        }

        private void CheckReports(object state)
        {
            var reports = this.SendRequest<Comm.Responses.Report>(new Comm.Requests.Report()).Result;
            if (reports != null)
            {
                this.Reachable = true;
                if (reports.Reports.Count != 0)
                {
                    foreach (var report in reports.Reports)
                    {
                        if (report.ReportType == typeof(Comm.Reports.WorkerExited))
                        {
                            this.HandleReport((Comm.Reports.WorkerExited)report);
                        }
                    }
                }
            }
        }

        private void HandleReport(Comm.Reports.WorkerExited report)
        {
            this.WorkerExited(report.ID);
        }

        private async Task<Comm.Responses.Base> SendRequest(Comm.Requests.Base request)
        {
            return await this.SendRequest<Comm.Responses.Base>(request);
        }

        private void SendFailedHandler()
        {
            this.retryCounter++;
            if (this.retryCounter == Ctl.RequestAttempts)
            {
                this.Reachable = false;
                this.LostNode(this.Schematic.ID);
            }
        }

        private void SendSuccessfulHandler()
        {
            if (!this.Reachable)
            {
                this.Reachable = true;
                this.RecoveredNode(this.Schematic.ID);
            }
        }

        private async Task<T> SendRequest<T>(Comm.Requests.Base request)
        {
            while (this.retryCounter < 3)
            {
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.MissingMemberHandling = MissingMemberHandling.Error;

                    string responseString = await this.client.Send(JsonConvert.SerializeObject(request));
                    if (responseString == null)
                    {
                        this.SendFailedHandler();
                    }
                    else
                    {
                        this.SendSuccessfulHandler();
                        if (responseString != DistCommon.Constants.Comm.InvalidResponse)
                        {
                            this.retryCounter = 0;
                            var baseResponse = JsonConvert.DeserializeObject<Comm.Responses.Base>(responseString);
                            if (baseResponse.ResponseType == typeof(T))
                            {
                                return JsonConvert.DeserializeObject<T>(responseString);
                            }
                        }
                        else
                        {
                            throw new JsonException();
                        }
                    }
                }
                catch (JsonException)
                {
                }
            }

            return default(T);
        }
    }
}
