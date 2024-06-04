namespace DarkRift.Server.Plugins.Commands
{
    public class BasisTags
    {
        public const ushort AuthenticationTag = 0;
        public const ushort AuthenticationSucess = 1;
        public const ushort AuthenticationFailure = 2;
        public const ushort CreateRemotePlayerTag = 3;
        public const ushort CreateAllRemoteClientsTag = 4;
        public const ushort ReadyState = 5;
        public const ushort PlayerUpdateTag = 10;
        public const ushort DisconnectTag = 12;
        public const ushort VoiceAudioSegment = 16;
    }
}
