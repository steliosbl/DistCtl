namespace DistCtl
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DistCommon;
    using Results = DistCommon.Constants.Results;

    public sealed class Controller
    {
        #region Fields
        private Config config;
        private ConcurrentDictionary<int, Job> jobs;
        private ConcurrentDictionary<int, Node> nodes;
        private DistCommon.Schema.Controller schematic;
        private Logger logger;
        #endregion

        #region Constructor
        public Controller(Config config, Logger.SayHandler sayHandler)
        {
            this.config = config;
            this.jobs = new ConcurrentDictionary<int, DistCtl.Job>();
            this.nodes = new ConcurrentDictionary<int, DistCtl.Node>();
            this.logger = new Logger(DistCommon.Constants.Ctl.LogFilename, sayHandler);
        }
        #endregion

        #region Events
        private delegate void DistributionChangedHandler();

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
                return this.TotalSlots - this.jobs.Count(job => job.Value.NodeID != 0);
            }
        }
        #endregion

        #region Exposed
        public Task<int> Add(DistCommon.Job.Blueprint job)
        {
            return this.AddJob(new DistCtl.Job(job, -1, false));
        }

        public Task<int> Add(DistCommon.Job.Blueprint job, int nodeID)
        {
            return this.AddJob(new Job(job, nodeID, false));
        }

        public Task<int> Add(DistCommon.Schema.Node schematic)
        {
            return this.AddNode(schematic);
        }

        public Task<int> Assign(int jobID)
        {
            return this.AssignJobBalanced(jobID);
        }

        public Task<int> Assign(int jobID, int nodeID)
        {
            return this.AssignJobManual(jobID, nodeID);
        }

        public bool Initialize()
        {
            return this.StartInit().Result;
        }

        public Task<int> Remove(int nodeID)
        {
            return this.RemoveNode(nodeID);
        }

        public Task<int> Remove(int jobID, int nodeID)
        {
            return this.RemoveJob(jobID);
        }

        public Task<int> Sleep(int jobID)
        {
            return this.SleepJob(jobID);
        }

        public Task<int> Wake(int jobID)
        {
            return this.WakeJob(jobID, false);
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
                    this.logger.Log("Schematic file not found.", 3);
                    Environment.Exit(2);
                }

                if (missingFiles.Contains(this.config.PreLoadFilename))
                {
                    this.logger.Log("Pre-load file not found.", 1);
                    preloadPossible = false;
                }
            }

            this.logger.Log("Loading schematic...");
            {
                this.schematic = JFI.GetObject<DistCommon.Schema.Controller>(this.config.SchematicFilename);
                await this.LoadNodes();

                if (this.nodes.Count == 0)
                {
                    this.logger.Log("All nodes failed to initialize.", 3);
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

            this.logger.Log("Startup completed");

            return true;
        }
        #endregion

        private async Task<int> AddJob(Job job)
        {
            return await this.AddJob(job, -1);
        }

        private async Task<Tuple<int, int>> AddJob(int jobID, Job job)
        {
            return new Tuple<int, int>(jobID, await this.AddJob(job));
        }

        private async Task<int> AddJob(Job job, int nodeID)
        {
            if (!this.jobs.ContainsKey(job.Blueprint.ID))
            {
                this.jobs.TryAdd(job.Blueprint.ID, job);
                int res;
                if (nodeID == -1)
                {
                    res = await this.AssignJobBalanced(job.Blueprint.ID);
                }
                else
                {
                    res = await this.AssignJobManual(job.Blueprint.ID, nodeID);
                }

                if (res != Results.Success)
                {
                    this.jobs.TryRemove(job.Blueprint.ID, out job);
                }

                return res;
            }

            return Results.Invalid;
        }

        private async Task<int> AddNode(DistCommon.Schema.Node schematic)
        {
            if (!this.nodes.ContainsKey(schematic.ID))
            {
                var node = new Node(schematic, this.config.UpdateDelay, this.config.TimeoutDuration, this.LostNodeHandler, this.RecoveredNodeHandler, this.WorkerExitedHandler);
                if (await node.Test())
                {
                    int constructionRes = await node.Construct();
                    if ((constructionRes == Results.Invalid && await node.Reset() == Results.Success && await node.Construct() == Results.Success) || constructionRes == Results.Success)
                    {
                        if (this.nodes.TryAdd(node.Schematic.ID, node))
                        {
                            this.nodes[node.Schematic.ID].StartReportTimer();
                            return Results.Success;
                        }

                        return Results.Fail;
                    }
                }

                return Results.Unreachable;
            }

            return Results.Invalid;
        }

        private async Task<Tuple<int, int>> AddNode(DistCommon.Schema.Node schematic, int id)
        {
            return new Tuple<int, int>(id, await this.AddNode(schematic));
        }

        private async Task<int> AssignJobBalanced(int jobID, params int[] exempt)
        {
            var l = new List<int>();
            l.Add(jobID);
            var res = await this.AssignJobsBalanced(l, exempt);
            return res.Count == 0 ? Results.Success : res[jobID];
        }

        private async Task<int> AssignJobManual(int jobID, int nodeID)
        {
            if (this.nodes.ContainsKey(nodeID))
            {
                if (this.nodes[nodeID].Reachable)
                {
                    int res = await this.nodes[nodeID].Assign(this.jobs[jobID]);
                    
                    if (res == Results.Success)
                    {
                        this.jobs[jobID].Transfer(nodeID);
                    }

                    return res;
                }

                return Results.Fail;
            }

            return Results.NotFound;
        }

        private async Task<int> RemoveJob(int jobID)
        {
            if (this.jobs.ContainsKey(jobID))
            {
                int nodeID = this.jobs[jobID].NodeID;
                int res = await this.nodes[nodeID].Remove(jobID);
                if (res == Results.Success)
                {
                    Job job;
                    this.jobs.TryRemove(jobID, out job);
                }

                return res;
            }

            return Results.NotFound;
        }

        private async Task<int> RemoveNode(int nodeID)
        {
            if (this.nodes.ContainsKey(nodeID))
            {
                int res = await this.nodes[nodeID].Reset();
                if (res == Results.Success)
                {
                    await this.AssignJobsBalanced(this.GetAssignedJobs(nodeID).Select(job => job.NodeID).ToList());
                }

                return res;
            }

            return Results.NotFound;
        }

        private async Task<int> SleepJob(int jobID)
        {
            if (this.jobs.ContainsKey(jobID))
            {
                int nodeID = this.jobs[jobID].NodeID;
                if (this.jobs[jobID].Awake)
                {
                    int res = await this.nodes[nodeID].Sleep(jobID);
                    if (res == Results.Success)
                    {
                        this.jobs[jobID].Sleep();
                    }

                    return res;
                }

                return Results.Invalid;
            }

            return Results.NotFound;
        }

        private async Task<int> WakeJob(int jobID, bool isPreload = false)
        {
            if (this.jobs.ContainsKey(jobID))
            {
                int nodeID = this.jobs[jobID].NodeID;
                if (!this.jobs[jobID].Awake || isPreload)
                {
                    int res = await this.nodes[nodeID].Wake(jobID);
                    if (res == Results.Success)
                    {
                        this.jobs[jobID].Wake();
                    }

                    return res;
                }

                return Results.Invalid;
            }

            return Results.NotFound;
        }

        private async Task<Tuple<int, int>> WakeJob(bool tuple, int jobID)
        {
            return new Tuple<int, int>(jobID, await this.WakeJob(jobID, true));
        }
        #endregion

        #region Internal
        #region Init
        private async Task<bool> LoadJobs()
        {
            var jobs = JFI.GetObject<List<Job>>(this.config.PreLoadFilename);
            //// var assignTasks = jobs.Select(job => this.AddJob(job.Blueprint.ID, job).ContinueWith((t) => this.JobAssignMsg(t.Result)));
            //// var wakeTasks = jobs.Where(job => job.Awake).Select(job => this.WakeJob(true, job.Blueprint.ID).ContinueWith((t) => this.JobWakeMsg(t.Result)));
            var tasks = jobs.Select(job => this.LoadJob(job));
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
            if (job.Awake)
            {
                await this.WakeJob(true, job.Blueprint.ID).ContinueWith((t) => this.JobWakeMsg(t.Result));
            }

            return true;
        }

        private void NodeMsg(Tuple<int, int> res)
        {
            if (res.Item2 == Results.Success)
            {
                this.logger.Log(string.Format("Node ID: {0} initialized successfully", res.Item1));
            }
            else
            {
                this.logger.Log(string.Format("Node ID: {0} failed to initialize", res.Item1), 1);
            }
        }

        private void JobAssignMsg(Tuple<int, int> res)
        {
            if (res.Item2 == Results.Success)
            {
                this.logger.Log(string.Format("Loaded job ID: {0}", res.Item1));
            }
            else
            {
                this.logger.Log(string.Format("Failed to load job ID: {0}", res.Item1), 1);
            }
        }

        private void JobWakeMsg(Tuple<int, int> res)
        {
            if (res.Item2 == Results.Success)
            {
                this.logger.Log(string.Format("Awoke job ID: {0}", res.Item1));
            }
            else
            {
                this.logger.Log(string.Format("Failed to wake job ID: {0}", res.Item1), 1);
            }
        }
        #endregion

        private async Task<Dictionary<int, int>> AssignJobsBalanced(List<int> jobIDs, params int[] exempt)
        {
            var res = new Dictionary<int, int>();
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
                            int assignRes = await this.AssignJobManual(jobID, min.Key);
                            if (assignRes == Results.Success)
                            {
                                nodes[min.Key] = (float)this.GetAssignedJobs(min.Key).Count / this.nodes[min.Key].Schematic.Slots;
                                jobIDs.RemoveAt(0);
                                slotCount -= 1;
                            }

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

            return jobIDs.ToDictionary(job => job, job => Results.Fail);
        }

        private async Task<bool> AttemptRestart(int id)
        {
            if (this.jobs.ContainsKey(id))
            {
                this.jobs[id].AttemptRestart();
                if (await this.nodes[this.jobs[id].NodeID].Remove(id) == Results.Success)
                {
                    if (await this.AssignJobBalanced(id, this.jobs[id].NodeID) == Results.Success)
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
            this.logger.Log("Beginning load balance");
            Task.Run(this.BalanceAllJobs);
        }

        private List<Job> GetAssignedJobs(int id)
        {
            return this.nodes.ContainsKey(id) ? this.jobs.Values.Where(job => job.NodeID == id).ToList() : null;
        }

        private void LostNodeHandler(int id)
        {
            if (this.nodes.ContainsKey(id))
            {
                this.logger.Log(string.Format("Lost node ID:{0}", id), 2);
                if (this.config.EnableRedundancy)
                {
                    this.logger.Log(string.Format("Beginning job transfer from node ID:{0}", id));
                    var res = this.Transfer(id).Result;
                    foreach (var job in res)
                    {
                        if (job.Value == null)
                        {
                            this.logger.Log(string.Format("Failed to transfer job ID:{0}", job.Key), 1);
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

        private void RecoveredNodeHandler(int id)
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

        private async Task<Dictionary<int, int?>> Transfer(int nodeID)
        {
            if (this.nodes.ContainsKey(nodeID))
            {
                var res = new Dictionary<int, int?>();
                var assignRes = await this.AssignJobsBalanced(this.GetAssignedJobs(nodeID).Select(job => job.Blueprint.ID).ToList());
                foreach (var job in assignRes)
                {
                    res.Add(job.Key, job.Value == Results.Success ? (int?)this.jobs[job.Key].NodeID : null);
                }

                return res;
            }

            return null;
        }

        private void WorkerExitedHandler(int id)
        {
            if (this.jobs.ContainsKey(id))
            {
                this.logger.Log(string.Format("Worker ID:{0} exited unexpectedly", id), 1);
                if (this.config.EnableAutoRestart)
                {
                    if (this.jobs[id].RestartAttempted || !this.AttemptRestart(id).Result)
                    {
                        this.logger.Log(string.Format("Restart of worker ID:{0} failed", id), 1);
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
