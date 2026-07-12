using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        DashboardSummaryDto GetDashboardSummary();
    }
}
