namespace DistNode
{
    using System;
    using System.Threading.Tasks;

    internal sealed class Worker
    {
        private DistCommon.Job.Blueprint job;
        private System.Diagnostics.Process process;
        private bool supressExitEvent;

        public Worker(DistCommon.Job.Blueprint job, ProcessExitedHandler exitedHandler)
        {
            this.job = job;
            this.Awake = false;
            this.ProcessExited += exitedHandler;
            this.supressExitEvent = true;
        }

        public delegate void ProcessExitedHandler(int id);

        public event ProcessExitedHandler ProcessExited;

        public bool Awake { get; private set; }

        public async Task<bool> Start()
        {
            if (!this.Awake)
            {
                this.supressExitEvent = true;
                this.StartWork();
                await Task.Delay(DistCommon.Constants.Node.Worker.CheckDelay);
                this.supressExitEvent = false;
                return this.Awake;
            }

            return false;
        }

        public async Task<bool> Stop()
        {
            if (this.Awake)
            {
                this.supressExitEvent = true;
                this.StopWork();
                await Task.Delay(DistCommon.Constants.Node.Worker.CheckDelay);
                this.supressExitEvent = false;
                return !this.Awake;
            }

            return false;
        }

        private void StartWork()
        {
            this.Awake = true;
            this.process = new System.Diagnostics.Process();
            var startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = this.job.Command;
            ////startInfo.FileName = DistCommon.Constants.Node.Worker.ProcessFilename;
            ////startInfo.Arguments = DistCommon.Constants.Node.Worker.CmdPrefix + this.job.Command;
            this.process.StartInfo = startInfo;
            this.process.Exited += this.OnProcessExited;
            this.process.EnableRaisingEvents = true;
            this.process.Start();
        }

        private void StopWork()
        {
            if (this.Awake)
            {
                this.process.Kill();
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            this.Awake = false;
            if (this.ProcessExited != null && !this.supressExitEvent)
            {
                this.ProcessExited(this.job.ID);
            }
        }
    }
}
