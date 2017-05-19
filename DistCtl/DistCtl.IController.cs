namespace DistCtl
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DistCommon;
    using DistCommon.Logging;

    public interface IController
    {
        Task<Result> Add(DistCommon.Job.Blueprint job, Source src);

        Task<Result> Add(DistCommon.Schema.Node schematic, Source src);

        Task<Result> Assign(int jobID, Source src);

        Task<Result> Assign(int jobID, int nodeID, Source src);

        JobInfo GetJob(int jobID);

        Dictionary<int, JobInfo> GetJob();

        NodeInfo GetNode(int nodeID);

        Dictionary<int, NodeInfo> GetNode();

        Task<Result> Remove(int nodeID, Source src);

        Task<Result> Remove(int jobID, int nodeID, Source src);

        Task<Result> Sleep(int jobID, Source src);

        Task<Result> Wake(int jobID, Source src);
    }
}
