namespace DistCtl
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DistCommon;
    using DistCommon.Logging;

    public sealed class Controller : IController
    {
        #region Fields
        private Config config;
        private ConcurrentDictionary<int, Job> jobs;
        private ConcurrentDictionary<int, Node> nodes;
        private DistCommon.Schema.Controller schematic;
        private Logger logger;
        private bool ignoreAllEvents = false;
        #endregion

        #region Constructors
        public Controller(Config config, ExitCommandHandler exitHandler, Logger.SayHandler sayHandler)
        {
            this.config = config;
            this.jobs = new ConcurrentDictionary<int, DistCtl.Job>();
            this.nodes = new ConcurrentDictionary<int, DistCtl.Node>();
            this.logger = new Logger(DistCommon.Constants.Ctl.LogFilename, Source.Ctl, sayHandler);
            this.ExitCommand += exitHandler;
        }
        #endregion

        #region Events
        public delegate void ExitCommandHandler();

        private delegate void DistributionChangedHandler();

        public event ExitCommandHandler ExitCommand;

        private event DistributionChangedHandler DistributionChanged;
        #endregion

        #region Properties
        private int TotalSlots
        {
            get
            {
                return this.nodes.Values.Where(node => node.Reachable).Sum(node => node.Schematic.Slots);
            }
        }

        private int TotalSlotsAvailable
        {
            get
            {
                return this.TotalSlots - this.jobs.Count(job => job.Value.NodeID != -1);
            }
        }
        #endregion

        #region Exposed
        public Task<Result> Add(DistCommon.Job.Blueprint job, Source src)
        {
            return this.AddJob(new DistCtl.Job(job, -1, false));
        }

        public Task<Result> Add(DistCommon.Schema.Node schematic, Source src)
        {
            return this.AddNode(schematic);
        }

        public Task<Result> Assign(int jobID, Source src)
        {
            return this.AssignJobBalanced(jobID);
        }

        public Task<Result> Assign(int jobID, int nodeID, Source src)
        {
            return this.AssignJobManual(jobID, nodeID);
        }

        public bool Exit()
        {
            return this.ExitController().Result;
        }

        public JobInfo GetJob(int id)
        {
            return this.GetJobInfo(id);
        }

        public Dictionary<int, JobInfo> GetJob()
        {
            return this.GetJobsInfo();
        }

        public NodeInfo GetNode(int id)
        {
            return this.GetNodeInfo(id);
        }

        public Dictionary<int, NodeInfo> GetNode()
        {
            return this.GetNodesInfo();
        }

        public bool Initialize()
        {
            return this.StartInit().Result;
        }

        public Task<Result> Remove(int nodeID, Source src)
        {
            return this.RemoveNode(nodeID);
        }

        public Task<Result> Remove(int jobID, int nodeID, Source src)
        {
            return this.RemoveJob(jobID);
        }

        public Task<Result> Sleep(int jobID, Source src)
        {
            return this.SleepJob(jobID);
        }

        public Task<Result> Wake(int jobID, Source src)
        {
            return this.WakeJob(jobID);
        }
        #endregion

        #region Intermediate
        #region Init
        private async Task<bool> StartInit()
        {
            this.logger.Log("Initializing controller...");
            this.logger.Log("Loading dependencies...");
            bool preloadPossible = true;
            {
                var dependencies = new List<string>();
                dependencies.Add(this.config.SchematicFilename);
                if (this.config.EnableJobPreload)
                {
                    dependencies.Add(this.config.PreLoadFilename);
                }

                var missingFiles = new DepMgr(dependencies.ToArray()).FindMissing();
                if (missingFiles.Contains(this.config.SchematicFilename))
                {
                    this.logger.Log("Schematic file not found.", Severity.Critical);
                    Environment.Exit(2);
                }

                if (missingFiles.Contains(this.config.PreLoadFilename))
                {
                    this.logger.Log("Pre-load file not found.", Severity.Warn);
                    preloadPossible = false;
                }
            }

            this.logger.Log("Loading schematic...");
            {
                this.schematic = JFI.GetObject<DistCommon.Schema.Controller>(this.config.SchematicFilename);
                await this.LoadNodes();

                if (this.nodes.Count == 0)
                {
                    this.logger.Log("All nodes failed to initialize.", Severity.Critical);
                    Environment.Exit(3);
                }
            }

            {
                if (this.config.EnableJobPreload && preloadPossible)
                {
                    this.logger.Log("Loading jobs...");
                    await this.LoadJobs();
                }
                else if (!preloadPossible)
                {
                    this.logger.Log("Skipping job pre-load");
                }
            }

            {
                if (this.config.EnableLoadBalancing)
                {
                    this.DistributionChanged += this.BalanceWithMsg;
                    this.DistributionChanged();
                }
            }

            this.logger.Log("Controller ready");

            return true;
        }
        #endregion

        private async Task<Result> AddJob(Job job)
        {
            return await this.AddJob(job, null);
        }

        private async Task<Tuple<int, Result>> AddJob(int jobID, Job job)
        {
            return new Tuple<int, Result>(jobID, await this.AddJob(job));
        }

        private async Task<Result> AddJob(Job job, int? nodeID)
        {
            if (!this.jobs.ContainsKey(job.Blueprint.ID) && job.Blueprint.ID > 0)
            {
                this.jobs.TryAdd(job.Blueprint.ID, job);
                Result res;
                if (nodeID == null)
                {
                    res = await this.AssignJobBalanced(job.Blueprint.ID);
                }
                else
                {
                    res = await this.AssignJobManual(job.Blueprint.ID, (int)nodeID);
                }

                if (res != Result.Success)
                {
                    this.jobs.TryRemove(job.Blueprint.ID, out job);
                }

                return res;
            }

            return Result.Invalid;
        }

        private async Task<Result> AddNode(DistCommon.Schema.Node schematic)
        {
            if (!this.nodes.ContainsKey(schematic.ID) && schematic.ID > 0)
            {
                var node = new Node(schematic, this.config.UpdateDelay, this.config.TimeoutDuration, this.LostNodeHandler, this.RecoveredNodeHandler, this.WorkerExitedHandler);
                if (await node.Test())
                {
                    Result constructionRes = await node.Construct();
                    if ((constructionRes == Result.Invalid && await node.Reset() == Result.Success && await node.Construct() == Result.Success) || constructionRes == Result.Success)
                    {
                        if (this.nodes.TryAdd(node.Schematic.ID, node))
                        {
                            this.nodes[node.Schematic.ID].StartReportTimer();
                            return Result.Success;
                        }

                        return Result.Fail;
                    }
                }

                return Result.Unreachable;
            }

            return Result.Invalid;
        }

        private async Task<Tuple<int, Result>> AddNode(DistCommon.Schema.Node schematic, int id)
        {
            return new Tuple<int, Result>(id, await this.AddNode(schematic));
        }

        private async Task<Result> AssignJobBalanced(int jobID, params int[] exempt)
        {
            var l = new List<int>();
            l.Add(jobID);
            var res = await this.AssignJobsBalanced(l, exempt);
            return res.Count == 0 ? Result.Success : res[jobID];
        }

        private async Task<Result> AssignJobManual(int jobID, int nodeID)
        {
            if (this.nodes.ContainsKey(nodeID))
            {
                if (this.nodes[nodeID].Reachable)
                {
                    Result res = await this.nodes[nodeID].Assign(this.jobs[jobID]);
                    
                    if (res == Result.Success)
                    {
                        this.jobs[jobID].Transfer(nodeID);
                    }

                    return res;
                }

                return Result.Fail;
            }

            return Result.NotFound;
        }

        private async Task<bool> ExitController()
        {
            this.logger.Log("Exiting controller...", Severity.Warn);
            this.logger.Log("Resetting nodes...", Severity.Warn);
            await this.ResetAll();
            this.nodes.Clear();
            this.logger.Log("Shutting down interfaces", Severity.Warn);
            this.ExitCommand();
            this.logger.Log("Done");
            return true;
        }

        private async Task<Result> RemoveJob(int jobID)
        {
            if (this.jobs.ContainsKey(jobID))
            {
                int nodeID = this.jobs[jobID].NodeID;
                Result res = await this.nodes[nodeID].Remove(jobID);
                if (res == Result.Success)
                {
                    Job job;
                    this.jobs.TryRemove(jobID, out job);
                }

                return res;
            }

            return Result.NotFound;
        }

        private async Task<Result> RemoveNode(int nodeID)
        {
            if (this.nodes.ContainsKey(nodeID))
            {
                Result res = await this.nodes[nodeID].Reset();
                if (res == Result.Success)
                {
                    await this.AssignJobsBalanced(this.GetAssignedJobs(nodeID).Select(job => job.NodeID).ToList(), nodeID);
                    Node temp;
                    this.nodes.TryRemove(nodeID, out temp);
                }

                return res;
            }

            return Result.NotFound;
        }

        private JobInfo GetJobInfo(int id)
        {
            if (this.jobs.ContainsKey(id))
            {
                return this.jobs[id].Info;
            }

            return null;
        }

        private Dictionary<int, JobInfo> GetJobsInfo()
        {
            return this.jobs.ToDictionary(job => job.Key, job => job.Value.Info);
        }

        private NodeInfo GetNodeInfo(int id)
        {
            if (this.nodes.ContainsKey(id))
            {
                return this.nodes[id].Info;
            }

            return null;
        }

        private Dictionary<int, NodeInfo> GetNodesInfo()
        {
            return this.nodes.ToDictionary(node => node.Key, node => node.Value.Info);
        }

        private async Task<Result> SleepJob(int jobID)
        {
            if (this.jobs.ContainsKey(jobID))
            {
                int nodeID = this.jobs[jobID].NodeID;
                if (this.jobs[jobID].Awake)
                {
                    Result res = await this.nodes[nodeID].Sleep(jobID);
                    if (res == Result.Success)
                    {
                        this.jobs[jobID].Sleep();
                    }

                    return res;
                }

                return Result.Invalid;
            }

            return Result.NotFound;
        }

        private async Task<Result> WakeJob(int jobID)
        {
            if (this.jobs.ContainsKey(jobID))
            {
                int nodeID = this.jobs[jobID].NodeID;
                if (!this.jobs[jobID].Awake)
                {
                    Result res = await this.nodes[nodeID].Wake(jobID);
                    if (res == Result.Success)
                    {
                        this.jobs[jobID].Wake();
                    }

                    return res;
                }

                return Result.Invalid;
            }

            return Result.NotFound;
        }

        private async Task<Tuple<int, Result>> WakeJob(bool tuple, int jobID)
        {
            return new Tuple<int, Result>(jobID, await this.WakeJob(jobID));
        }
        #endregion

        #region Internal
        #region Init
        private async Task<bool> LoadJobs()
        {
            var jobs = JFI.GetObject<List<DistCommon.Job.Blueprint>>(this.config.PreLoadFilename);
            //// var assignTasks = jobs.Select(job => this.AddJob(job.Blueprint.ID, job).ContinueWith((t) => this.JobAssignMsg(t.Result)));
            //// var wakeTasks = jobs.Where(job => job.Awake).Select(job => this.WakeJob(true, job.Blueprint.ID).ContinueWith((t) => this.JobWakeMsg(t.Result)));
            var tasks = jobs.Select(job => this.LoadJob(new Job(job, 0, false)));
            await Task.WhenAll(tasks);
            return true;
        }

        private async Task<bool> LoadNodes()
        {
            var tasks = this.schematic.Nodes.Select(node => this.AddNode(node.Value, node.Key).ContinueWith((t) => this.NodeMsg(t.Result)));
            await Task.WhenAll(tasks);
            return true;
        }

        private async Task<bool> LoadJob(Job job)
        {
            await this.AddJob(job.Blueprint.ID, job).ContinueWith((t) => this.JobAssignMsg(t.Result));
            if (this.config.EnableWakePreloadJobs)
            {
                await this.WakeJob(true, job.Blueprint.ID).ContinueWith((t) => this.JobWakeMsg(t.Result));
            }

            return true;
        }

        private void NodeMsg(Tuple<int, Result> res)
        {
            if (res.Item2 == Result.Success)
            {
                this.logger.Log(string.Format("Node ID: {0} initialized successfully", res.Item1));
            }
            else
            {
                this.logger.Log(string.Format("Node ID: {0} failed to initialize", res.Item1), Severity.Warn);
            }
        }

        private void JobAssignMsg(Tuple<int, Result> res)
        {
            if (res.Item2 == Result.Success)
            {
                this.logger.Log(string.Format("Loaded job ID: {0}", res.Item1));
            }
            else
            {
                this.logger.Log(string.Format("Failed to load job ID: {0}", res.Item1), Severity.Warn);
            }
        }

        private void JobWakeMsg(Tuple<int, Result> res)
        {
            if (res.Item2 == Result.Success)
            {
                this.logger.Log(string.Format("Awoke job ID: {0}", res.Item1));
            }
            else
            {
                this.logger.Log(string.Format("Failed to wake job ID: {0}", res.Item1), Severity.Warn);
            }
        }
        #endregion

        private async Task<Dictionary<int, Result>> AssignJobsBalanced(List<int> jobIDs, params int[] exempt)
        {
            var res = new Dictionary<int, Result>();
            if ((this.config.EnableRejectTooManyAssignments && jobIDs.Count <= this.TotalSlotsAvailable) || !this.config.EnableRejectTooManyAssignments)
            {
                var nodes = this.nodes.Where(node => node.Value.Reachable && !exempt.Contains(node.Key)).ToDictionary(node => node.Key, node => (float)this.GetAssignedJobs(node.Key).Count / node.Value.Schematic.Slots);
                if (nodes.Count != 0)
                {
                    jobIDs = jobIDs.OrderBy(job => this.jobs[job].Blueprint.Priority).ToList();

                    int slotCount = this.TotalSlotsAvailable;
                    while (jobIDs.Count > 0 && slotCount > 0)
                    {
                        var min = nodes.Aggregate((l, r) => l.Value < r.Value ? l : r);
                        if (min.Value != 1)
                        {
                            int jobID = jobIDs[0];
                            Result assignRes = await this.AssignJobManual(jobID, min.Key);
                            if (assignRes == Result.Success)
                            {
                                nodes[min.Key] = (float)this.GetAssignedJobs(min.Key).Count / this.nodes[min.Key].Schematic.Slots;
                                slotCount -= 1;
                            }

                            jobIDs.RemoveAt(0);
                            res.Add(jobID, assignRes);
                        }
                        else
                        {
                            break;
                        }
                    }

                    return res;
                }
            }

            return jobIDs.ToDictionary(job => job, job => Result.Fail);
        }

        private async Task<bool> AttemptRestart(int id)
        {
            if (this.jobs.ContainsKey(id))
            {
                this.jobs[id].AttemptRestart();
                if (await this.nodes[this.jobs[id].NodeID].Remove(id) == Result.Success)
                {
                    if (await this.AssignJobBalanced(id, this.jobs[id].NodeID) == Result.Success)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> BalanceAllJobs()
        {
            var nodes = this.nodes.Where(node => node.Value.Reachable).Select(node => node.Key);
            var jobs = new List<int>();

            foreach (var node in nodes)
            {
                int idealJobs = (int)Math.Ceiling((decimal)(this.nodes[node].Schematic.Slots * this.jobs.Count));
                int currentCount = this.GetAssignedJobs(node).Count;
                if (currentCount > idealJobs)
                {
                    var removeJobs = this.GetAssignedJobs(node).GetRange(idealJobs, currentCount - idealJobs).Select(job => job.Blueprint.ID);
                    foreach (int id in removeJobs)
                    {
                        await this.nodes[node].Remove(id);
                        jobs.Add(id);
                    }
                }
            }

            jobs.Concat(this.jobs.Where(job => job.Value.NodeID == -1).Select(job => job.Key));

            await this.AssignJobsBalanced(jobs);
            return true;
        }

        private void BalanceWithMsg()
        {
            this.logger.Log("Balancing distribution...");
            this.BalanceAllJobs().RunSynchronously();
            this.logger.Log("Balance complete!");
        }

        private void DistributionChangeHandler()
        {
            if (!this.ignoreAllEvents)
            {
                this.logger.Log("Beginning load balance");
                Task.Run(this.BalanceAllJobs);
            }
        }

        private List<Job> GetAssignedJobs(int id)
        {
            return this.nodes.ContainsKey(id) ? this.jobs.Values.Where(job => job.NodeID == id).ToList() : null;
        }

        private void LostNodeHandler(int id)
        {
            if (!this.ignoreAllEvents)
            {
                if (this.nodes.ContainsKey(id))
                {
                    this.logger.Log(string.Format("Lost node ID:{0}", id), Severity.Severe);
                    if (this.config.EnableRedundancy)
                    {
                        this.logger.Log(string.Format("Beginning job transfer from node ID:{0}", id));
                        var res = this.Transfer(id).Result;
                        foreach (var job in res)
                        {
                            if (job.Value == null)
                            {
                                this.logger.Log(string.Format("Failed to transfer job ID:{0}", job.Key), Severity.Warn);
                                this.jobs[job.Key].Transfer(-1);
                            }
                            else
                            {
                                this.logger.Log(string.Format("Transferred job ID:{0} to node ID:{1}", job.Key, job.Value));
                            }
                        }

                        this.logger.Log("Job transfer finished");
                    }
                }
            }
        }

        private void RecoveredNodeHandler(int id)
        {
            if (!this.ignoreAllEvents)
            {
                if (this.nodes.ContainsKey(id))
                {
                    this.logger.Log(string.Format("Recovered node ID:{0}", id));
                    Node node;
                    this.nodes.TryRemove(id, out node);
                    this.AddNode(node.Schematic).RunSynchronously();
                    this.DistributionChanged();
                }
            }
        }

        private async Task<Result> ResetAll()
        {
            this.ignoreAllEvents = true;
            var tasks = this.nodes.Where(node => node.Value.Reachable).Select(node => node.Value.Reset());
            await Task.WhenAll(tasks);
            return Result.Success;
        }

        private async Task<Dictionary<int, int?>> Transfer(int nodeID)
        {
            if (this.nodes.ContainsKey(nodeID))
            {
                var res = new Dictionary<int, int?>();
                var assignRes = await this.AssignJobsBalanced(this.GetAssignedJobs(nodeID).Select(job => job.Blueprint.ID).ToList());
                foreach (var job in assignRes)
                {
                    res.Add(job.Key, job.Value == Result.Success ? (int?)this.jobs[job.Key].NodeID : null);
                }

                return res;
            }

            return null;
        }

        private void WorkerExitedHandler(int id)
        {
            if (this.jobs.ContainsKey(id))
            {
                this.logger.Log(string.Format("Worker ID:{0} exited unexpectedly", id), Severity.Warn);
                if (this.config.EnableAutoRestart)
                {
                    if (this.jobs[id].RestartAttempted || !this.AttemptRestart(id).Result)
                    {
                        this.logger.Log(string.Format("Restart of worker ID:{0} failed", id), Severity.Warn);
                        this.DistributionChanged();
                    }
                    else
                    {
                        this.logger.Log(string.Format("Worker ID:{0} restarted successfully", id));
                    }
                }
            }
        }
        #endregion
    }
}
