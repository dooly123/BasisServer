namespace DarkRift.Server.Plugins.Commands
{
    public class BasisTags
    {
        //CommandCode 0 & 1 are taken by CommandCode
        public const ushort AuthTag = 2;
        public const ushort AuthSuccess = 3;
        public const ushort AuthFailure = 4;
        public const ushort DisconnectTag = 5;
        public const ushort CreateRemotePlayerTag = 6;
        public const ushort CreateRemotePlayersTag = 7;
        public const ushort ReadyStateTag = 8;
        public const ushort AudioSegmentTag = 9;
        public const ushort AvatarMuscleUpdateTag = 10;
    }
}
