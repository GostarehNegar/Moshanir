using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Chats
{
	public class SendEventsModel
	{
		public ChatChannelVersionableEvent[] Events { get; set; }
		public string Mode { get; set; }
		public long Position { get; set; }
		public long RemainingCount { get; set; }

		public static SendEventsModel FromString (string _message)
        {
            var result = new SendEventsModel
            {
                Events = new ChatChannelVersionableEvent[] { }

            };
            if (_message != null)
            {
                if (_message.IndexOf("events") > -1 && _message.IndexOf("events") < 10)
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<SendEventsModel>(_message.ToString(), new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }

                    });
                }
                else
                {
                    var _model = Newtonsoft.Json.JsonConvert.DeserializeObject<ChatChannelVersionableEvent>(_message.ToString(), new Newtonsoft.Json.JsonSerializerSettings
                    {
                        ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() }

                    });
                    result.Events = new ChatChannelVersionableEvent[] { _model };
                }
            }
            return result;
        }
	}
}
