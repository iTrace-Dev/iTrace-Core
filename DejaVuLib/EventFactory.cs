namespace DejaVuLib
{
    public interface IEventFactory
    {
        ComputerEvent Create(string serialized);
    }

    public class EventFactory<T> : IEventFactory 
        where T: ComputerEvent, new()
    {
        protected IPauseStrategy strategy;

        public EventFactory(IPauseStrategy strategy)
        {
            this.strategy = strategy;
        }

        public ComputerEvent Create(string serialized)
        {
            ComputerEvent result = new T();

            result.DeserializeFrom(serialized);
            result.PauseStrategy = strategy;

            return result;
        }
    }
}
