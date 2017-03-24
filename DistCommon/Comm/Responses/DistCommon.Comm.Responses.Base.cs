namespace DistCommon.Comm.Responses
{
    using System;

    public class Base
    {
        public Base()
        {
            this.ResponseType = this.GetType();
        }

        public Base(int responseCode)
        {
            this.ResponseType = this.GetType();
            this.ResponseCode = responseCode;
        }

        [Newtonsoft.Json.JsonConstructor]
        public Base(Type responseType, int responseCode)
        {
            this.ResponseType = responseType;
            this.ResponseCode = responseCode;
        }

        public Type ResponseType { get; private set; }

        public int ResponseCode { get; private set; }
    }
}
