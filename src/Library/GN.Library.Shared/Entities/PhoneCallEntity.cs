using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GN.Library.Shared.Chats;

namespace GN.Library.Shared.Entities
{
    public class PhoneCallEntity : XrmDynamicEntity
    {
        public new class Schema
        {
            public const string SolutionPerfix = "gndync_";
            public const int EnumPrefix = 630750000;

            public const string CallDirection = SolutionPerfix + "calldirection";
            public const string Duration = SolutionPerfix + "duration";
            public const string Transfered = SolutionPerfix + "transfered";
            public const string TransferedLineNumber = SolutionPerfix + "transferedlinenumber";
            public const string CallState = SolutionPerfix + "callstate";
            public const string CallType = SolutionPerfix + "calltype";

            public const string LogicalName = "phonecall";
            public const string From = "from";
            public const string To = "to";
            public const string Description = "description";
            public const string Subject = "subject";
            public const string Start = "actualstart";
            public const string State = "statecode";
            public const string Priority = "prioritycode";
            public const string Status = "statuscode";
            public const string Number = "phonenumber";
            public const string CreatedOn = "createdon";
            public const string OwnerId = "ownerid";
            public const string NotificationSubject = SolutionPerfix + "notificationsubject";
            public const string PhoneNumber = "phonenumber";
            public const string DirectionCode = "directioncode";
            public enum StateCodes
            {
                Open = 0,
                Completed = 1,
                Canceled = 2

            }
            public enum StatusCodes
            {
                Open = 1,
                Made = 2,
                Canceled = 3,
                Recieved = 4,
            }
            public enum DirectionCodes
            {
                Incomming = EnumPrefix,
                Outgoing = EnumPrefix + 1,
                Internal = EnumPrefix + 2,
            }
            public enum CallStates
            {
                Ring = EnumPrefix,
                Ringing = EnumPrefix + 1,
                Up = EnumPrefix + 2,
                HangUp = EnumPrefix + 3,
                Unknown = EnumPrefix + 4,
                Transfer = EnumPrefix + 5,
                Transferred = EnumPrefix + 6,
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
                Transferee

            }


        }
        
        public PhoneCallEntity()
        {
            LogicalName = "phonecall";
        }
        public int State { get => GetAttributeValue<int>(Schema.State); }
        public int CallState { get => GetAttributeValue<int>(Schema.CallState); }
        public string CallType { get => GetAttributeValue(Schema.CallType); }
        public bool Transfered { get => GetAttributeValue<bool>(Schema.Transfered); }
        public string TransferedLineNumber { get => GetAttributeValue(Schema.TransferedLineNumber); }
        public string Duration { get => GetAttributeValue(Schema.Duration); }
        public string NotificationSubject { get => GetAttributeValue(Schema.NotificationSubject); set => SetAttributeValue(Schema.NotificationSubject, value); }

        public int CallDirection { get => GetAttributeValue<int>(Schema.CallDirection); set => SetAttributeValue(Schema.CallDirection, value); }
        public int Status { get => GetAttributeValue<int>(Schema.Status); }
        public string Subject { get => GetAttributeValue(Schema.Subject); set => SetAttributeValue(Schema.Subject, value); }

        public string Start { get => GetAttributeValue(Schema.Start); }
        public int Priority { get => GetAttributeValue<int>(Schema.Priority); }
        public string Number { get => GetAttributeValue(Schema.Number); }
        public ChatUserEntity Owner_Deprecated { get => GetAttributeValue<ChatUserEntity>(Schema.OwnerId); set => SetAttributeValue(Schema.OwnerId, value); }
        public DynamicEntityReference Owner { get => GetAttributeValue<DynamicEntityReference>(Schema.OwnerId); set => SetAttributeValue(Schema.OwnerId, value); }

        public string PhoneNumber
        {
            get { return this.GetAttributeValue<string>(Schema.PhoneNumber); }
            set { this.SetAttributeValue(Schema.PhoneNumber, value); }
        }
        public bool DirectionCode
        {
            get => this.GetAttributeValue<bool>(Schema.DirectionCode);
            set => this.SetAttributeValue(Schema.DirectionCode, value);
        }
        public bool IsOutgoing => this.DirectionCode;
        public DateTime CreatedOn { get => GetAttributeValue<DateTime>(Schema.CreatedOn); }
        public DynamicEntity[] From_Deprecated => GetAttributeValue<DynamicEntity[]>(Schema.From);
        public DynamicEntityReference[] From => GetAttributeValue<DynamicEntityReference[]>(Schema.From);
        public DynamicEntityReference[] To => GetAttributeValue<DynamicEntityReference[]>(Schema.To) ?? new DynamicEntityReference[] { };
        public DynamicEntity[] To_Deprecated => GetAttributeValue<DynamicEntity[]>(Schema.To);
        public int GetInternalCallDirectionCode()
        {
            var ownerId = Owner_Deprecated;
            foreach (var fr in From_Deprecated)
            {
                if (fr.Id == ownerId.Id)
                {
                    return (int)Schema.DirectionCodes.Outgoing;
                }
            }
            foreach (var t in To_Deprecated)
            {
                if (t.Id == ownerId.Id)
                {
                    return (int)Schema.DirectionCodes.Incomming;
                }
            }
            return -1;
        }
        public PhoneCallEntity AddTo(params DynamicEntityReference[] entity)
        {
            var tos = new List<DynamicEntityReference>(To ?? Array.Empty<DynamicEntityReference>());
            tos.AddRange(entity);
            SetAttributeValue(Schema.To, tos.ToArray());
            return this;
        }
        
