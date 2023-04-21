namespace Enqueuer.Core.TextProviders;

public static class CallbackMessageKeys
{
    public static class EnqueueMeCallbackHandler
    {
        public const string Callback_EnqueueMe_QueueHasBeenDeleted_Message = nameof(Callback_EnqueueMe_QueueHasBeenDeleted_Message);

        public const string Callback_EnqueueMe_UserAlreadyParticipates_Alert = nameof(Callback_EnqueueMe_UserAlreadyParticipates_Alert);

        public const string Callback_EnqueueMe_SuccessfullyEnqueued_Alert = nameof(Callback_EnqueueMe_SuccessfullyEnqueued_Alert);
    }

    public static class ListChatsCallbackHandler
    {
        public const string Callback_ListChats_UserDoesNotParticipateInAnyGroup_Message = nameof(Callback_ListChats_UserDoesNotParticipateInAnyGroup_Message);

        public const string Callback_ListChats_DisplayChatsList_Message = nameof(Callback_ListChats_DisplayChatsList_Message);
    }

    public static class GetChatCallbackHandler
    {
        public const string Callback_GetChat_ChatHasNoQueues_Message = nameof(Callback_GetChat_ChatHasNoQueues_Message);

        public const string Callback_GetChat_DisplayQueuesList_Message = nameof(Callback_GetChat_DisplayQueuesList_Message);

        public const string Callback_GetChat_DisplayQueueList_PostScriptum_Message = nameof(Callback_GetChat_DisplayQueueList_PostScriptum_Message);
    }

    public static class GetQueueCallbackHandler
    {
        public const string Callback_GetQueue_EnqueueMe_Button = nameof(Callback_GetQueue_EnqueueMe_Button);

        public const string Callback_GetQueue_DequeueMe_Button = nameof(Callback_GetQueue_DequeueMe_Button);

        public const string Callback_GetQueue_RemoveQueue_Button = nameof(Callback_GetQueue_RemoveQueue_Button);

        public const string Callback_GetQueue_MakeQueueStatic_Button = nameof(Callback_GetQueue_MakeQueueStatic_Button);

        public const string Callback_GetQueue_MakeQueueDynamic_Button = nameof(Callback_GetQueue_MakeQueueDynamic_Button);

        public const string Callback_GetQueue_ListQueueMembers_QueueIsEmpty_Message = nameof(Callback_GetQueue_ListQueueMembers_QueueIsEmpty_Message);

        public const string Callback_GetQueue_ListQueueMembers_Message = nameof(Callback_GetQueue_ListQueueMembers_Message);

        public const string Callback_GetQueue_ListQueueMembers_QueueIsDynamic_PostScriptum_Message = nameof(Callback_GetQueue_ListQueueMembers_QueueIsDynamic_PostScriptum_Message);

        public const string Callback_GetQueue_SwapPositions_Button = nameof(Callback_GetQueue_SwapPositions_Button);
    }

    public static class EnqueueCallbackHandler
    {
        public const string Callback_Enqueue_QueueHasBeenDeleted_Message = nameof(Callback_Enqueue_QueueHasBeenDeleted_Message);

        public const string Callback_Enqueue_FirstAvailable_Button = nameof(Callback_Enqueue_FirstAvailable_Button);

        public const string Callback_Enqueue_SelectPosition_Message = nameof(Callback_Enqueue_SelectPosition_Message);
    }

    public static class EnqueueAtCallbackHandler
    {
        public const string Callback_EnqueueAt_UserAlreadyParticipates_Message = nameof(Callback_EnqueueAt_UserAlreadyParticipates_Message);

        public const string Callback_EnqueueAt_QueueIsFull_Message = nameof(Callback_EnqueueAt_QueueIsFull_Message);

        public const string Callback_EnqueueAt_QueueIsDynamicButPositionIsSpecified_Message = nameof(Callback_EnqueueAt_QueueIsDynamicButPositionIsSpecified_Message);

        public const string Callback_EnqueueAt_PositionIsReserved_Message = nameof(Callback_EnqueueAt_PositionIsReserved_Message);

        public const string Callback_EnqueueAt_PositionIsReserved_ChooseAnother_Button = nameof(Callback_EnqueueAt_PositionIsReserved_ChooseAnother_Button);

        public const string Callback_EnqueueAt_Success_Message = nameof(Callback_EnqueueAt_Success_Message);
    }

    public static class DequeueMeCallbackHandler
    {
        public const string Callback_DequeueMe_UserDoesNotParticipate_Message = nameof(Callback_DequeueMe_UserDoesNotParticipate_Message);

        public const string Callback_DequeueMe_AreYouSureToBeDequeued_Message = nameof(Callback_DequeueMe_AreYouSureToBeDequeued_Message);

        public const string Callback_DequeueMe_AgreeToDequeue_Button = nameof(Callback_DequeueMe_AgreeToDequeue_Button);

        public const string Callback_DequeueMe_Success_Message = nameof(Callback_DequeueMe_Success_Message);
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
