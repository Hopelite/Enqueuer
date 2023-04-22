namespace Enqueuer.Telegram.Core.Localization;

public static class MessageKeys
{
    public static class StartMessageHandler
    {
        public const string Message_StartCommand_PublicChat_Message = nameof(Message_StartCommand_PublicChat_Message);

        public const string Message_StartCommand_PrivateChat_Message = nameof(Message_StartCommand_PrivateChat_Message);

        public const string Message_StartCommand_PrivateChat_ListChats_Button = nameof(Message_StartCommand_PrivateChat_ListChats_Button);
    }

    public static class HelpMessageHandler
    {
        public const string Message_HelpCommand_Message = nameof(Message_HelpCommand_Message);
    }

    public static class QueueMessageHandler
    {
        public const string Message_QueueCommand_PublicChat_QueueDoesNotExist_Message = nameof(Message_QueueCommand_PublicChat_QueueDoesNotExist_Message);

        public const string Message_QueueCommand_PublicChat_QueueEmpty_Message = nameof(Message_QueueCommand_PublicChat_QueueEmpty_Message);

        public const string Message_QueueCommand_PublicChat_ListQueueParticipants_Message = nameof(Message_QueueCommand_PublicChat_ListQueueParticipants_Message);

        public const string Message_QueueCommand_PublicChat_DisplayQueueParticipant_Message = nameof(Message_QueueCommand_PublicChat_DisplayQueueParticipant_Message);

        public const string Message_QueueCommand_PublicChat_ListQueues_NoQueues_Message = nameof(Message_QueueCommand_PublicChat_ListQueues_NoQueues_Message);

        public const string Message_QueueCommand_PublicChat_ListQueues_Message = nameof(Message_QueueCommand_PublicChat_ListQueues_Message);

        public const string Message_QueueCommand_PublicChat_DisplayQueue_Message = nameof(Message_QueueCommand_PublicChat_DisplayQueue_Message);

        public const string Message_QueueCommand_PublicChat_ListQueues_PostScriptum_Message = nameof(Message_QueueCommand_PublicChat_ListQueues_PostScriptum_Message);
    }

    public static class EnqueueMessageHandler
    {
        public const string Message_EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message = nameof(Message_EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message);

        public const string Message_EnqueueCommand_PublicChat_InvalidPositionSpecified_Message = nameof(Message_EnqueueCommand_PublicChat_InvalidPositionSpecified_Message);

        public const string Message_EnqueueCommand_PublicChat_QueueDoesNotExist_Message = nameof(Message_EnqueueCommand_PublicChat_QueueDoesNotExist_Message);

        public const string Message_EnqueueCommand_PublicChat_UserAlreadyParticipates_Message = nameof(Message_EnqueueCommand_PublicChat_UserAlreadyParticipates_Message);

        public const string Message_EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message = nameof(Message_EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message);

        public const string Message_EnqueueCommand_PublicChat_PositionIsReserved_Message = nameof(Message_EnqueueCommand_PublicChat_PositionIsReserved_Message);

        public const string Message_EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message = nameof(Message_EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message);
    }

    public static class DequeueMessageHandler
    {
        public const string Message_DequeueCommand_PublicChat_QueueNameIsNotProvided_Message = nameof(Message_DequeueCommand_PublicChat_QueueNameIsNotProvided_Message);

        public const string Message_DequeueCommand_PublicChat_QueueDoesNotExist_Message = nameof(Message_DequeueCommand_PublicChat_QueueDoesNotExist_Message);

        public const string Message_DequeueCommand_PublicChat_UserDoesNotParticipate_Message = nameof(Message_DequeueCommand_PublicChat_UserDoesNotParticipate_Message);

        public const string Message_DequeueCommand_PublicChat_SuccessfullyDequeued_Message = nameof(Message_DequeueCommand_PublicChat_SuccessfullyDequeued_Message);
    }

    public static class CreateQueueMessageHandler
    {
        public const string Message_CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message = nameof(Message_CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message);

        public const string Message_CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message = nameof(Message_CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message);

        public const string Message_CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message = nameof(Message_CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message);

        public const string Message_CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message = nameof(Message_CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message);

        public const string Message_CreateQueueCommand_PublicChat_QueueNameIsTooLong_Message = nameof(Message_CreateQueueCommand_PublicChat_QueueNameIsTooLong_Message);

        public const string Message_CreateQueueCommand_PublicChat_QueueAlreadyExists_Message = nameof(Message_CreateQueueCommand_PublicChat_QueueAlreadyExists_Message);

        public const string Message_CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message = nameof(Message_CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message);

        public const string Message_CreateQueueCommand_PublicChat_EnqueueMe_Button = nameof(Message_CreateQueueCommand_PublicChat_EnqueueMe_Button);
    }

    public static class RemoveQueueMessageHandler
    {
        public const string Message_RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message = nameof(Message_RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message);

        public const string Message_RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message = nameof(Message_RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message);

        public const string Message_RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message = nameof(Message_RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message);

        public const string Message_RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message = nameof(Message_RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message);
    }

    public const string Message_UnsupportedCommand_Message = nameof(Message_UnsupportedCommand_Message);

    public const string Message_UnsupportedCommand_PrivateChat_Message = nameof(Message_UnsupportedCommand_PrivateChat_Message);
}
