namespace Ordering.Application.Exceptions
{
    public class NotFoundException : ApplicationException
    {
        public NotFoundException(string name, object value)
            : base($"Entity \"{name}\" ({value}) was not found.") {}
    }
}