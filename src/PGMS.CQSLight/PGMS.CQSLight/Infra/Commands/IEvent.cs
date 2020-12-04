using Newtonsoft.Json;
using PGMS.CQSLight.Extensions;
using System;

namespace PGMS.CQSLight.Infra.Commands
{
    public interface IEvent : IMessage
    {
        Guid AggregateId { get; set; }
        Guid Id { get; }
        string ByUser { get; set; }
    }

    public interface IDomainEvent : IEvent
    {
        long Timestamp { get; set; }

        string GetJSonSerialisation();
    }

    public abstract class DomainEvent<T> : Event, IEquatable<DomainEvent<T>>, IDomainEvent
    {
        public T Parameters { get; set; }

        protected DomainEvent(T parameters) => Parameters = parameters;

        public string GetJSonSerialisation()
        {
            return JsonConvert.SerializeObject(Parameters);
        }

        public bool Equals(DomainEvent<T> other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(Parameters, other.Parameters);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DomainEvent<T>)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Parameters != null ? Parameters.GetHashCode() : 0);
            }
        }
    }

    [Serializable]
    public abstract class Event : IEvent, IEquatable<Event>
    {
        protected Event()
        {
            Id = Guid.NewGuid();
            Timestamp = DateTime.Now.ToEpoch();
        }

        public Guid Id { get; private set; }
        public string ByUser { get; set; }
        public long Timestamp { get; set; }
        public Guid AggregateId { get; set; }

        public bool Equals(Event other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.Id.Equals(Id);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Event ? Equals((Event)obj) : false;
        }

        public override int GetHashCode() => Id.GetHashCode();
    }
}