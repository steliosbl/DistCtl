namespace DistCommon.Comm.Responses
{
    using System.Collections.Generic;

    public sealed class Report : Base
    {
        public Report(List<Reports.Base> reports) : base(DistCommon.Constants.Results.Success)
        {
            this.Reports = reports;
        }

        public List<Reports.Base> Reports { get; private set; }
    }
}