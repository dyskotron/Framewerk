namespace Framewerk.Core
{
    public interface IInjection
    {
        object Value { get; set; }
        object Source{ get;}
    }

    public enum InjectionType
    {
        Default,
        Singleton,
        Value
    }

    public class Injection : IInjection
    {
        public object Value { get; set; }

        public object Source{ get; private set; }

        public Injection(object source, InjectionType type)
        {
            Source = source;
        }
    }
}