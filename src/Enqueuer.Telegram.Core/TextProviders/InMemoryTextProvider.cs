using System;

namespace Enqueuer.Core.TextProviders;

public class InMemoryTextProvider : IMessageProvider
{
    public string GetMessage(string key, params object[] args)
    {
        var formatText = key switch
        {
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
