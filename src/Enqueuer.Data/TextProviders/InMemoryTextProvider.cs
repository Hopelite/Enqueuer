using System;

namespace Enqueuer.Data.TextProviders;

public class InMemoryTextProvider : IMessageProvider
{
    public string GetMessage(string key, params object[] args)
    {
        var formatText = key switch
        {
            TextKeys.StartCommand_PublicChat_Message => "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "To get the list of commands, write '<b>/help</b>'.\n"
                + "<i>Please, message this guy (@hopelite) to get help, give feedback or report a bug.</i>\n",
            TextKeys.StartCommand_PrivateChat_Message => "Hello there! I'm <b>Enqueuer Bot</b>, the master of creating and managing queues.\n"
                + "And your personal queue manager too!\n"
                + "Start by pressing the button below:",
            TextKeys.StartCommand_PrivateChat_ListChatsButton => "View chats",

            TextKeys.HelpCommand_Message => "Here is the list of available commands with short description:\n"
                + "<b>/start</b> - get introducing message\n"
                + "<b>/help</b> - get bot help\n"
                + "<b>/queue</b> - list chat queues or get info about one of them\n"
                + "<b>/createqueue</b> - create new queue\n"
                + "<b>/enqueue</b> - add yourself to the end of a queue or to a specified position\n"
                + "<b>/dequeue</b> - remove yourself from a queue\n"
                + "<b>/removequeue</b> - delete a queue\n"
                + "To get slightly more detailed info about one of them, write the appropriate command.",

            TextKeys.QueueCommand_PublicChat_QueueDoesNotExist_Message => "This chat has no queue with name '<b>{QueueName}</b>'.",
            TextKeys.QueueCommand_PublicChat_QueueEmpty_Message => "Queue '<b>{QueueName}</b>' has no participants.",
            TextKeys.QueueCommand_PublicChat_ListQueueParticipants_Message => "'<b>{QueueName}</b>' has these participants:\n",
            TextKeys.QueueCommand_PublicChat_DisplayQueueParticipant_Message => "{Position}) <b>{UserName}</b>",
            TextKeys.QueueCommand_PublicChat_ListQueues_NoQueues_Message => "This chat has no queues. To create new one write '<b>/createqueue</b> <i>queue_name</i>'.",
            TextKeys.QueueCommand_PublicChat_ListQueues_Message => "This chat has these queues:",
            TextKeys.QueueCommand_PublicChat_DisplayQueue_Message => "• {QueueName}",
            TextKeys.QueueCommand_PublicChat_ListQueues_PostScriptum_Message => "To get info about one of them write '<b>/queue</b> <i>[queue_name]</i>'.",

            TextKeys.EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message => "To be enqueued, please write the command this way: '<b>/enqueue</b> <i>[queue_name] [position(optional)]</i>'.",
            TextKeys.EnqueueCommand_PublicChat_InvalidPositionSpecified_Message => "Please, use positive numbers for user position.",
            TextKeys.EnqueueCommand_PublicChat_QueueDoesNotExist_Message => "There is no queue with name '<b>{QueueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
            TextKeys.EnqueueCommand_PublicChat_UserAlreadyParticipates_Message => "You're already participating in queue '<b>{QueueName}</b>'.",
            TextKeys.EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message => "Queue '<b>{QueueName}</b>' is dynamic - you can enqueue yourself only at the end of queue.",
            TextKeys.EnqueueCommand_PublicChat_PositionIsReserved_Message => "Position '<b>{Position}</b>' in queue '<b>{QueueName}</b>' is reserved. Please, reserve other position.",
            TextKeys.EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message => "Successfully added to queue '<b>{QueueName}</b>' on position <b>{UserPosition}</b>!",

            TextKeys.DequeueCommand_PublicChat_QueueNameIsNotProvided_Message => "Please write the command this way: '/<b>dequeue</b> <i>[queue_name]</i>'.",
            TextKeys.DequeueCommand_PublicChat_QueueDoesNotExist_Message => "There is no queue with name '<b>{QueueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
            TextKeys.DequeueCommand_PublicChat_UserDoesNotParticipate_Message => "You're not participating in queue '<b>{QueueName}</b>'.",
            TextKeys.DequeueCommand_PublicChat_SuccessfullyDequeued_Message => "Successfully removed from queue '<b>{QueueName}</b>'!",

            TextKeys.RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message => "To delete queue, please write command this way: '<b>/removequeue</b> <i>[queue_name]</i>'.",
            TextKeys.RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message => "There is no queue with name '<b>{QueueName}</b>'. You can get list of chat queues using '<b>/queue</b>' command.",
            TextKeys.RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message => "Unable to delete queue '<b>{QueueName}</b>'. It can be deleted only by it's creator or chat administrators.",
            TextKeys.RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message => "Successfully deleted queue '<b>{QueueName}</b>'!",

            TextKeys.UnsupportedCommand_Message => "Bot does not support the '{Command}' command.",
            TextKeys.UnsupportedCommand_PrivateChat_Message => "Bot does not support commands in private chats except '<b>/start</b>'. Please, use interface it provides.",
            _ => throw new Exception()
        };

        return string.Format(formatText, args);
    }
}
