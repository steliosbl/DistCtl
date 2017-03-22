namespace DistNode
{
    using System;

    internal sealed class Worker
    {
        private DistCommon.Job.Blueprint job;
        private System.Diagnostics.Process process;
        private System.Threading.Timer timer;

        public Worker(DistCommon.Job.Blueprint job)
        {
            this.job = job;
            this.process = new System.Diagnostics.Process();
            this.Awake = false;
        }

        public bool Awake { get; private set; }

        public void BeginWork()
        {
            this.Awake = true;
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = DistCommon.Constants.Node.Worker.ProcessFilename;
            startInfo.Arguments = DistCommon.Constants.Node.Worker.CmdPrefix + this.job.Command;
            this.timer = new System.Threading.Timer(this.CheckWorking, null, DistCommon.Constants.Node.Worker.TimerInitialWait, DistCommon.Constants.Node.Worker.TimerPeriod);
            this.process.StartInfo = startInfo;
            this.process.Start();
        }

        private void CheckWorking(object state)
        {
            this.Awake = !this.process.HasExited;
        }
    }
}
