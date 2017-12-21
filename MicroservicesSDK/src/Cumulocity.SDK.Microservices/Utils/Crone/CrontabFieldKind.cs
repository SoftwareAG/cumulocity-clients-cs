using System;
using System.Collections.Generic;
using System.Text;

namespace Cumulocity.SDK.Microservices.Utils.Crone
{
    [Serializable]
    public enum CrontabFieldKind
    {
        Minute,
        Hour,
        Day,
        Month,
        DayOfWeek
    }
}
