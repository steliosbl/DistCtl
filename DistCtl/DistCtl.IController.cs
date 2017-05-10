namespace DistCtl
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DistCommon;

    public interface IController
    {
        Task<Result> Add(DistCommon.Job.Blueprint job);

        Task<Result> Add(DistCommon.Job.Blueprint job, int nodeID);

        Task<Result> Add(DistCommon.Schema.Node schematic);

        Task<Result> Assign(int jobID);

        Task<Result> Assign(int jobID, int nodeID);

        JobInfo GetJob(int jobID);

        Dictionary<int, JobInfo> GetJob();

        NodeInfo GetNode(int nodeID);

        Dictionary<int, NodeInfo> GetNode();

        Task<Result> Remove(int nodeID);

        Task<Result> Remove(int jobID, int nodeID);

        Task<Result> Sleep(int jobID);

        Task<Result> Wake(int jobID);
    }
}
