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
        public const string Callback_RemoveQueue_QueueHasBeenDeleted_Message = nameof(Callback_RemoveQueue_QueueHasBeenDeleted_Message);

        public const string Callback_RemoveQueue_AreYouSureToDeleteQueue_Message = nameof(Callback_RemoveQueue_AreYouSureToDeleteQueue_Message);

        public const string Callback_RemoveQueue_AgreeToDelete_Button = nameof(Callback_RemoveQueue_AgreeToDelete_Button);

        public const string Callback_RemoveQueue_UserHasNoRightsToDelete_Message = nameof(Callback_RemoveQueue_UserHasNoRightsToDelete_Message);

        public const string Callback_RemoveQueue_Success_PublicChat_Message = nameof(Callback_RemoveQueue_Success_PublicChat_Message);

        public const string Callback_RemoveQueue_Success_Message = nameof(Callback_RemoveQueue_Success_Message);
    }

    public static class SwitchQueueCallbackHandler
    {
        public const string Callback_SwitchQueue_QueueIsNotDynamicNow_Message = nameof(Callback_SwitchQueue_QueueIsNotDynamicNow_Message);

        public const string Callback_SwitchQueue_QueueIsDynamicNow_PublicChat_Message = nameof(Callback_SwitchQueue_QueueIsDynamicNow_PublicChat_Message);

        public const string Callback_SwitchQueue_QueueIsDynamicNow_Message = nameof(Callback_SwitchQueue_QueueIsDynamicNow_Message);
    }

    public static class SwapPositionsCallbackHandler
    {
        public const string Callback_SwapPositions_QueueHasBeenDeleted_Message = nameof(Callback_SwapPositions_QueueHasBeenDeleted_Message);

        public const string Callback_SwapPositions_UserDoesNotParticipate_Message = nameof(Callback_SwapPositions_UserDoesNotParticipate_Message);

        public const string Callback_SwapPositions_SelectUserToSwapWith_Message = nameof(Callback_SwapPositions_SelectUserToSwapWith_Message);

        public const string Callback_SwapPositions_UserHasLeftTheQueue_Message = nameof(Callback_SwapPositions_UserHasLeftTheQueue_Message);

        public const string Callback_SwapPositions_UserWantsToSwapWithYou_Message = nameof(Callback_SwapPositions_UserWantsToSwapWithYou_Message);

        public const string Callback_SwapPositions_SwapRequestHasBeenSent_Message = nameof(Callback_SwapPositions_SwapRequestHasBeenSent_Message);

        public const string Callback_SwapPositions_RequestersPositionHasChanged_Message = nameof(Callback_SwapPositions_RequestersPositionHasChanged_Message);

        public const string Callback_SwapPositions_SuccessfullySwappedPositions_Message = nameof(Callback_SwapPositions_SuccessfullySwappedPositions_Message);

        public const string Callback_SwapPositions_SwapWasSuccessfulyRefused_Message = nameof(Callback_SwapPositions_SwapWasSuccessfulyRefused_Message);

        public const string Callback_SwapPositions_SwapRequestWasRefusedByUser_Message = nameof(Callback_SwapPositions_SwapRequestWasRefusedByUser_Message);

        public const string Callback_SwapPositions_AgreeToSwap_Button = nameof(Callback_SwapPositions_AgreeToSwap_Button);

        public const string Callback_SwapPositions_RefuseToSwap_Button = nameof(Callback_SwapPositions_RefuseToSwap_Button);

        public const string Callback_SwapPositions_ListOfMembers_OnlyPreviousPageAvailable_Button = nameof(Callback_SwapPositions_ListOfMembers_OnlyPreviousPageAvailable_Button);

        public const string Callback_SwapPositions_ListOfMembers_OnlyNextPageAvailable_Button = nameof(Callback_SwapPositions_ListOfMembers_OnlyNextPageAvailable_Button);
    }

    public const string Callback_OutdatedCallback_Message = nameof(Callback_OutdatedCallback_Message);

    public const string Callback_EverythingIsUpToDate_Message = nameof(Callback_EverythingIsUpToDate_Message);

    public const string Callback_RefreshMessage_Button = nameof(Callback_RefreshMessage_Button);

    public const string Callback_ChatHasBeenDeleted_Message = nameof(Callback_ChatHasBeenDeleted_Message);

    public const string Callback_QueueHasBeenDeleted_Message = nameof(Callback_QueueHasBeenDeleted_Message);

    public const string Callback_Return_Button = nameof(Callback_Return_Button);
}
