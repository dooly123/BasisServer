namespace DarkRift.Server.Plugins.Commands
{
    public class BasisTags
    {
        //CommandCode 0 & 1 are taken by CommandCode
        //dont want to use just in case we do something in the future. were they use this
        public const ushort PingTag = 0;
        public const ushort AuthSuccess = 3;
        public const ushort DisconnectTag = 5;
        public const ushort CreateRemotePlayerTag = 6;
        public const ushort CreateRemotePlayersTag = 7;
        public const ushort ReadyStateTag = 8;
        public const ushort AudioSegmentTag = 9;
        public const ushort AvatarMuscleUpdateTag = 10;
        public const ushort AvatarChangeMessage = 11;
        public const ushort SceneGenericMessage = 12;
        public const ushort AvatarGenericMessage = 13;
    }
}
