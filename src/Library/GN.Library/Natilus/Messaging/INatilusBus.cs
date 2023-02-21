using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Natilus.Messaging
{
    public interface INatilusBus
    {
        INatilusMessageContext CreateNatilusMessage(string subject, object message);
        INatilusSubscriptionBuilder CreateNatilusSubscription(string subject);
    }
    public interface INatilusBusEx : INatilusBus
    {

    }
}
