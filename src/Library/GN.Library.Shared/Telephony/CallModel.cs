using System;
using System.Collections.Generic;

namespace GN.Library.Shared.Telephony
{
    public enum CallStates
    {
        Ringing,
        Ring,
        Up,
        Down,
        HangUp,
        UnKnown
    }
    public enum CallTypes
    {
        Unknown,
        InternalOriginalCall,
        InternalSecondaryCall,
        IncomingCall,
        OutgoingCall,
        OriginalIncomingCall,
        Transferred,
        Transferee,
        Queue

    }
    public enum QueueStatus
    {
        QueueUnknown,
        QueueRinging,
        QueueAsnsweredByMe,
        QueueAnswerd,
        QueueMissCall
    }
    public enum KnownEvents
    {
        Unknown,
        Transferred,
        StateChanged
    }
    public class CallModel
    {
        public KnownEvents LastEvent { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public CallTypes CallType
        {
            get
            {
                if (this.Transfered)
                    return this.ID == this.OrigTransfererUniqueId ? CallTypes.Transferred : CallTypes.Transferee;

                //else if (this.IsQueue)
                //    return CallTypes.Queue;

                else if (this.ChannelStreamName == "SIP/RAW/Newrock")
                    return CallTypes.OriginalIncomingCall;

                else if (this.IsInternal())
                    return this.ID == this.LinkedId ? CallTypes.InternalOriginalCall : CallTypes.InternalSecondaryCall;

                else if (this.IsOutgoing())
                    return CallTypes.OutgoingCall;

                else if (this.IsIncomming())
                    return CallTypes.IncomingCall;

                return CallTypes.Unknown;
            }
        }
        public string FromlineNumber
        {
            get
            {
                switch (this.CallType)
                {
                    case CallTypes.InternalOriginalCall:
                        return this.CallerIdNum;
                    case CallTypes.InternalSecondaryCall:
                        return this.ConnectedLineNum;
                    case CallTypes.IncomingCall:
                        return this.ConnectedLineNum;
                    case CallTypes.OutgoingCall:
                        return this.CallerIdNum;
                    case CallTypes.Unknown:
                        return this.CallerIdNum;
                    case CallTypes.Transferred:
                        return this.ConnectedLineNum;
                    case CallTypes.Transferee:
                        return this.TransferredLineNumber;
                    default:
                        return "";
                }
            }
        }
        public string TolineNumber
        {
            get
            {
                switch (this.CallType)
                {
                    case CallTypes.InternalOriginalCall:
                        return this.ConnectedLineNumberIsUnknown() ? this.Exten : this.ConnectedLineNum;
                    case CallTypes.InternalSecondaryCall:
                        return this.CallerIdNum;
                    case CallTypes.IncomingCall:
                        return this.CallerIdNum;
                    case CallTypes.OutgoingCall:
                        return this.ConnectedLineNum == "<unknown>" ? this.Exten : this.ConnectedLineNum;
                    case CallTypes.Unknown:
                        return this.ConnectedLineNum;
                    case CallTypes.Transferred:
                        return this.CallerIdNum;
                    case CallTypes.Transferee:
                        return this.CallerIdNum;
                    default:
                        return "";
                }
            }

        }
        public string CallerIdNum { get; set; }
        public string Exten => this.Attributes.TryGetValue("exten", out var tmp) ? tmp : null;
        public string State { get; set; }
        public string Direction { get; set; }
        public string ConnectedLineNum { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public string TransferredFromLineNumber { get; set; }
        public string TransferredToLineNumber { get; set; }
        public string TransferredLineNumber { get; set; }
        public string ChannelName { get; set; }
        public bool Transfered { get; set; }
        public string OrigTransfererUniqueId { get; set; }
        public string ID { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ChannelStreamName => !string.IsNullOrWhiteSpace(this.ChannelName) ? this.ChannelName.Split('-')[0].Replace("SIP", "SIP/RAW") : "";
        public string Context { get; set; }
        public double Duration { get; set; }
        public long UpTime { get; set; }
        public long HangUpTime { get; set; }
        public long RingingTime { get; set; }
        public long RingTime { get; set; }
        public string TrasferedCallUniqueId { get; set; }
        public string TransferedLineNum { get; set; }
        public long TimeStamp { get; set; }
        public string LinkedId { get; set; }
        public override string ToString()
        {
            return $"extention {this.GetExtension()} changed state to {this.State} by {this.GetExternalPhoneNumber()} on {this.TimeStamp}";
        }
        public CallStates GetCallState()
        {
            switch (this.State?.ToLower())
            {
                case "ringing":
                    return CallStates.Ringing;
                case "ring":
                    return CallStates.Ring;
                case "up":
                    return CallStates.Up;
                case "down":
                    return CallStates.Down;
                case "hangup":
                    return CallStates.HangUp;
                default:
                    return CallStates.UnKnown;
            }
        }
        public bool IsOutgoing() => this.Direction?.ToLowerInvariant() == "out";
        public bool IsIncomming() => this.Direction?.ToLowerInvariant() == "in";
        public bool IsInternal() => this.Direction?.ToLowerInvariant() == "inter";//&& this.Context == "from-internal";
        public string GetExtension()
        {
            if (this.CallType == CallTypes.InternalOriginalCall || this.CallType == CallTypes.InternalSecondaryCall)
                return this.FromlineNumber;
            return this.IsExtension(this.FromlineNumber) ? this.FromlineNumber : this.TolineNumber;

        }
        public string GetExternalPhoneNumber()
        {
            if (this.CallType == CallTypes.InternalOriginalCall || this.CallType == CallTypes.InternalSecondaryCall)
                return this.TolineNumber;
            return this.IsExtension(this.FromlineNumber) ? this.TolineNumber : this.FromlineNumber;

        }
        public bool IsExtension(string num) => num != null && num.Length == 3;
        public string GetStringedDuration()
        {
            TimeSpan t = TimeSpan.FromSeconds(this.Duration);
            return string.Format("{0:D2}:{1:D2}:{2:D2}:{3:D3}",
                            t.Hours,
                            t.Minutes,
                            t.Seconds,
                            t.Milliseconds);
        }
        public bool ConnectedLineNumberIsUnknown()
        {
            return this.ConnectedLineNum == "<unknown>";
        }
    }
}
