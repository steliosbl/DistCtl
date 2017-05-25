namespace DistCtlApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using DistCommon;
    using DistCommon.Job;
    using DistCommon.Logging;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Utils;
    using Result = DistCommon.Result;

    [Route("[controller]")]
    public sealed class CtlController : Microsoft.AspNetCore.Mvc.Controller
    {
        #region Fields
        private DistCtl.IController controller;
        private ILogger logger;
        #endregion

        #region Constructors
        public CtlController(DistCtl.IController controller, ILogger logger)
        {
            this.controller = controller;
            this.logger = logger;
        }
        #endregion

        #region Requests

        #region GET
        #region Jobs
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
        #endregion

        #region Nodes
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
        #endregion
        #endregion

        #region POST
        #region Jobs
        [HttpPost("jobs/add")]
        public IActionResult AddJob([FromBody] DistCommon.Job.Blueprint blueprint)
        {
            if (!string.IsNullOrEmpty(blueprint.Command) && blueprint.ID != 0 && blueprint.Priority != 0)
            {
                int rid = this.logger.AddID();
                this.logger.Log(string.Format("Received request [Add Job ID:{0}]", blueprint.ID.ToString()), rid);
                var res = this.controller.Add(blueprint, Source.API).Result;
                this.LogResult(rid, res);
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

            this.LogBadRequest();
            return this.BadRequest();
        }
        #endregion

        #region Nodes
        [HttpPost("nodes/add")]
        public IActionResult AddNode([FromBody] DistCommon.Schema.Node node)
        {
            if (node.ID != 0 && node.Slots != 0 && node.Address != null)
            {
                int rid = this.logger.AddID();
                this.logger.Log(string.Format("Received request [Add Node ID:{0}]", node.ID.ToString()), rid);
                var res = this.controller.Add(node, Source.API).Result;
                this.LogResult(rid, res);
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

            this.LogBadRequest();
            return this.BadRequest();
        }
        #endregion
        #endregion

        #region DELETE
        #region Jobs
        [HttpDelete("jobs/remove")]
        public IActionResult RemoveJob([FromBody] Models.IdObject idobj)
        {
            if (idobj.ID != null)
            {
                int rid = this.logger.AddID();
                this.logger.Log(string.Format("Received request [Remove Job ID:{0}]", idobj.ID.ToString()), rid);
                var res = this.controller.Remove((int)idobj.ID, 0, Source.API).Result;
                this.LogResult(rid, res);
                if (res == Result.Success)
                {
                    return this.Ok();
                }
                else if (res == Result.NotFound)
                {
                    return this.NotFound();
                }
                else
                {
                    return this.Forbid();
                }
            }

            this.LogBadRequest();
            return this.BadRequest();
        }
        #endregion

        #region Nodes
        [HttpDelete("nodes/remove")]
        public IActionResult RemoveNode([FromBody] Models.IdObject idobj)
        {
            if (idobj.ID != null)
            {
                int rid = this.logger.AddID();
                this.logger.Log(string.Format("Received request [Remove Job ID:{0}]", idobj.ID.ToString()), rid);
                var res = this.controller.Remove((int)idobj.ID, Source.API).Result;
                this.LogResult(rid, res);
                if (res == Result.Success)
                {
                    return this.Ok();
                }
                else if (res == Result.NotFound)
                {
                    return this.NotFound();
                }
                else
                {
                    return this.Forbid();
                }
            }

            this.LogBadRequest();
            return this.BadRequest();
        }
        #endregion
        #endregion

        #region PATCH
        #region Jobs
        [HttpPatch("jobs/sleep")]
        public IActionResult SleepJob([FromBody] Models.IdObject idobj)
        {
            if (idobj.ID != null)
            {
                int rid = this.logger.AddID();
                this.logger.Log(string.Format("Received request [Sleep Job ID:{0}]", idobj.ID.ToString()), rid);
                var res = this.controller.Sleep((int)idobj.ID, Source.API).Result;
                this.LogResult(rid, res);
                if (res == Result.Success)
                {
                    return this.Ok();
                }
                else if (res == Result.NotFound)
                {
                    return this.NotFound();
                }
                else
                {
                    return this.Forbid();
                }
            }

            this.LogBadRequest();
            return this.BadRequest();
        }

        [HttpPatch("jobs/wake")]
        public IActionResult WakeJob([FromBody] Models.IdObject idobj)
        {
            if (idobj.ID != null)
            {
                int rid = this.logger.AddID();
                this.logger.Log(string.Format("Received request [Wake Job ID:{0}]", idobj.ID.ToString()), rid);
                var res = this.controller.Wake((int)idobj.ID, Source.API).Result;
                this.LogResult(rid, res);
                if (res == Result.Success)
                {
                    return this.Ok();
                }
                else if (res == Result.NotFound)
                {
                    return this.NotFound();
                }
                else
                {
                    return this.Forbid();
                }
            }

            this.LogBadRequest();
            return this.BadRequest();
        }
        #endregion
        #endregion

        #endregion

        #region Status Codes
        private StatusCodeResult Conflict()
        {
            return this.StatusCode(409);
        }
        #endregion

        #region Utils
        private void LogResult(int id, Result result)
        {
            this.logger.Log(string.Format("Command execution finished with result [{0}]", result.GetString()), id);
        }

        private void LogBadRequest()
        {
            this.logger.Log("Received bad request", Severity.Warn);
        }
        #endregion
    }
}
