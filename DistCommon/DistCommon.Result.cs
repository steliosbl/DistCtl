namespace DistCommon
{
    public enum Result
    {
        Fail,
        Success,
        NotFound,
        Invalid,
        NotConstructed,
        Unreachable
    }

    public static class ResultInfo
    {
        public static string GetString(this Result result)
        {
            switch (result)
            {
                case Result.Fail:
                    return "Fail";
                case Result.Success:
                    return "Success";
                case Result.NotFound:
                    return "Not Found";
                case Result.Invalid:
                    return "Invalid";
                case Result.NotConstructed:
                    return "Not Constructed";
                case Result.Unreachable:
                    return "Unreachable";
                default:
                    return "UNKNOWN";
            }
        }
    }
}
