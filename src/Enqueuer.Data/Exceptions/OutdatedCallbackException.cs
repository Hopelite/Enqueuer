using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enqueuer.Callbacks.Exceptions;

public class OutdatedCallbackException : Exception
{
    public OutdatedCallbackException()
    {
    }

    public OutdatedCallbackException(string message)
        : base(message)
    {
    }

    public OutdatedCallbackException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
