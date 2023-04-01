namespace Enqueuer.Data.TextProviders;

public static class CallbackMessageKeys
{
    public static class EnqueueMeCallbackHandler
    {
        public const string EnqueueMeCallback_QueueHasBeenDeleted_Message = "EnqueueMeCallback_QueueHasBeenDeleted_Message";

        public const string EnqueueMeCallback_UserAlreadyParticipates_Notification = "EnqueueMeCallback_UserAlreadyParticipates_Notification";

        public const string EnqueueMeCallback_SuccessfullyEnqueued_Notification = "EnqueueMeCallback_SuccessfullyEnqueued_Notification";
    }

    public static class ListChatsCallbackHandler
    {
        public const string ListChatsCallback_UserDoesNotParticipateInAnyGroup_Message = "ListChatsCallback_UserDoesNotParticipateInAnyGroup_Message";

        public const string ListChatsCallback_ListChats_Message = "ListChatsCallback_ListChats_Message";
    }

    public static class GetChatCallbackHandler
    {
        public const string GetChatCallback_ChatHasNoQueues_Message = "GetChatCallback_ChatHasNoQueues_Message";

        public const string GetChatCallback_ListQueues_Message = "GetChatCallback_ListQueues_Message";

        public const string GetChatCallback_ListQueues_PostScriptum_Message = "GetChatCallback_ListQueues_PostScriptum_Message";
    }

    public static class GetQueueCallbackHandler
    {
        public const string GetQueueCallback_EnqueueMe_Button = "GetQueueCallback_EnqueueMe_Button";

        public const string GetQueueCallback_DequeueMe_Button = "GetQueueCallback_DequeueMe_Button";

        public const string GetQueueCallback_RemoveQueue_Button = "GetQueueCallback_RemoveQueue_Button";

        public const string GetQueueCallback_MakeQueueStatic_Button = "GetQueueCallback_MakeQueueStatic_Button";

        public const string GetQueueCallback_MakeQueueDynamic_Button = "GetQueueCallback_MakeQueueDynamic_Button";

        public const string GetQueueCallback_ListQueueMembers_QueueIsEmpty_Message = "GetQueueCallback_ListQueueMembers_QueueIsEmpty_Message";

        public const string GetQueueCallback_ListQueueMembers_Message = "GetQueueCallback_ListQueueMembers_Message";

        public const string GetQueueCallback_ListQueueMembers_QueueIsDynamic_PostScriptum_Message = "GetQueueCallback_ListQueueMembers_QueueIsDynamic_PostScriptum_Message";

        public const string GetQueueCallback_SwapPositions_Button = "GetQueueCallback_SwapPositions_Button";
    }

    public static class EnqueueCallbackHandler
    {
        public const string EnqueueCallback_QueueHasBeenDeleted_Message = "EnqueueCallback_QueueHasBeenDeleted_Message";

        public const string EnqueueCallback_FirstAvailable_Button = "EnqueueCallback_FirstAvailable_Button";

        public const string EnqueueCallback_SelectPosition_Message = "EnqueueCallback_SelectPosition_Message";
    }

    public static class EnqueueAtCallbackHandler
    {
        public const string EnqueueAtCallback_UserAlreadyParticipates_Message = "EnqueueAtCallback_UserAlreadyParticipates_Message";

        public const string EnqueueAtCallback_QueueIsDynamicButPositionIsSpecified_Message = "EnqueueAtCallback_QueueIsDynamicButPositionIsSpecified_Message";

        public const string EnqueueAtCallback_PositionIsReserved_Message = "EnqueueAtCallback_PositionIsReserved_Message";

        public const string EnqueueAtCallback_Success_Message = "EnqueueAtCallback_Success_Message";
    }

    public static class DequeueMeCallbackHandler
    {
        public const string DequeueMeCallback_UserDoesNotParticipate_Message = "DequeueMeCallback_UserDoesNotParticipate_Message";

        public const string DequeueMeCallback_AreYouSureToBeDequeued_Message = "DequeueMeCallback_AreYouSureToBeDequeued_Message";

        public const string DequeueMeCallback_AgreeToDequeue_Button = "DequeueMeCallback_AgreeToDequeue_Button";

        public const string DequeueMeCallback_Success_Message = "DequeueMeCallback_Success_Message";
    }

