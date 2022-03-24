﻿using Microsoft.Data.SqlClient;
using StatsdClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NServiceBus
{
    public static class CommonEndpointSettings
    {
        public static void ApplyCommonConfiguration(this EndpointConfiguration config) 
        {
            config.AuditProcessedMessagesTo("audit");
            config.SendFailedMessagesTo("error");

            config.UseSerialization<NewtonsoftSerializer>();
            config.UseTransport<LearningTransport>();

            var messageConventions = config.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));

            config.SendHeartbeatTo(
                serviceControlQueue: "Particular.ServiceControl",
                frequency: TimeSpan.FromSeconds(10),
                timeToLive: TimeSpan.FromSeconds(5));

            var metrics = config.EnableMetrics();
            metrics.SendMetricDataToServiceControl(
                serviceControlMetricsAddress: "Particular.Monitoring",
                interval: TimeSpan.FromSeconds(5));
        }

        public static void ApplyCommonConfigurationWithPersistence(this EndpointConfiguration config, string sqlPersistenceConnectionString)
        {
            ApplyCommonConfiguration(config);

            config.EnableInstallers();

            var persistence = config.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(sqlPersistenceConnectionString));

            config.EnableOutbox();
        }
    }
}
