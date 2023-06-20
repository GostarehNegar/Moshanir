using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GN.Library.Shared.Authorization;
using GN.Library.Authorization;
using GN.Library.Messaging;
using Microsoft.Extensions.Logging;

namespace GN.Library.Identity
{
    public class AuthenticateCommandHandler : IMessageHandler<AuthenticateCommand>
    {
        private readonly IAuthorizationService authorizationService;
        private readonly ILogger<AuthenticateCommandHandler> logger;

        public AuthenticateCommandHandler(IAuthorizationService authorizationService, ILogger<AuthenticateCommandHandler> logger)
        {
            this.authorizationService = authorizationService;
            this.logger = logger;
        }
        public async Task Handle(IMessageContext<AuthenticateCommand> ctx)
        {
            try
            {
                var result = this.authorizationService.Login(ctx.Message.Body.UserName, ctx.Message.Body.Password);
                await ctx.Reply(new AuthenticateResponse { Token = result.Token , UserName = result.UserName , DisplayName = result.DisplayName });
            }
            catch (Exception err)
            {
                this.logger.LogError(
                    $"An error occured while trying to handle 'AuthenticateCommand'. Err:{err.GetBaseException().Message}");
                await ctx.Reply(err);
            }
        }
    }
}
