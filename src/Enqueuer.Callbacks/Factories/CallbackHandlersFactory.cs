using System.Collections.Generic;
using Enqueuer.Callbacks.CallbackHandlers;
using Enqueuer.Callbacks.CallbackHandlers.BaseClasses;
using Enqueuer.Data.DataSerialization;
using Enqueuer.Services.Interfaces;

namespace Enqueuer.Callbacks.Factories
{
    public class CallbackHandlersFactory : ICallbackHandlersFactory
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IQueueService _queueService;
        private readonly IUserInQueueService _userInQueueService;
        private readonly IDataSerializer _dataSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CallbackHandlersFactory"/> class.
        /// </summary>
        /// <param name="chatService">Chat service to use.</param>
        /// <param name="userService">User service to use.</param>
        /// <param name="queueService">Queue service to use.</param>
        /// <param name="userInQueueService">User in queue service to use.</param>
        /// <param name="dataSerializer"></param>
        public CallbackHandlersFactory(
            IChatService chatService,
            IUserService userService,
            IQueueService queueService,
            IUserInQueueService userInQueueService,
            IDataSerializer dataSerializer)
        {
            _chatService = chatService;
            _userService = userService;
            _queueService = queueService;
            _userInQueueService = userInQueueService;
            _dataSerializer = dataSerializer;
        }

        public IEnumerable<ICallbackHandler> CreateCallbackHandlers()
        {
            return new ICallbackHandler[]
            {
                new EnqueueMeCallbackHandler(_chatService, _userService, _queueService, _userInQueueService),
                new GetChatCallbackHandler(_chatService, _dataSerializer),
                new GetQueueCallbackHandler(_queueService, _userService, _dataSerializer),
                new ListChatsCallbackHandler(_userService, _dataSerializer),
                new EnqueueCallbackHandler(_queueService, _userInQueueService, _dataSerializer),
                new EnqueueAtCallbackHandler(_userService, _queueService, _userInQueueService, _dataSerializer),
                new DequeueMeCallbackHandler(_userService, _queueService, _userInQueueService, _dataSerializer),
                new RemoveQueueCallbackHandler(_queueService, _dataSerializer),
                new SwitchQueueDynamicHandler(_queueService, _userInQueueService, _dataSerializer),
            };
        }
    }
}