    public static class RemoveQueueCallbackHandler
    {
        public const string RemoveQueueCallback_QueueHasBeenDeleted_Message = "RemoveQueueCallback_QueueHasBeenDeleted_Message";

        public const string RemoveQueueCallback_AreYouSureToDeleteQueue_Message = "RemoveQueueCallback_AreYouSureToDeleteQueue_Message";

        public const string RemoveQueueCallback_AgreeToDelete_Button = "RemoveQueueCallback_AgreeToDelete_Button";

        public const string RemoveQueueCallback_UserHasNoRightsToDelete_Message = "RemoveQueueCallback_UserHasNoRightsToDelete_Message";

        public const string RemoveQueueCallback_Success_PublicChat_Message = "RemoveQueueCallback_Success_PublicChat_Message";

        public const string RemoveQueueCallback_Success_Message = "RemoveQueueCallback_Success_Message";
    }

    public static class SwitchQueueCallbackHandler
    {
        public const string SwitchQueueCallback_QueueIsNotDynamicNow_Message = "SwitchQueueCallback_QueueIsNotDynamicNow_Message";

        public const string SwitchQueueCallback_QueueIsDynamicNow_PublicChat_Message = "SwitchQueueCallback_QueueIsDynamicNow_PublicChat_Message";

        public const string SwitchQueueCallback_QueueIsDynamicNow_Message = "SwitchQueueCallback_QueueIsDynamicNow_Message";
    }

    public static class SwapPositionsCallbackHandler
    {
        public const string SwapPositionsCallback_QueueHasBeenDeleted_Message = "SwapPositionsCallback_QueueHasBeenDeleted_Message";

        public const string SwapPositionsCallback_UserDoesNotParticipate_Message = "SwapPositionsCallback_UserDoesNotParticipate_Message";

        public const string SwapPositionsCallback_SelectUserToSwapWith_Message = "SwapPositionsCallback_SelectUserToSwapWith_Message";

        public const string SwapPositionsCallback_UserHasLeftTheQueue_Message = "SwapPositionsCallback_UserHasLeftTheQueue_Message";

        public const string SwapPositionsCallback_UserWantsToSwapWithYou_Message = "SwapPositionsCallback_UserWantsToSwapWithYou_Message";

        public const string SwapPositionsCallback_SwapRequestHasBeenSent_Message = "SwapPositionsCallback_SwapRequestHasBeenSent_Message";

        public const string SwapPositionsCallback_RequestersPositionHasChanged_Message = "SwapPositionsCallback_RequestersPositionHasChanged_Message";

        public const string SwapPositionsCallback_SuccessfullySwappedPositions_Message = "SwapPositionsCallback_SuccessfullySwappedPositions_Message";

        public const string SwapPositionsCallback_SwapWasSuccessfulyRefused_Message = "SwapPositionsCallback_SwapWasSuccessfulyDenied_Message";

        public const string SwapPositionsCallback_SwapRequestWasRefusedByUser_Message = "SwapPositionsCallback_SwapWasDeniedByUser_Message";

        public const string SwapPositionsCallback_AgreeToSwap_Button = "SwapPositionsCallback_AgreeToSwap_Button";

        public const string SwapPositionsCallback_RefuseToSwap_Button = "SwapPositionsCallback_RefuseToSwap_Button";

        public const string SwapPositionsCallback_ListOfMembers_OnlyPreviousPageAvailable_Button = "SwapPositionsCallback_ListOfMembers_OnlyPreviousPageAvailable_Button";

        public const string SwapPositionsCallback_ListOfMembers_OnlyNextPageAvailable_Button = "SwapPositionsCallback_ListOfMembers_OnlyNextPageAvailable_Button";
    }

    public const string OutdatedCallback_Message = "CallbackMessageKeys_OutdatedCallback_Message";

    public const string EverythingIsUpToDate_Message = "CallbackMessageKeys_EverythingIsUpToDate_Message";

    public const string RefreshMessage_Button = "CallbackMessageKeys_RefreshMessage_Button";

    public const string ChatHasBeenDeleted_Message = "CallbackMessageKeys_ChatHasBeenDeleted_Message";

    public const string QueueHasBeenDeleted_Message = "QueueHasBeenDeleted_Message";

    public const string Return_Button = "CallbackMessageKeys_Return_Button";
}
