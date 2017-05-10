namespace DistCtlApi
{
    using System;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;

    internal sealed class RequiredFromQueryActionConstraint : IActionConstraint
    {
        private readonly string parameter;

        public RequiredFromQueryActionConstraint(string parameter)
        {
            this.parameter = parameter;
        }

        public int Order => 999;

        public bool Accept(ActionConstraintContext context)
        {
            if (!context.RouteContext.HttpContext.Request.Query.ContainsKey(this.parameter))
            {
                return false;
            }

            return true;
        }
    }
}
