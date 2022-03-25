# Enqueuer

This is the repository of **Enqueuer** Telegram bot (*@enqueuer_bot*), the master of creating and managing queues. Just add it to your chat and write the */start* command to begin, or write the same command in direct messages with it.

## Command List
All of these commands, except the */start* are available only in group chats - use convenient user interface in direct messages with bot.
*Square brackets mean required paramethers, while curly brackets mean optional.*
- */start* - get introducing message in group chats and begins interaction with the bot in direct messages
- */help* - lists all available commands
- */queue {queue name}* - lists all chat queues or lists all queue participants if entered with a queue name
- */createqueue [queue name]* - creates a queue with specified name
- */enqueue [queue name] {position}* - adds you at the first available position in the queue, or at the specified position, if provided with number
- */dequeue [queue name]* - removes you from the queue
- */removequeue [queue name]* - deletes a queue with specified name

## Support and Feedback
If any error or a bug occures, please open an issue here or message the developer (*@VadimK.ok*). Please share your feedback same way too!
