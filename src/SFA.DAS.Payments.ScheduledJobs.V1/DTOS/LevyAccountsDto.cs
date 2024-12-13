using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Payments.Model.Core.Entities;

namespace SFA.DAS.Payments.ScheduledJobs.V1.DTOS
{
    public class LevyAccountsDto
    {
        public LevyAccountModel DasLevyAccount { get; set; }
        public LevyAccountModel PaymentsLevyAccount { get; set; }
    }
}
