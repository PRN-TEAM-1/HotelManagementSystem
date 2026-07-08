using System;
using System.Collections.Generic;
using System.Text;
using Repositories.Interfaces;
using Services.Interfaces;

namespace Services.Implements
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public DashboardSummaryDto GetDashboardSummary()
        {
            return _dashboardRepository.GetDashboardSummary();
        }
    }
}