        public PhoneCallEntity AddTo_Deprecated(params DynamicEntity[] entity)
        {
            var tos = new List<DynamicEntity>(To_Deprecated ?? Array.Empty<DynamicEntity>());
            tos.AddRange(entity);
            SetAttributeValue(Schema.To, tos.ToArray());
            return this;
        }
        public PhoneCallEntity AddFrom(params DynamicEntityReference[] entity)
        {
            var froms = new List<DynamicEntityReference>(From ?? Array.Empty<DynamicEntityReference>());
            froms.AddRange(entity.Select(x=> x));
            SetAttributeValue(Schema.From, froms.ToArray());
            return this;
        }

        public PhoneCallEntity AddFrom_Deprecated(params DynamicEntity[] entity)
        {
            var froms = new List<DynamicEntity>(From_Deprecated ?? Array.Empty<DynamicEntity>());
            froms.AddRange(entity);
            SetAttributeValue(Schema.From, froms.ToArray());
            return this;
        }
        public ChatAccountEntity GetAccount()
        {
            return GetContact()?.Account
                ?? From_Deprecated.ToList().FirstOrDefault(x => x.LogicalName == AccountEntity.Schema.LogicalName)?.To<ChatAccountEntity>()
                ?? To_Deprecated.ToList().FirstOrDefault(x => x.LogicalName == AccountEntity.Schema.LogicalName)?.To<ChatAccountEntity>();
        }
        public ChatAccountEntity GetAccount(DynamicEntity[] ls)
        {
            return GetContact()?.Account
                ?? ls.ToList().FirstOrDefault(x => x.LogicalName == AccountEntity.Schema.LogicalName)?.To<ChatAccountEntity>();
        }
        public ContactEntity GetContact()
        {
            return From_Deprecated.ToList().FirstOrDefault(x => x.LogicalName == ContactEntity.Schema.LogicalName)?.To<ContactEntity>()
                    ?? To_Deprecated.ToList().FirstOrDefault(x => x.LogicalName == ContactEntity.Schema.LogicalName)?.To<ContactEntity>();

        }
        public ContactEntity GetContact(DynamicEntity[] ls)
        {
            return ls.ToList().FirstOrDefault(x => x.LogicalName == ContactEntity.Schema.LogicalName)?.To<ContactEntity>();

        }
        public ChatUserEntity GetUser()
        {
            return From_Deprecated.FirstOrDefault(x => x.LogicalName == UserEntity.Schema.LogicalName)?.To<ChatUserEntity>()
                ?? To_Deprecated.FirstOrDefault(x => x.LogicalName == UserEntity.Schema.LogicalName)?.To<ChatUserEntity>();
        }
        public ChatUserEntity GetUser(DynamicEntity[] ls)
        {
            return ls.FirstOrDefault(x => x.LogicalName == UserEntity.Schema.LogicalName)?.To<ChatUserEntity>();
        }
        public DynamicEntity PrioritizeFoundEntities(DynamicEntity[] ls)
        {

            return GetUser(ls) ?? GetContact(ls) ?? GetAccount(ls)?.To<DynamicEntity>() ?? null;
        }
        public string GetNameFrom(DynamicEntity[] ls)
        {
            var user = GetUser(ls);
            if (user != null)
            {
                return user.FullName;
            }
            var contact = GetContact(ls);
            var account = GetAccount(ls);
            if (contact != null && account != null)
            {
                return $"{contact.FullName} از شرکت {account.Name}";
            }
            else if (contact != null)
            {
                return contact.FullName;
            }
            else if (account != null)
            {
                return $" شرکت {account.Name}";
            }
            return null;
        }
        public string GetUrl()
        {
            return Id != null ? $"http://crm/GN/main.aspx?etn={LogicalName}&pagetype=entityrecord&id=%7B{Id}%7D#301740946"
                                    : "http://crm/gn/main.aspx?etn=phonecall&pagetype=entitylist&viewid={FD140AAF-4DF4-11DD-BD17-0019B9312238}&web=true";
        }

        public string GetNotificationSubjectForNumber(string number)
        {
            var subject = string.Empty;
            if (CallDirection == (int)ChatPhoneCallEntity.Schema.DirectionCodes.Outgoing)
            {
                var n = GetNameFrom(To_Deprecated);
                if (!string.IsNullOrWhiteSpace(n))
                    subject = $" در حال زنگ زدن به {n}";
                else
                    subject = $" در حال زنگ زدن به {number}";
            }
            else if (CallDirection == (int)ChatPhoneCallEntity.Schema.DirectionCodes.Internal
                || CallDirection == (int)ChatPhoneCallEntity.Schema.DirectionCodes.Incomming)
            {
                var n = GetNameFrom(From_Deprecated);
                if (!string.IsNullOrWhiteSpace(n))
                    subject = $" تماس تلفنی از {n}";
                else
                    subject = $" تماس تلفنی از {number}";
            }
            return subject;
        }
        public bool ShouldSendNotification()
        {
            return CallState == (int)Schema.CallStates.Ringing ||
                  GetCallType() == Schema.CallTypes.Transferee && CallState == (int)Schema.CallStates.Unknown;
        }

        public Schema.CallTypes GetCallType()
        {
            return (Schema.CallTypes)Enum.Parse(typeof(Schema.CallTypes), CallType);
        }

        
    }

}
