using GN.Library.Shared;
using GN.Library.Shared.Authorization;
using GN.Library.Shared.Chats;
using GN.Library.Shared.Entities;
using GN.Library.Shared.Internals;
using GN.Library.Shared.Telephony;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library
{
    public static partial class SharedLibraryExtensions
    {
        public static Task<AuthenticateResponse> Authenticate(this IProcedureCall rpc, AuthenticateCommand request)
        {
            return rpc.Call<AuthenticateCommand, AuthenticateResponse>(request);
        }

        public static Task<UpdatePhoneCallSubjectReply> UpdatePhoneCallSubject(this IProcedureCall rpc, string id, string subject)
        {
            return rpc.Call<UpdatePhoneCallSubjectCommand, UpdatePhoneCallSubjectReply>(new UpdatePhoneCallSubjectCommand
            {
                Id = id,
                Subject = subject
            });
        }
        public static async Task<LoadDynamicEntityReply> LoadDynamicEntity(this IProcedureCall rpc, string logicalName, string id)
        {
            var result = await rpc.Call<LoadDynamicEntityCommand, LoadDynamicEntityReply>(new LoadDynamicEntityCommand
            {
                Id = id,
                LogicalName = logicalName
            });

            return result;

        }
        public static async Task<InitiateNotificationReply> InitiateNotification(this IProcedureCall THIS, InitiateNotificationCommand notif)
        {
            var result = await THIS.Call<InitiateNotificationCommand, InitiateNotificationReply>(notif);
            return result;
        }
        public static async Task<CreatePhoneCallContactReply> CreatePhoneCallContact(this IProcedureCall rpc, string firstName, string lastName, string phoneNumber, string phoneRecordId)
        {
            var result = await rpc.Call<CreatePhoneCallContactCommand, CreatePhoneCallContactReply>(new CreatePhoneCallContactCommand
            {
                FirstName = firstName,
                LastName = lastName,
                Phone = phoneNumber,
                PhoneRecordId = phoneRecordId
            });

            return result;
        }
        public static async Task<CreatePhoneCallContactWithAccountReply> CreatePhoneCallContactWithAccount(this IProcedureCall THIS, CreatePhoneCallContactWithAccountCommand command)
        {
            var res = await THIS.Call<CreatePhoneCallContactWithAccountCommand, CreatePhoneCallContactWithAccountReply>(command);
            return res;
        }
        public static async Task<AddExistingContactToPhoneCallReply> AddExistingContactToPhoneCall(this IProcedureCall THIS, string PhoneCallId, string ContactId)
        {
            var res = await THIS.Call<AddExistingContactToPhoneCallCommand, AddExistingContactToPhoneCallReply>(new AddExistingContactToPhoneCallCommand()
            {
                ContactId = ContactId,
                PhoneRecordId = PhoneCallId
            });
            return res;
        }
        public static async Task<ResolveIdentityReply> ResolveUser(this IProcedureCall rpc, DynamicEntity user)
        {
            var result = await rpc.Call<ResolveIdentityCommand, ResolveIdentityReply>(new ResolveIdentityCommand
            {
                User = user
            });

            return result;


        }
    }
}
