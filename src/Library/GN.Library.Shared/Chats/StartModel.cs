namespace GN.Library.Shared.Chats
{
	public class StartModel
	{

		public ChannelStartModel[] Channels { get; set; }

	}
	public class SignInOptions
    {
		public bool UseChannels { get; set; }
		public bool StartMyUpdateService { get; set; }
		public ChannelStartModel[] Channels { get; set; }
		public long? LastSynchedOn { get; set; }
		public string Application { get; set; }

        public override string ToString()
        {
			return $"MyUpdateService:{StartMyUpdateService}";
        }

    }
	public class SignInModel
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Token { get; set; }
		public SignInOptions Options { get; set; }
		
	}
	public class UserSignedIn
    {
		public string UserId { get; set; }
		public string Token { get; set; }
		public SignInOptions Options { get; set; }
	}
	public class UserDisconnected
	{
		public string UserId { get; set; }
	}
	public class SignInReply
    {
		public bool Success { get; set; }
		public string Token { get; set; }
		public string UserId { get; set; }
		public string DisplayName { get; set; }
    }
}
