namespace DarkRift.Server.Plugins.Commands
{
    public class BasisTags
    {
        // CommandCode 0 & 1 are reserved for future use to avoid conflicts
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

        // Tags for No Recipients and No Payload combinations
        public const ushort AvatarGenericMessage_NoRecipients_NoPayload = 14;
        public const ushort AvatarGenericMessage_NoRecipients = 15;
        public const ushort SceneGenericMessage_NoRecipients_NoPayload = 16;
        public const ushort SceneGenericMessage_NoRecipients = 17;

        // Ownership-related tags
        public const ushort OwnershipResponse = 18; // Request to determine who owns an object
        public const ushort OwnershipTransfer = 19; // Request to transfer ownership of an object

        // New additions for Recipients but No Payload
        public const ushort AvatarGenericMessage_Recipients_NoPayload = 20;
        public const ushort SceneGenericMessage_Recipients_NoPayload = 21;

        public const ushort AudioCommunication = 22;
    }
}
