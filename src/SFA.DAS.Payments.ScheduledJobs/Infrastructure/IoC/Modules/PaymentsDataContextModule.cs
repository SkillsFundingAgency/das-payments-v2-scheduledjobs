﻿using System;
using Autofac;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.Payments.Application.Repositories;
using SFA.DAS.Payments.Core.Configuration;

namespace SFA.DAS.Payments.ScheduledJobs.Infrastructure.IoC.Modules
{
    public class PaymentsDataContextModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register((c, p) =>
                {
                    var configHelper = c.Resolve<IConfigurationHelper>();
                    var dbContextOptions = new DbContextOptionsBuilder<PaymentsDataContext>()
                        .UseSqlServer(configHelper.GetSetting("PaymentsConnectionString"), 
                            optionsBuilder => optionsBuilder.CommandTimeout(270)).Options;
                    return new PaymentsDataContext(dbContextOptions);
                })
                .As<IPaymentsDataContext>()
                .InstancePerLifetimeScope();
        }
    }
}