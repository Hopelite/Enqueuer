using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enqueuer.Messaging.Core.Types.Common;

public enum ChatType
{
    /// <summary>
    /// Normal one to one <see cref="Chat"/>
    /// </summary>
    Private = 1,

    /// <summary>
    /// Normal group chat
    /// </summary>
    Group,

    /// <summary>
    /// A channel
    /// </summary>
    Channel,

    /// <summary>
    /// A supergroup
    /// </summary>
    Supergroup,

    /// <summary>
    /// “sender” for a private chat with the inline query sender
    /// </summary>
    Sender
}
