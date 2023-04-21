using System;

namespace Enqueuer.Core.TextProviders;

public class InMemoryTextProvider : IMessageProvider
{
    public string GetMessage(string key, params object[] args)
    {
        var formatText = key switch
        {
            CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_QueueHasBeenDeleted_Message => "This queue has been deleted. Please, create a new one to participate in.",
            CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_UserAlreadyParticipates_Notification => "You're already participating in queue '{0}'!",
            CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_SuccessfullyEnqueued_Notification => "Successfully added to queue '{0}' at the '{1}' position!",

            CallbackMessageKeys.ListChatsCallbackHandler.ListChatsCallback_UserDoesNotParticipateInAnyGroup_Message => "I haven't seen you before. Please, write any command in any chat with me, and I'll notice you there.",
            CallbackMessageKeys.ListChatsCallbackHandler.ListChatsCallback_ListChats_Message => "I know that you do participate in these chats. If one of the chats is not presented, please write any command in this chat, and I'll notice you there.",

            CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ChatHasNoQueues_Message => "This chat has no queues. Are you thinking of creating one?",
            CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ListQueues_Message => "This chat has these queues. You can manage any one of them by selecting it.",
            CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ListQueues_PostScriptum_Message => "\n<i>Currently, you can create queues only by writting the '<b>/createqueue</b>' command in this chat, but I'll learn how to create them in direct messages soon!</i>",

            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_EnqueueMe_Button => "Enqueue me",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_DequeueMe_Button => "Dequeue me",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_RemoveQueue_Button => "Remove queue",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_MakeQueueStatic_Button => "Make static",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_MakeQueueDynamic_Button => "Make dynamic",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_SwapPositions_Button => "Swap positions",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsEmpty_Message => "Queue <b>'{0}'</b> has no participants.",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_Message => "Queue <b>'{0}'</b> has these participants:\n",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsDynamic_PostScriptum_Message => "Queue is <i>dynamic</i>",

            CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_QueueHasBeenDeleted_Message => "This queue has been deleted.",
            CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_FirstAvailable_Button => "First available",
            CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_SelectPosition_Message => "Select an available position in queue <b>'{0}'</b>:",

            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_UserAlreadyParticipates_Message => "You're already participating in queue '<b>{0}</b>'. To change your position, please, dequeue yourself first.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueIsFull_Message => "All possible positions in queue <b>'{0}' are reserved. Please, wait untill someone leaves to join.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueIsDynamicButPositionIsSpecified_Message => "Queue '<b>{0}</b>' is now dynamic. Please, return and press the 'First available' button.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_PositionIsReserved_Message => "Position '<b>{0}</b>' in queue '<b>{1}</b>' is reserved. Please, reserve other position.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_PositionIsReserved_ChooseAnother_Button => "Choose another",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_Success_Message => "Successfully added to the queue '<b>{0}</b>' on position <b>{1}</b>!",

            CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_UserDoesNotParticipate_Message => "You've already dequeued from the '<b>{0}</b>' queue.",
            CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_Success_Message => "Successfully removed from the '<b>{0}</b>' queue!",
            CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_AreYouSureToBeDequeued_Message => "Do you really want to leave the <b>'{0}'</b> queue?",
            CallbackMessageKeys.DequeueMeCallbackHandler.DequeueMeCallback_AgreeToDequeue_Button => "Yes, I want to leave",

            CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_QueueHasBeenDeleted_Message => "This queue has already been deleted.",
            CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_AreYouSureToDeleteQueue_Message => "Do you really want to delete the <b>'{0}'</b> queue? This action cannot be undone.",
            CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_AgreeToDelete_Button => "Yes, delete it",
            CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_UserHasNoRightsToDelete_Message => "Unable to delete <b>'{0}'</b> queue: you are not queue creator or the chat admin.",
            CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_Success_PublicChat_Message => "{0} deleted <b>'{1}'</b> queue. I shall miss it.",
            CallbackMessageKeys.RemoveQueueCallbackHandler.RemoveQueueCallback_Success_Message => "Successfully deleted the <b>'{0}'</b> queue.",

            CallbackMessageKeys.SwitchQueueCallbackHandler.SwitchQueueCallback_QueueIsNotDynamicNow_Message => "Queue <b>'{0}'</b> is not dynamic now.",
            CallbackMessageKeys.SwitchQueueCallbackHandler.SwitchQueueCallback_QueueIsDynamicNow_PublicChat_Message => "{0} made <b>'{1}'</b> queue dynamic. Keep up!",
            CallbackMessageKeys.SwitchQueueCallbackHandler.SwitchQueueCallback_QueueIsDynamicNow_Message => "Queue <b>'{0}'</b> is dynamic now.",

            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_QueueHasBeenDeleted_Message => "This queue has been deleted.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_UserDoesNotParticipate_Message => "You're not participating in the '<b>{0}</b>' queue.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_SelectUserToSwapWith_Message => "Select the user with whom you want to request a position swap.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_UserHasLeftTheQueue_Message => "User has left the '<b>{0}</b>' queue. Swap was not executed.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_UserWantsToSwapWithYou_Message => "<b>{0}</b> wants to swap their '{1}' position with your in the '<b>{2}</b>' queue.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_SwapRequestHasBeenSent_Message => "The position swap request has been sent. Waiting response from {0}!",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_RequestersPositionHasChanged_Message => "<b>{0}</b> position has changed. Another position swap request is needed.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_SuccessfullySwappedPositions_Message => "Successfully swapped positions with <b>{0}</b>!",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_SwapWasSuccessfulyRefused_Message => "Positions switch with <b>{0}</b> was rejected.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_SwapRequestWasRefusedByUser_Message => "<b>{0}</b> was refused to swap positions.",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_AgreeToSwap_Button => "Lets swap!",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_RefuseToSwap_Button => "Refuse",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_ListOfMembers_OnlyPreviousPageAvailable_Button => "Previous",
            CallbackMessageKeys.SwapPositionsCallbackHandler.SwapPositionsCallback_ListOfMembers_OnlyNextPageAvailable_Button => "Next",

            CallbackMessageKeys.OutdatedCallback_Message => "This message is outdated. Please use newer messages.",
            CallbackMessageKeys.EverythingIsUpToDate_Message => "Everything is up to date.",
            CallbackMessageKeys.RefreshMessage_Button => "Refresh",
            CallbackMessageKeys.ChatHasBeenDeleted_Message => "This chat has been deleted.",
            CallbackMessageKeys.QueueHasBeenDeleted_Message => "This queue has been deleted.",
            CallbackMessageKeys.Return_Button => "Return",
            _ => throw new Exception()
        };

        return string.Format(formatText, args);
    }
}
