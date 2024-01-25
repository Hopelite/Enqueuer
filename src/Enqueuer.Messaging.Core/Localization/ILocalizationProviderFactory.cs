using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enqueuer.Messaging.Core.Localization;

public interface ILocalizationProviderFactory
{
    ILocalizationProvider Create();
}
