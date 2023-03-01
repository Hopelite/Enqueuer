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

            TextKeys.QueueCommand_PublicChat_GetQueue_DoesNotExist_Message => "This chat has no queue with name '<b>{QueueName}</b>'.",
            TextKeys.QueueCommand_PublicChat_GetQueue_QueueEmpty_Message => "Queue '<b>{QueueName}</b>' has no participants.",
            TextKeys.QueueCommand_PublicChat_GetQueue_ListParticipants_Message => "'<b>{QueueName}</b>' has these participants:\n",
            TextKeys.QueueCommand_PublicChat_GetQueue_DisplayQueueParticipant_Message => "{Position}) <b>{UserName}</b>",
            TextKeys.QueueCommand_PublicChat_ListQueues_NoQueues_Message => "This chat has no queues. To create new one write '<b>/createqueue</b> <i>queue_name</i>'.",
            TextKeys.QueueCommand_PublicChat_ListQueues_Message => "This chat has these queues:",
            TextKeys.QueueCommand_PublicChat_DisplayQueue_Message => "• {QueueName}",
            TextKeys.QueueCommand_PublicChat_ListQueues_PostScriptum_Message => "To get info about one of them write '<b>/queue</b> <i>[queue_name]</i>'.",

            TextKeys.UnsupportedCommand_Message => "Bot does not support the '{Command}' command.",
            TextKeys.UnsupportedCommand_PrivateChat_Message => "Bot does not support commands in private chats except '<b>/start</b>'. Please, use interface it provides.",
            _ => throw new Exception()
        };

        return string.Format(formatText, args);
    }
}
