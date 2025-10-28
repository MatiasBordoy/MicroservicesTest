namespace EmployeeService.Api.Attributes
{
    public class EndpointAttributes
    {
        [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
        public class SkipLoggingAttribute : Attribute
        {
        }
    }

}
