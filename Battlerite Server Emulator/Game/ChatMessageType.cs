namespace SKYNET
{
    public enum ChatMessageType
    {
        None,
        Message,
        RoomRequest,
        PrivateMatchInvite,
        DeclineGroupInvite,
        DeclinePrivateMatchInvite,
        CancelGroupInvite,
        CancelPrivateMatchInvite,
        RequestSilentGroupInvite,
        AcceptSilentGroupInvite,
        NotificationMessage,
        RequestSilentGroupInviteV2,
        AcceptSilentGroupInviteV2,
        DeclineSilentGroupInvite
    }
}