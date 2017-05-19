namespace DistCtlApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using DistCommon.Job;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Utils;
    using Result = DistCommon.Result;

    [Route("[controller]")]
    public sealed class CtlController : Microsoft.AspNetCore.Mvc.Controller
    {
        private DistCtl.IController controller;

        public CtlController(DistCtl.IController controller)
        {
            this.controller = controller;
        }

        [HttpGet("jobs")]
        public IActionResult GetAllJobs()
        {
            return new ObjectResult(JsonConvert.SerializeObject(this.controller.GetJob()));
        }

        [HttpGet("jobs")]
        public IActionResult GetJobById([RequiredFromQuery] string id)
        {
            int jobID;
            if (int.TryParse(id, out jobID))
            {
                var info = this.controller.GetJob(jobID);
                if (info != null)
                {
                    return new ObjectResult(JsonConvert.SerializeObject(info));
                }

                return this.NotFound();
            }

            return this.BadRequest();
        }

        [HttpPost("jobs/add")]
        public IActionResult AddJob([FromBody] DistCommon.Job.Blueprint blueprint)
        {
            if (!string.IsNullOrEmpty(blueprint.Command) && blueprint.ID != 0 && blueprint.Priority != 0)
            {
                var res = this.controller.Add(blueprint).Result;
                if (res == Result.Success)
                {
                    return this.Ok();
                }
                else if (res == Result.Invalid)
                {
                    return this.Conflict();
                }
                else
                {
                    return this.Forbid();
                }
            }

            return this.BadRequest();
        }

        [HttpPost("nodes/add")]
        public IActionResult AddNode([FromBody] DistCommon.Schema.Node node)
        {
            if (node.ID != 0 && node.Slots != 0 && node.Address != null)
            {
                var res = this.controller.Add(node).Result;
                if (res == Result.Success)
                {
                    return this.Ok();
                }
                else if (res == Result.Invalid)
                {
                    return this.Conflict();
                }
                else
                {
                    return this.Forbid();
                }
            }

            return this.BadRequest();
        }

        [HttpGet("nodes")]
        public IActionResult GetAllNodes()
        {
            return new ObjectResult(JsonConvert.SerializeObject(this.controller.GetNode()));
        }

        [HttpGet("nodes")]
        public IActionResult GetNodeById([RequiredFromQuery] string id)
        {
            int nodeID;
            if (int.TryParse(id, out nodeID))
            {
                var info = this.controller.GetNode(nodeID);
                if (info != null)
                {
                    return new ObjectResult(JsonConvert.SerializeObject(info));
                }

                return this.NotFound();
            }

            return this.BadRequest();
        }

        private StatusCodeResult Conflict()
        {
            return this.StatusCode(409);
        }
    }
}
