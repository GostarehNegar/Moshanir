using GN.Library.Contracts_Deprecated;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using GN.Library.Messaging.Messages;

namespace GN.Library.Messaging.Internals
{
    public class RequestHandler
    {
        public string EndpointName { get; set; }
    }
    public class RequestOptions
    {
        public int LoadBalanceWait { get; set; }
    }
    public interface IRequest
    {
        Task<IMessageContext> WaitFor(Func<IMessageContext, bool> validator = null, CancellationToken cancellationToken = default);
        //Task<IEnumerable<RequestHandler>> QueryHandlers(Func<RequestHandler, bool> validator, bool localOnly = false, CancellationToken cancellationToken = default);
        //IEnumerable<RequestHandler> Handlers { get; }
    }
    class Request : IRequest
    {
        private TaskCompletionSource<IMessageContext> source;
        private Func<IMessageContext, bool> validator;
        private IMessageContext command;
        private Func<CancellationToken, Task> send;
        private IMessageBusEx bus;
        private readonly RequestOptions options;
        private List<RequestHandler> handlers;
        private bool sent;
        IEnumerable<RequestHandler> Handlers => this.handlers ?? new List<RequestHandler>();
        private List<IMessageContext<AcquireMessageRequest>> acquire_requests = new List<IMessageContext<AcquireMessageRequest>>();
        private List<AcquireMessageReply> acquire_replies = new List<AcquireMessageReply>();

        public Request(Func<CancellationToken, Task> send, IMessageContext command, IMessageBusEx bus, RequestOptions options)
        {
            this.send = send;
            source = new TaskCompletionSource<IMessageContext>();
            this.command = command;
            this.bus = bus;
            this.options = options ?? new RequestOptions();
            this.handlers = new List<RequestHandler>();
        }
        public async Task Acquire(IMessageContext context)
        {
            this.acquire_requests = this.acquire_requests ?? new List<IMessageContext<AcquireMessageRequest>>();
            this.acquire_replies = this.acquire_replies ?? new List<AcquireMessageReply>();
            if (context.TryCast<AcquireMessageRequest>(out var request))
            {
                var acquired = false;
                this.acquire_requests.Add(request);
                int wait = this.options.LoadBalanceWait > 0
                    ? this.options.LoadBalanceWait
                    : (request.Message.Body.RacingWait > 0
                        ? request.Message.Body.RacingWait
                        : 1);
                await Task.Delay(wait);
                var task_list = new List<Task>();
                lock (this.acquire_replies)
                {
                    acquired = this.acquire_replies.Any(x => x.Acquired);
                    if (!acquired)
                    {
                        var candidates = this.acquire_requests.OrderBy(x => x.Message.Body.Load).ToList();
                        for (int i = 0; i < candidates.Count; i++)
                        {
                            var reply = new AcquireMessageReply
                            {
                                Acquired = i == 0
                            };
                            this.acquire_replies.Add(reply);
                            acquired = true;
                            task_list.Add(candidates[i].Reply(reply));
                            this.acquire_requests.Remove(candidates[i]);
                        }
                    }
                    this.acquire_requests.ToArray().ToList().ForEach(ctx =>
                    {
                        var reply = new AcquireMessageReply { Acquired = !acquired };
                        this.acquire_replies.Add(reply);
                        acquired = this.acquire_replies.Any(x => x.Acquired);
                        task_list.Add(context.Reply(reply));
                        this.acquire_requests.Remove(ctx);
                    });
                    //this.acquire_requests.Clear();
                }
                await Task.WhenAll(task_list.ToArray());
            }
            else
            {
                await context.Reply(new Exception("Invalid Request"));
            }

        }
        public bool SetReply(IMessageContext context)
        {
            if (this.validator == null || this.validator(context))
                return source.TrySetResult(context);
            return false;
        }
        public void SetCanceld()
        {
            this.bus.CancelRequest(this.command);
            if (source.Task.Status == TaskStatus.Running)
                source.SetCanceled();
            source.TrySetCanceled();
        }
        public async Task<IMessageContext> WaitFor(Func<IMessageContext, bool> validator = null, CancellationToken cancellationToken = default)
        {

            cancellationToken.Register(() => { this.SetCanceld(); });
            this.validator = validator;
            if (!sent)
            {
                sent = true;
                await this.send(cancellationToken).ConfigureAwait(false);
            }
            return await source.Task;
        }

        public Task<IEnumerable<QueryHandlerReply>> QueryHandlers(bool internalOnly = false)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<QueryHandlerReply>> QueryHandlers(int miliSecondTimeOut = 30000, bool internalOnly = false)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<RequestHandler>> QueryHandlers(Func<RequestHandler, bool> validator, bool localOnly = false, CancellationToken cancellationToken = default)
        {
            this.handlers = new List<RequestHandler>();
            try
            {
                await bus.CreateMessage(new QueryHandler
                {
                    TopicName = this.command.Message.Subject,
                    Stream = this.command.Message.Stream,
                    //StreamId = this.command.Message.Topic.StreamId
                })
                    .UseTopic(typeof(QueryHandler))
                    .Options(x => x.LocalOnly = localOnly)
                    .CreateRequest()
                    .WaitFor(h =>
                    {
                        var reply = h.Message.Cast<QueryHandlerReply>();
                        if (reply != null && reply.Body.IsReady)
                        {
                            var handler = new RequestHandler
                            {
                                EndpointName = reply.Body.EndpointName
                            };
                            this.handlers.Add(handler);
                            return validator(handler);
                        }
                        return false;
                    });
            }
            catch (Exception err)
            {

            }

            return this.handlers;
        }
    }

}
