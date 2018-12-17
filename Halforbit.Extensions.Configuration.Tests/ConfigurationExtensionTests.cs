using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Halforbit.Extensions.Configuration.Tests
{
    public class ConfigurationExtensionTests
    {
        [Fact]
        [Trait("Type", "Unit")]
        public void TestToObject()
        {
            // ARRANGE ////////////////////////////////////////////////////////

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("dispatcher.config.json")
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["steve:name"] = "Steve",

                    ["steve:age"] = "25"
                })
                .Build();

            var expectedDispatcherConfig = JsonConvert
                .DeserializeObject<JObject>(File.ReadAllText("dispatcher.config.json"))["dispatcher"]
                .ToObject<DispatcherConfig>();

            // ACT ////////////////////////////////////////////////////////////

            var actualDispatcherConfig = configuration
                .GetSection("dispatcher")
                .ToObject<DispatcherConfig>();

            var actualSteve = configuration.GetSection("steve").ToObject<JObject>();

            // ASSERT /////////////////////////////////////////////////////////

            Assert.Equal(
                JsonConvert.SerializeObject(expectedDispatcherConfig),
                JsonConvert.SerializeObject(actualDispatcherConfig));

            Assert.Equal("Steve", actualSteve["name"]);

            Assert.Equal("25", actualSteve["age"]);
        }

        // MODEL //////////////////////////////////////////////////////////////

        public class DispatcherConfig
        {
            public DispatcherConfig(
                IReadOnlyList<JobConfig> jobs)
            {
                Jobs = jobs;
            }

            public IReadOnlyList<JobConfig> Jobs { get; }
        }

        public class JobConfig
        {
            public JobConfig(
                string name,
                TimeSpan estAvgHandleTime,
                IReadOnlyList<QueueConfig> queues)
            {
                Name = name;

                EstAvgHandleTime = estAvgHandleTime;

                Queues = queues;
            }

            public string Name { get; }

            public TimeSpan EstAvgHandleTime { get; }

            public IReadOnlyList<QueueConfig> Queues { get; }
        }

        public class QueueConfig
        {
            public QueueConfig(
                string name,
                TimeSpan targetTimeToAnswer)
            {
                Name = name;

                TargetTimeToAnswer = targetTimeToAnswer;
            }

            public string Name { get; }

            public TimeSpan TargetTimeToAnswer { get; }
        }
    }
}
