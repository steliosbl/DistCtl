namespace DistNode
{
    using System;

    internal sealed class Worker
    {
        private DistCommon.Job.Blueprint Job;
        private System.Diagnostics.Process Process;
        private System.Threading.Timer Timer;

        public Worker(DistCommon.Job.Blueprint job)
        {
            this.Job = job;
            this.Process = new System.Diagnostics.Process();
            this.Awake = false;
        }

        public bool Awake { get; private set; }

        public void BeginWork()
        {
            this.Awake = true;
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = DistCommon.Constants.Node.Worker.ProcessFilename;
            startInfo.Arguments = DistCommon.Constants.Node.Worker.CmdPrefix + this.Job.Command;
            this.Timer = new System.Threading.Timer(CheckWorking, null, DistCommon.Constants.Node.Worker.TimerInitialWait, DistCommon.Constants.Node.Worker.TimerPeriod);
            this.Process.StartInfo = startInfo;
            this.Process.Start();
        }

        private void CheckWorking(object state)
        {
            this.Awake = !this.Process.HasExited;
        }
    }
}
