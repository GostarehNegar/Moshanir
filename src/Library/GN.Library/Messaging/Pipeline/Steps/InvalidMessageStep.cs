using GN.Library.Messaging.Internals;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace GN.Library.Messaging.Pipeline
{
    class InvalidMessageStep : IPipelineStep
    {
        private readonly MessageBus bus;
        private IMemoryCache chache;
        public InvalidMessageStep(MessageBus bus)
        {
            this.chache = bus.Advanced().ServiceProvider.GetServiceEx<IMemoryCache>();
            this.bus = bus;
        }
        public Task Handle(IPipelineContext ctx, Func<IPipelineContext, Task> next)
        {
            var context = ctx;
            if (context?.MessageContext == null)
                throw new Exception("Invalid or null context");
            var message = context.MessageContext;
            if (message == null)
                return Task.CompletedTask;
            if (!message.Options().ByPassDuplicateValidation)
            {
                if (message.Message.From() == this.bus.EndpointName && !message.Headers.IsReplayingForMe(this.bus.EndpointName))
                    return Task.CompletedTask;
                var tag = $"seen_message{message.Message.MessageId}";
                if (this.chache.TryGetValue(tag, out var _m))
                {
                    long? _exixting = _m != null && typeof(long?).IsAssignableFrom(_m.GetType())
                        ? (long?)_m
                        : (long?)null;
                    if (_exixting == message.Message.Version)
                    {
                        return Task.CompletedTask;

                    }
                }
                this.chache.Set(tag, message.Message.Version, TimeSpan.FromDays(1));
                if (message.HasBeenPublishedBy(this.bus.EndpointName) && !message.Headers.IsReplayingForMe(this.bus.EndpointName))
                    return Task.CompletedTask;
            }
            if (string.IsNullOrWhiteSpace(message.Message.From()))
            {
                message.Message.From(this.bus.EndpointName);
            }

            return next(context);
        }
    }
}
