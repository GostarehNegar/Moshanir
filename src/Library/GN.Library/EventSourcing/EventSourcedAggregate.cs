using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GN.Library.EventSourcing
{

	public abstract class EventSourcedAggregate
	{
		private List<object> _domainEvents = new List<object>();
		public Guid Id { get; protected set; }
		protected abstract void DoApply(object @event);
		protected virtual object[] DoDecide(object command)
		{
			return new object[] { };
		}
		public void Apply(object @event, bool isNew = true)
		{
			this.DoApply(@event);
			this._domainEvents = this._domainEvents ?? new List<object>();
			if (isNew)
				this._domainEvents.Add(@event);
		}
		public object[] Decide(object command, bool apply = true)
		{
			var events = this.DoDecide(command);
			if (events != null)
			{
				events.ToList().ForEach(x => this.Apply(x));
			}
			return events;
		}
		public object[] GetEvents(bool clear = false)
		{
			this._domainEvents = this._domainEvents ?? new List<object>();
			var result = this._domainEvents.ToArray();
			if (clear)
				this._domainEvents.Clear();
			return result;
		}

	}
}
