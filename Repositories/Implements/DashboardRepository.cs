using System;
using System.Collections.Generic;
using System.Text;
using DataAccessObjects;
using Repositories.Interfaces;

namespace Repositories.Implements
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DashboardDao _dashboardDao;
        public DashboardRepository()
        {
            _dashboardDao = new DashboardDao();
        }
        public DashboardSummaryDto GetDashboardSummary()
        {
            return _dashboardDao.GetDashboardSummary();
        }
    }
}
