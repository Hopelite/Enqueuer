namespace Enqueuer.Core.TextProviders;

public static class MessageKeys
{
    public static class StartMessageHandler
    {
        public const string StartCommand_PublicChat_Message = "StartCommand_PublicChat_Message";

        public const string StartCommand_PrivateChat_Message = "StartCommand_PrivateChat_Message";

        public const string StartCommand_PrivateChat_ListChatsButton = "StartCommand_PrivateChat_ListChatsButton";
    }

    public static class HelpMessageHandler
    {
        public const string HelpCommand_Message = "HelpCommand_Message";
    }

    public static class QueueMessageHandler
    {
        public const string QueueCommand_PublicChat_QueueDoesNotExist_Message = "QueueCommand_PublicChat_QueueDoesNotExist_Message";

        public const string QueueCommand_PublicChat_QueueEmpty_Message = "QueueCommand_PublicChat_QueueEmpty_Message";

        public const string QueueCommand_PublicChat_ListQueueParticipants_Message = "QueueCommand_PublicChat_ListQueueParticipants_Message";

        public const string QueueCommand_PublicChat_DisplayQueueParticipant_Message = "QueueCommand_PublicChat_DisplayQueueParticipant_Message";

        public const string QueueCommand_PublicChat_ListQueues_NoQueues_Message = "QueueCommand_PublicChat_ListQueues_NoQueues_Message";

        public const string QueueCommand_PublicChat_ListQueues_Message = "QueueCommand_PublicChat_ListQueues_Message";

        public const string QueueCommand_PublicChat_DisplayQueue_Message = "QueueCommand_PublicChat_DisplayQueue_Message";

        public const string QueueCommand_PublicChat_ListQueues_PostScriptum_Message = "QueueCommand_PublicChat_ListQueues_PostScriptum_Message";
    }

    public static class EnqueueMessageHandler
    {
        public const string EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message = "EnqueueCommand_PublicChat_QueueNameIsNotProvided_Message";

        public const string EnqueueCommand_PublicChat_InvalidPositionSpecified_Message = "EnqueueCommand_PublicChat_InvalidPositionSpecified_Message";

        public const string EnqueueCommand_PublicChat_QueueDoesNotExist_Message = "EnqueueCommand_PublicChat_QueueDoesNotExist_Message";

        public const string EnqueueCommand_PublicChat_UserAlreadyParticipates_Message = "EnqueueCommand_PublicChat_UserAlreadyParticipates_Message";

        public const string EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message = "EnqueueCommand_PublicChat_PositionSpecified_DynamicQueue_Message";

        public const string EnqueueCommand_PublicChat_PositionIsReserved_Message = "EnqueueCommand_PublicChat_PositionIsReserved_Message";

        public const string EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message = "EnqueueCommand_PublicChat_SuccessfullyAddedOnPosition_Message";
    }

    public static class DequeueMessageHandler
    {
        public const string DequeueCommand_PublicChat_QueueNameIsNotProvided_Message = "DequeueCommand_PublicChat_QueueNameIsNotProvided_Message";

        public const string DequeueCommand_PublicChat_QueueDoesNotExist_Message = "DequeueCommand_PublicChat_QueueDoesNotExist_Message";

        public const string DequeueCommand_PublicChat_UserDoesNotParticipate_Message = "DequeueCommand_PublicChat_UserDoesNotParticipate_Message";

        public const string DequeueCommand_PublicChat_SuccessfullyDequeued_Message = "DequeueCommand_PublicChat_SuccessfullyDequeued_Message";
    }

    public static class CreateQueueMessageHandler
    {
        public const string CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message = "CreateQueueCommand_PublicChat_QueueNameIsNotProvided_Message";

        public const string CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message = "CreateQueueCommand_PublicChat_MaxNumberOfQueuesReached_Message";

        public const string CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message = "CreateQueueCommand_PublicChat_NumberAtTheEndOfQueueName_Message";

        public const string CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message = "CreateQueueCommand_PublicChat_OnlyNumberInQueueName_Message";

        public const string CreateQueueCommand_PublicChat_QueueNameTooLong_Message = "CreateQueueCommand_PublicChat_QueueNameTooLong_Message";

        public const string CreateQueueCommand_PublicChat_QueueAlreadyExists_Message = "CreateQueueCommand_PublicChat_QueueAlreadyExists_Message";

        public const string CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message = "CreateQueueCommand_PublicChat_SuccessfullyCreatedQueue_Message";

        public const string CreateQueueCommand_PublicChat_EnqueueMeButton = "CreateQueueCommand_PublicChat_EnqueueMeButton";
    }

    public static class RemoveQueueMessageHandler
    {
        public const string RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message = "RemoveQueueCommand_PublicChat_QueueNameIsNotProvided_Message";

        public const string RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message = "RemoveQueueCommand_PublicChat_QueueDoesNotExist_Message";

        public const string RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message = "RemoveQueueCommand_PublicChat_UserHasNoRightToDelete_Message";

        public const string RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message = "RemoveQueueCommand_PublicChat_SuccessfullyRemovedQueue_Message";
    }

    public const string UnsupportedCommand_Message = "UnsupportedCommand_Message";

    public const string UnsupportedCommand_PrivateChat_Message = "UnsupportedCommand_PrivateChat_Message";
}
