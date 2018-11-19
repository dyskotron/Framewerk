namespace Framewerk.Core
{
    public interface IInjection
    {
        object Value { get; set; }
        object Source{ get;}
        InjectionType Type{ get; }
    }

    public enum InjectionType
    {
        Unique,
        Singleton,
        Value
    }

    public class Injection : IInjection
    {
        public object Value { get; set; }

        public object Source{ get; private set; }
        public InjectionType Type{ get; private set; }

        public Injection(object source, InjectionType type)
        {
            Source = source;
            Type = type;
        }
    }
}