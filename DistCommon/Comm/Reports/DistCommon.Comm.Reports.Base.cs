namespace DistCommon.Comm.Reports
{
    using System;

    public class Base
    {
        public Base()
        {
            this.ReportType = this.GetType();
        }

        [Newtonsoft.Json.JsonConstructor]
        public Base(Type reportType)
        {
            this.ReportType = reportType;
        }

        public Type ReportType { get; private set; }
    }
}
