using System.Globalization;

namespace Enqueuer.Persistence.Models;

public class GroupSettings
{
    public long GroupId { get; set; }

    /// <summary>
    /// Group, private chat or supergroup to which these settings belong.
    /// </summary>
    public Group Group { get; set; }

    /// <summary>
    /// The culture info of the group.
    /// </summary>
    public CultureInfo Culture { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string CultureName
    {
        get
        {
            if (Culture != null)
            {
                return Culture.Name;
            }

            return null;
        }
        set
        {
            Culture = new CultureInfo(value);
        }
    }
}
