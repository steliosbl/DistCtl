namespace DistCommon.Comm.Responses
{
    using System;

    public class Base
    {
        public Base()
        {
            this.ResponseType = this.GetType();
        }

        public Base(Result result)
        {
            this.ResponseType = this.GetType();
            this.Result = result;
        }

        [Newtonsoft.Json.JsonConstructor]
        public Base(Type responseType, Result result)
        {
            this.ResponseType = responseType;
            this.Result = result;
        }

        public Type ResponseType { get; private set; }

        public Result Result { get; private set; }
    }
}
