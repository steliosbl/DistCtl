namespace DistCtlApi.Models
{
    public sealed class IdObject
    {
        public IdObject(int? id)
        {
            this.ID = id;
        }

        public int? ID { get; private set; }
    }
}
