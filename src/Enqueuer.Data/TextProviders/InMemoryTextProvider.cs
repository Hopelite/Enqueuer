using System;
using Telegram.Bot.Types;

namespace Enqueuer.Data.TextProviders;

public class InMemoryTextProvider : IMessageProvider
{
    public string GetMessage(string key, params object[] args)
    {
        var formatText = key switch
        {
            MessageKeys.StartMessageHandler.StartCommand_PublicChat_Message => "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "To get the list of commands, write '<b>/help</b>'.\n"
                + "<i>Please, message this guy (@hopelite) to get help, give feedback or report a bug.</i>\n",
            MessageKeys.StartMessageHandler.StartCommand_PrivateChat_Message => "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "And your personal queue manager too!\n"
                + "Start by pressing the button below:",
            MessageKeys.StartMessageHandler.StartCommand_PrivateChat_ListChatsButton => "View chats",

            MessageKeys.HelpMessageHandler.HelpCommand_Message => "Here is the list of available commands with short description:\n"
                + "<b>/start</b> - get introducing message\n"
                + "<b>/help</b> - get bot help\n"
                + "<b>/queue</b> - list chat queues or get info about one of them\n"
                + "<b>/createqueue</b> - create new queue\n"
                + "<b>/enqueue</b> - add yourself to the end of a queue or to a specified position\n"
                + "<b>/dequeue</b> - remove yourself from a queue\n"
                + "<b>/removequeue</b> - delete a queue\n"
                + "To get slightly more detailed info about one of them, write the appropriate command.",

            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueDoesNotExist_Message => "This chat has no queue with name '<b>{0}</b>'.",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_QueueEmpty_Message => "Queue '<b>{0}</b>' has no participants.",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueueParticipants_Message => "'<b>{0}</b>' has these participants:\n",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_DisplayQueueParticipant_Message => "{0}) <b>{1}</b>",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_NoQueues_Message => "This chat has no queues. To create new one write '<b>/createqueue</b> <i>queue_name</i>'.",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_Message => "This chat has these queues:\n",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_DisplayQueue_Message => "• {0}",
            MessageKeys.QueueMessageHandler.QueueCommand_PublicChat_ListQueues_PostScriptum_Message => "To get info about one of them write '<b>/queue</b> <i>[queue_name]</i>'.",

            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message => "To be enqueued, please write the command this way: '<b>/enqueue</b> <i>[queue_name] [position(optional)]</i>'.",
            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message => "Please, use positive numbers for user position.",
            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_QueueDoesNotExist_Message => "There is no queue with name '<b>{0}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_UserAlreadyParticipates_Message => "You're already participating in queue '<b>{0}</b>'.",
            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message => "Queue '<b>{0}</b>' is dynamic - you can enqueue yourself only at the end of queue.",
            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_PositionIsReserved_Message => "Position '<b>{0}</b>' in queue '<b>{1}</b>' is reserved. Please, reserve other position.",
            MessageKeys.EnqueueMessageHandler.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message => "Successfully added to queue '<b>{0}</b>' on position <b>{1}</b>!",

            MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_QueueNameIsNotProvided_Message => "Please write the command this way: '/<b>dequeue</b> <i>[queue_name]</i>'.",
            MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_QueueDoesNotExist_Message => "There is no queue with name '<b>{0}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
            MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_UserDoesNotParticipate_Message => "You're not participating in queue '<b>{0}</b>'.",
            MessageKeys.DequeueMessageHandler.DequeueCommand_PublicChat_SuccessfullyDequeued_Message => "Successfully removed from queue '<b>{0}</b>'!",

            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message => "To create a new queue, please write the command this way: '<b>/createqueue</b> <i>[queue_name]</i>'.",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message => "This chat has reached its maximum number of queues. Please remove one using the '<b>/removequeue</b>' command before adding a new one.",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message => "Unable to create a queue with a number at the last position of its name. Please concat the queue name like this: '<b>Test 23</b>' => '<b>Test23</b>' or remove the number.",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message => "Unable to create a queue with only a number in its name. Please add some nice words.",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueNameTooLong_Message => "This queue name is too long. Please, provide it with a name shorter than 50 symbols.",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message => "Successfully created a new queue '<b>{0}</b>'!",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_QueueAlreadyExists_Message => "This chat already has a queue named '<b>{0}</b>'. Please, use some other name for this queue or delete the existing one using '<b>/removequeue</b>'.",
            MessageKeys.CreateQueueMessageHandler.CreateQueueCommand_PublicChat_EnqueueMeButton => "Enqueue me!",

            MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message => "To delete queue, please write command this way: '<b>/removequeue</b> <i>[queue_name]</i>'.",
            MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message => "There is no queue with name '<b>{0}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
            MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message => "Unable to delete queue '<b>{0}</b>'. It can be deleted only by it's creator or chat administrators.",
            MessageKeys.RemoveQueueMessageHandler.RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message => "Successfully deleted queue '<b>{0}</b>'!",

            MessageKeys.UnsupportedCommand_Message => "Bot does not support the '{0}' command.",
            MessageKeys.UnsupportedCommand_PrivateChat_Message => "Bot does not support commands in private chats except '<b>/start</b>'. Please, use interface it provides.",

            CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_QueueHasBeenDeleted_Message => "This queue has been deleted. Please, create a new one to participate in.",
            CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_UserAlreadyParticipates_Notification => "You're already participating in queue '{0}'!",
            CallbackMessageKeys.EnqueueMeCallbackHandler.EnqueueMeCallback_SuccessfullyEnqueued_Notification => "Successfully added to queue '{0}' at the '{1}' position!",

            CallbackMessageKeys.ListChatsCallbackHandler.ListChatsCallback_UserDoesNotParticipateInAnyGroup_Message => "I haven't seen you before. Please, write any command in any chat with me, and I'll notice you there. Then come here and write <b>'/start'</b> again.",
            CallbackMessageKeys.ListChatsCallbackHandler.ListChatsCallback_ListChats_Message => "I know that you do participate in these chats. If one of the chats is not presented, please write any command in this chat, and I'll notice you there.",

            CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ChatHasNoQueues_Message => "This chat has no queues. Are you thinking of creating one?",
            CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ListQueues_Message => "This chat has these queues. You can manage any one of them by selecting it.",
            CallbackMessageKeys.GetChatCallbackHandler.GetChatCallback_ListQueues_PostScriptum_Message => "\n<i>Currently, you can create queues only by writting the '<b>/createqueue</b>' command in this chat, but I'll learn how to create them in direct messages soon!</i>",

            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_EnqueueMe_Button => "Enqueue me",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_DequeueMe_Button => "Dequeue me",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_RemoveQueue_Button => "Remove queue",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_MakeQueueStatic_Button => "Make static",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_MakeQueueDynamic_Button => "Make dynamic",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsEmpty_Message => "Queue <b>'{0}'</b> has no participants.",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_Message => "Queue <b>'{0}'</b> has these participants:\n",
            CallbackMessageKeys.GetQueueCallbackHandler.GetQueueCallback_ListQueueMembers_QueueIsDynamic_PostScriptum_Message => "Queue is <i>dynamic</i>",

            CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_QueueHasBeenDeleted_Message => "This queue has been deleted.",
            CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_FirstAvailable_Button => "First available",
            CallbackMessageKeys.EnqueueCallbackHandler.EnqueueCallback_SelectPosition_Message => "Select an available position in queue <b>'{0}'</b>:",

            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueHasBeenDeleted_Message => "This queue has been deleted.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_UserAlreadyParticipates_Message => "You're already participating in queue '<b>{0}</b>'. To change your position, please, dequeue yourself first.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_QueueIsDynamicButPositionIsSpecified_Message => "Queue '<b>{0}</b>' is now dynamic. Please, return and press the 'First available' button.",
            CallbackMessageKeys.EnqueueAtCallbackHandler.EnqueueAtCallback_PositionIsReserved_Message => "Queue '<b>{0}</b>' is now dynamic. Please, return and press the 'First available' button.",

            CallbackMessageKeys.OutdatedCallback_Message => "This message is outdated.",
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
