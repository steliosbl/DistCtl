namespace DistNode
{
    using System;

    internal sealed class Worker
    {
        private DistCommon.Job.Blueprint job;
        private System.Diagnostics.Process process;
        private bool supressExitEvent;

        public Worker(DistCommon.Job.Blueprint job, ProcessExitedHandler exitedHandler)
        {
            this.job = job;
            this.process = new System.Diagnostics.Process();
            this.Awake = false;
            this.ProcessExited += exitedHandler;
            this.supressExitEvent = true;
        }

        public delegate void ProcessExitedHandler(int id);

        public event ProcessExitedHandler ProcessExited;

        public bool Awake { get; private set; }

        public void StartWork()
        {
            if (!this.Awake)
            {
                this.supressExitEvent = false;
                this.Awake = true;
                var startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.CreateNoWindow = true;
                startInfo.FileName = DistCommon.Constants.Node.Worker.ProcessFilename;
                startInfo.Arguments = DistCommon.Constants.Node.Worker.CmdPrefix + this.job.Command;
                this.process.StartInfo = startInfo;
                this.process.Exited += this.OnProcessExited;
                this.process.EnableRaisingEvents = true;
                this.process.Start();
            }
        }

        public void StopWork()
        {
            if (this.Awake)
            {
                this.supressExitEvent = true;
                this.process.Kill();
                this.Awake = false;
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            if (this.ProcessExited != null && !this.supressExitEvent)
            {
                this.Awake = false;
                this.ProcessExited(this.job.ID);
            }
        }
    }
}
