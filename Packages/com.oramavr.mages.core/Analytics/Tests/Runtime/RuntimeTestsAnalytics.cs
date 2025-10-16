namespace MAGES.Tests
{
    using System.Collections.Generic;
    using MAGES;
    using MAGES.Analytics;
    using MAGES.DataContainer;
    using MAGES.SceneGraph;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Class for testing runtime analytics.
    /// </summary>
    public class RuntimeTestsAnalytics
    {
        /// <summary>
        /// Test for analytics module initialization.
        /// </summary>
        [Test]
        public void TestAnalyticsInitialization()
        {
            var go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = hub?.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for Query AttachData function.
        /// </summary>
        [Test]
        public void TestEventAttachData()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test1.action.*", typeof(ListSchema));
            dataContainer.SetSchema("test1.event.*", typeof(EventSchema));

            dataContainer.StoreData($"test1.action.list", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test1.action.list", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test1.action.list", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test1.action.list", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test1.action.list", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test1.action.list", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics1.txt";
            Hub.Instance.Get<AnalyticsModule>().AddEvent<MAGES.Analytics.Event>("TestEvent1")
                .SetTag("Event")
                .SetPrefix("test1.event")
                .AttachData("Station", "CPR")
                .AttachData("Heartrate", 10)
                .AttachData("CongintiveLoad", 10.4f);

            Assert.AreEqual(dataContainer.GetData("test1.event.TestEvent1.Station"), "CPR");
            Assert.AreEqual(dataContainer.GetData("test1.event.TestEvent1.Heartrate"), 10);
            Assert.AreEqual(dataContainer.GetData("test1.event.TestEvent1.CongintiveLoad"), 10.4f);

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for time limit default processor.
        /// </summary>
        [Test]
        public void TestTimeLimitProcessor()
        {
            GameObject go = new GameObject("__test_go__");
            ListIterator listIterator = new();

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test2.action.*", typeof(ListSchema));

            dataContainer.StoreData($"test2.action.list1", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test2.action.list2", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test2.action.list3", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test2.action.list4", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test2.action.list5", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test2.action.list6", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 59.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics2.txt";

            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("TimeLimit")
                .SetIterator(listIterator)
                .SetTag("Query")
                .SetFilter("test2.action", new List<string> { "list1" })
                .SetProcessor(DefaultProcessors.ProcessorTimeLimit)
                .SetTarget(8.0f)
                .SetArguments(arguments: new List<object> { "Action1", "Upper bound" })
                .SetMessage("Action1 completion time is more than 8 seconds.")
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, 12);

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for average default processor.
        /// </summary>
        [Test]
        public void TestAverageProcessor()
        {
            GameObject go = new GameObject("__test_go__");
            ListIterator listIterator = new();

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test3.action.*", typeof(ListSchema));
            dataContainer.SetSchema("test3.event.*", typeof(EventSchema));

            dataContainer.StoreData($"test3.action.list", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test3.action.list", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test3.action.list", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test3.action.list", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test3.action.list", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test3.action.list", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics3.txt";
            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("Average")
                .SetIterator(listIterator)
                .SetTag("Query")
                .SetFilter("test3.action", new List<string> { "*" }, "BeginTime")
                .SetProcessor(DefaultProcessors.ProcessorAvg)
                .SetTarget(8.0f)
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "12.33333");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for max default processor.
        /// </summary>
        [Test]
        public void TestMaxProcessor()
        {
            GameObject go = new GameObject("__test_go__");
            ListIterator listIterator = new();

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test4.action.*", typeof(ListSchema));

            dataContainer.StoreData($"test4.action.list", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test4.action.list", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test4.action.list", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test4.action.list", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test4.action.list", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test4.action.list", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics4.txt";
            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("Max")
                .SetIterator(listIterator)
                .SetTag("Query")
                .SetFilter("test4.action", new List<string> { "*" }, "EndTime")
                .SetProcessor(DefaultProcessors.ProcessorMax)
                .SetTarget(8.0f)
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "54");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for min default processor.
        /// </summary>
        [Test]
        public void TestMinProcessor()
        {
            GameObject go = new GameObject("__test_go__");
            ListIterator listIterator = new();

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test5.action.*", typeof(ListSchema));

            dataContainer.StoreData($"test5.action.list", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test5.action.list", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test5.action.list", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test5.action.list", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test5.action.list", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test5.action.list", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics5.txt";
            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("Min")
                .SetIterator(listIterator)
                .SetTag("Query")
                .SetFilter("test5.action", new List<string> { "*" }, "EndTime")
                .SetProcessor(DefaultProcessors.ProcessorMin)
                .SetTarget(8.0f)
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "5");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for sum default processor.
        /// </summary>
        [Test]
        public void TestSumProcessor()
        {
            GameObject go = new GameObject("__test_go__");
            ListIterator listIterator = new();

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test6.action.*", typeof(ListSchema));

            dataContainer.StoreData($"test6.action.list", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test6.action.list", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test6.action.list", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test6.action.list", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test6.action.list", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test6.action.list", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics6.txt";
            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("Sum")
                .SetIterator(listIterator)
                .SetTag("Query")
                .SetFilter("test6.action", new List<string> { "*" }, "EndTime")
                .SetProcessor(DefaultProcessors.ProcessorSum)
                .SetTarget(8.0f)
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "154");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for median default processor.
        /// </summary>
        [Test]
        public void TestMedianProcessor()
        {
            GameObject go = new GameObject("__test_go__");
            ListIterator listIterator = new();

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test7.action.*", typeof(ListSchema));

            dataContainer.StoreData($"test7.action.list", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test7.action.list", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test7.action.list", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test7.action.list", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test7.action.list", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test7.action.list", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics7.txt";
            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("Median")
                .SetIterator(listIterator)
                .SetTag("Query")
                .SetFilter("test7.action", new List<string> { "*" }, "EndTime")
                .SetProcessor(DefaultProcessors.ProcessorMedian)
                .SetTarget(8.0f)
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "20");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for the PerformSubActionProcessor.
        /// </summary>
        [Test]
        public void TestPerformSubActionProcessor()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test8.action.*", typeof(ListSchema));
            dataContainer.SetSchema("test8.event.*", typeof(EventSchema));

            dataContainer.StoreData($"test8.action.list1", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test8.action.list2", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test8.action.list3", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test8.action.list4", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f, ActiveActions = new List<string> { "Action4", "Action5" } });
            dataContainer.StoreData($"test8.action.list5", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f, ActiveActions = new List<string> { "Action4", "Action5" } });
            dataContainer.StoreData($"test8.action.list6", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f, ActiveActions = new List<string> { "Action6" } });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics8.txt";

            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("SubAction")
                .SetIterator(DefaultIterators.ListIterator)
                .SetTag("Query")
                .SetFilter("test8.action", new List<string> { "list5" })
                .SetProcessor(DefaultProcessors.ProcessorPerformedWrongAction)
                .SetTarget("Action4")
                .SetArguments(new List<object> { "Action5" })
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "Action5, ");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for the NotPerformSubActionProcessor.
        /// </summary>
        [Test]
        public void TestNotPerformSubActionProcessor()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test9.action.*", typeof(ListSchema));

            dataContainer.StoreData($"test9.action.list1", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"test9.action.list2", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"test9.action.list3", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"test9.action.list4", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"test9.action.list5", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"test9.action.list6", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics9.txt";

            Query ret = Hub.Instance.Get<AnalyticsModule>().AddQuery<Query>("SubAction")
                .SetIterator(DefaultIterators.ListIterator)
                .SetTag("Query")
                .SetFilter("test9.action", new List<string> { "*" })
                .SetProcessor(DefaultProcessors.ProcessorDidNotPerformRequiredAction)
                .SetTarget("Action4")
                .SetArguments(new List<object> { "Action2" })
                .Process();

            Assert.AreEqual(ret.GeneratedResult, true);
            Assert.AreEqual(ret.Payload, "Action5, ");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for the objective functionality.
        /// </summary>
        [Test]
        public void TestObjective()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test10.action.*", typeof(ListSchema));
            dataContainer.SetSchema("test10.objectives.*", typeof(EventSchema));

            dataContainer.StoreData($"test10.action.list1", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test10.action.list2", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test10.action.list3", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test10.action.list4", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f, ActiveActions = new List<string> { "Action4", "Action5" } });
            dataContainer.StoreData($"test10.action.list5", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f, ActiveActions = new List<string> { "Action4", "Action5" } });
            dataContainer.StoreData($"test10.action.list6", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f, ActiveActions = new List<string> { "Action6" } });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics10.txt";
            MAGESObjective objective = new("Objective1");

            // Create objective specification of type DoNotPerformAction.
            MAGESObjective.Specification specification = new()
            {
                FilterPrefix = "test10.action",
                FilterScope = new List<string> { "list4" },
                FilterField = string.Empty,
                ProcessorTarget = "Action1",
                ProcessorArguments = new List<object> { "Action4" },
                Description = "Do not perform Action 4 under Action1.",
                Processor = DefaultProcessors.ProcessorPerformedWrongAction,
                Iterator = DefaultIterators.ListIterator,
            };

            objective.ProcessObjective(specification);

            bool succeeded = (bool)dataContainer.GetData("mages.analytics.objectives.Objective1.Succeeded");
            string description = (string)dataContainer.GetData("mages.analytics.objectives.Objective1.Description");

            Assert.AreEqual(succeeded, true);
            Assert.AreEqual(description, "Do not perform Action 4 under Action1.");

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for the error functionality.
        /// </summary>
        [Test]
        public void TestError()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.SetSchema("test11.action.*", typeof(ListSchema));
            dataContainer.SetSchema("test11.event.*", typeof(EventSchema));

            dataContainer.StoreData($"test11.action.list1", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test11.action.list2", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test11.action.list3", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f, ActiveActions = new List<string> { "Action1", "Action2", "Action3" } });
            dataContainer.StoreData($"test11.action.list4", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f, ActiveActions = new List<string> { "Action4", "Action5" } });
            dataContainer.StoreData($"test11.action.list5", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f, ActiveActions = new List<string> { "Action4", "Action5" } });
            dataContainer.StoreData($"test11.action.list6", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f, ActiveActions = new List<string> { "Action6" } });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics11.txt";

            MAGESAnalyticsEntry entry = new("ActionName2", 50);

            // Create error specification of type PerformedWrongAction.
            MAGESAnalyticsEntry.Specification specification = new()
            {
                FilterPrefix = "test11.action",
                FilterScope = new List<string> { "list2" },
                BaseData = new AnalyticsTypes.BaseType("PerformAction2UnderAction1", string.Empty, string.Empty, AnalyticsTypes.Type.Error) { Penalty = 20 },
                ProcessorTarget = "Action1",
                ProcessorArguments = new List<object> { "Action2" },
                Processor = DefaultProcessors.ProcessorPerformedWrongAction,
                Iterator = DefaultIterators.ListIterator,
                KeyValuePairs = new(),
            };

            entry.ProcessError(specification);

            object penalty = dataContainer.GetData("mages.analytics.errors.ActionName2(1).PerformAction2UnderAction1(1).Penalty(%)");
            string name = (string)dataContainer.GetData("mages.analytics.errors.ActionName2(1).PerformAction2UnderAction1(1).Action");
            object init_score = dataContainer.GetData("mages.analytics.score.ActionName2(1).initial");
            object final_score = dataContainer.GetData("mages.analytics.score.ActionName2(1).final");

            Assert.AreEqual(penalty, 20);
            Assert.AreEqual(name, "ActionName2");
            Assert.AreEqual(init_score, 50);
            Assert.AreEqual(final_score, 40);

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for the collision processor functionality.
        /// </summary>
        [Test]
        public void TestCollisionProcessor()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            AnalyticsModule analytics = Hub.Instance.Get<AnalyticsModule>();
            Assert.IsNotNull(analytics);

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            dataContainer.StoreData($"mages.analytics.actions.list1", new ActionRecord { Name = "Action1", BeginTime = 0.0f, EndTime = 12.0f });
            dataContainer.StoreData($"mages.analytics.actions.list2", new ActionRecord { Name = "Action2", BeginTime = 0.0f, EndTime = 5.0f });
            dataContainer.StoreData($"mages.analytics.actions.list3", new ActionRecord { Name = "Action3", BeginTime = 0.0f, EndTime = 8.0f });
            dataContainer.StoreData($"mages.analytics.actions.list4", new ActionRecord { Name = "Action4", BeginTime = 13.0f, EndTime = 47.0f });
            dataContainer.StoreData($"mages.analytics.actions.list5", new ActionRecord { Name = "Action5", BeginTime = 13.0f, EndTime = 28.0f });
            dataContainer.StoreData($"mages.analytics.actions.list6", new ActionRecord { Name = "Action6", BeginTime = 48.0f, EndTime = 54.0f });

            dataContainer.StoreData(
                    $"mages.analytics.collisions.enter.Scalpel.Cauterizer",
                    new CollisionRecord { Name = "Scalpel", OtherName = "Cauterizer", ActionNames = new List<string>() { "Action1" }, Timestamp = 0.0f });
            dataContainer.StoreData(
                    $"mages.analytics.collisions.enter.Scalpel.Leg",
                    new CollisionRecord { Name = "Scalpel", OtherName = "Leg", ActionNames = new List<string>() { "Action1" }, Timestamp = 0.0f });
            dataContainer.StoreData(
                    $"mages.analytics.collisions.exit.Cauterizer.Leg",
                    new CollisionRecord { Name = "Cauterizer", OtherName = "Leg", ActionNames = new List<string>() { "Action2" }, Timestamp = 0.0f });

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics12.txt";

            MAGESAnalyticsEntry entry = new("ActionName", 50);

            // Create error specification of type Collision bwtween Scalpel and Cauterizer.
            MAGESAnalyticsEntry.Specification specification = new()
            {
                FilterPrefix = "mages.analytics.collisions",
                FilterScope = new List<string> { "enter.Scalpel.Cauterizer" },
                BaseData = new AnalyticsTypes.BaseType("CollisionErrorScalpCaut", string.Empty, string.Empty, AnalyticsTypes.Type.Error),
                ProcessorTarget = "Cauterizer",
                ProcessorArguments = new List<object> { "Action1", "Action5" },
                Iterator = DefaultIterators.ListIterator,
                Processor = DefaultProcessors.ProcessorCollision,
                KeyValuePairs = new(),
            };

            entry.ProcessError(specification);

            // Create error specification of type Collision bwtween Scalpel and Leg.
            specification = new()
            {
                FilterPrefix = "mages.analytics.collisions",
                FilterScope = new List<string> { "exit.Cauterizer.Leg" },
                BaseData = new AnalyticsTypes.BaseType("CollisionErrorCautLeg", string.Empty, string.Empty, AnalyticsTypes.Type.Error),
                ProcessorTarget = "Leg",
                ProcessorArguments = new List<object> { "Action1", "Action5" },
                Processor = DefaultProcessors.ProcessorCollision,
                Iterator = DefaultIterators.ListIterator,
                KeyValuePairs = new(),
            };

            entry.ProcessError(specification);

            object penalty = dataContainer.GetData("mages.analytics.errors.ActionName(1).CollisionErrorScalpCaut(1).Penalty(%)");
            string name = (string)dataContainer.GetData("mages.analytics.errors.ActionName(1).CollisionErrorScalpCaut(1).Action");
            object init_score = dataContainer.GetData("mages.analytics.score.ActionName(1).initial");
            object final_score = dataContainer.GetData("mages.analytics.score.ActionName(1).final");

            Debug.Log($"--> final_score: {final_score}");

            Assert.AreEqual(penalty, 50);
            Assert.AreEqual(name, "ActionName");
            Assert.AreEqual(init_score, 50);
            Assert.AreEqual(final_score, 25);

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Test for rotation limit error.
        /// </summary>
        [Test]
        public void TestRotationLimit()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            // Create rotation restriction error.
            AnalyticsTypes.RotationRestriction rotationRestriction = new AnalyticsTypes.RotationRestriction("RotationError", typeof(AnalyticsTypes.RotationRestriction).AssemblyQualifiedName, string.Empty, AnalyticsTypes.Type.Error)
            {
                TargetPrefab = new GameObject("Target"),
                MaxRotation = new Vector3(0, 90, 0),
                Name = "RotationError",
                Penalty = 20,
                Info = "Rotation error",
                TypeOfError = MAGESAnalyticsEntry.ErrorType.Procedural.ToString(),
            };

            // Set the rotation of the target prefab to a value that will trigger the error.
            rotationRestriction.TargetPrefab.transform.rotation = Quaternion.Euler(0, 100, 0);

            // Store the transform data of the target prefab.
            TransformRecord transformData = new TransformRecord
            {
                Name = "Target(Clone)",
                Position = rotationRestriction.TargetPrefab.transform.position,
                Rotation = rotationRestriction.TargetPrefab.transform.rotation,
                ActionNames = new List<string> { "Action1" },
                Timestamp = 0.0f,
                DistanceCovered = 0.0f,
                DistanceToCompare = 0.0f,
            };

            dataContainer.StoreData($"mages.analytics.transforms." + $"{rotationRestriction.TargetPrefab.gameObject.name}(Clone)", transformData);

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics13.txt";

            List<string> actions = new() { "Action1" };

            Dictionary<string, MAGESAnalyticsEntry> analytics = new()
        {
            { "Action1", new MAGESAnalyticsEntry("Action1", 100) },
        };

            string actionName = "Action1";
            string prefix = "mages.analytics.transforms";

            List<string> scope = new() { $"{rotationRestriction.TargetPrefab.gameObject.name}(Clone)" };

            object target = rotationRestriction.MaxRotation;

            List<object> arguments = new() { actionName };
            Processor processor = DefaultProcessors.ProcessorRotationLimit;

            if (actions.Contains(actionName) && analytics.ContainsKey(actionName))
            {
                // Create error specification of type RotationRestriction.
                MAGESAnalyticsEntry.Specification specification = new()
                {
                    FilterPrefix = prefix,
                    FilterScope = scope,
                    BaseData = rotationRestriction,
                    ProcessorTarget = target,
                    ProcessorArguments = arguments,
                    Processor = processor,
                    Iterator = DefaultIterators.EventIterator,
                    KeyValuePairs = new(),
                };

                analytics[actionName].ProcessError(specification, () => { Debug.Log("--> " + rotationRestriction.Info); }, false);

                object penalty = dataContainer.GetData("mages.analytics.errors.Action1(1).RotationError(1).Penalty(%)");
                object init_score = dataContainer.GetData("mages.analytics.score.Action1(1).initial");
                object final_score = dataContainer.GetData("mages.analytics.score.Action1(1).final");

                Assert.AreEqual(penalty, 20);
                Assert.AreEqual(init_score, 100);
                Assert.AreEqual(final_score, 80);

                GameObject.Destroy(go);
            }
        }

        /// <summary>
        /// Test for range of motion error.
        /// </summary>
        [Test]
        public void TestRangeOfMotion()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            AnalyticsLogger.Enable = false;

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);

            // Create range of motion error.
            AnalyticsTypes.RangeOfMotionLimit rangeOfMotionLimit = new AnalyticsTypes.RangeOfMotionLimit("ROML", typeof(AnalyticsTypes.RangeOfMotionLimit).AssemblyQualifiedName, string.Empty, AnalyticsTypes.Type.Error)
            {
                TargetPrefab = new GameObject("Target2"),
                MaxDistance = 1.0f,
                Name = "ROML",
                Penalty = 20,
                Info = "Range of motion error",
                TypeOfError = MAGESAnalyticsEntry.ErrorType.Procedural.ToString(),
            };

            string prefix = "mages.analytics.transforms";
            Dictionary<string, MAGESAnalyticsEntry> analytics = new()
        {
            { "Action2", new MAGESAnalyticsEntry("Action2", 100) },
        };

            List<string> actions = new() { "Action2" };
            string actionName = "Action2";
            List<string> scope = new() { $"{rangeOfMotionLimit.TargetPrefab.gameObject.name}(Clone)" };
            object target = rangeOfMotionLimit.MaxDistance;
            List<object> arguments = new() { actionName };
            Processor processor = DefaultProcessors.ProcessorRangeOfMotion;

            // Set the position of the target prefab to a value that will trigger the error.
            TransformRecord transformData = new TransformRecord
            {
                Name = "Target2(Clone)",
                Position = rangeOfMotionLimit.TargetPrefab.transform.position,
                Rotation = rangeOfMotionLimit.TargetPrefab.transform.rotation,
                ActionNames = new List<string> { "Action2" },
                Timestamp = 0.0f,
                DistanceCovered = 10.0f,
                DistanceToCompare = 0.0f,
            };

            dataContainer.StoreData($"mages.analytics.transforms." + $"{rangeOfMotionLimit.TargetPrefab.gameObject.name}(Clone)", transformData);

            Hub.Instance.Get<AnalyticsModule>().ExportPath = "./exportedAnalytics14.txt";

            if (actions.Contains(actionName) && analytics.ContainsKey(actionName))
            {
                // Create error specification of type RangeOfMotionLimit.
                MAGESAnalyticsEntry.Specification specification = new()
                {
                    FilterPrefix = prefix,
                    FilterScope = scope,
                    BaseData = rangeOfMotionLimit,
                    ProcessorTarget = target,
                    ProcessorArguments = arguments,
                    Processor = processor,
                    Iterator = DefaultIterators.EventIterator,
                    KeyValuePairs = new(),
                };

                analytics[actionName].ProcessError(specification, () => { Debug.Log("--> " + rangeOfMotionLimit.Info); }, false);

                object penalty = dataContainer.GetData("mages.analytics.errors.Action2(1).ROML(1).Penalty(%)");
                object init_score = dataContainer.GetData("mages.analytics.score.Action2(1).initial");
                object final_score = dataContainer.GetData("mages.analytics.score.Action2(1).final");

                Assert.AreEqual(penalty, 20);
                Assert.AreEqual(init_score, 100);
                Assert.AreEqual(final_score, 80);

                GameObject.Destroy(go);
            }
        }

        private Bundle BuildStubBundle()
        {
            DataContainerModule dataContainerModule = ScriptableObject.CreateInstance<MAGESDataContainer>();
            InteractionSystemModule interactionSystemModule = ScriptableObject.CreateInstance<StubInteractionSystem>();
            NetworkingModule networkingModule = ScriptableObject.CreateInstance<StubNetworking>();
            AnalyticsModule analyticsModule = ScriptableObject.CreateInstance<MAGESAnalytics>();
            DeviceManagerModule deviceManagerModule = ScriptableObject.CreateInstance<StubDeviceManager>();
            SceneGraphModule sceneGraphModule = ScriptableObject.CreateInstance<StubSceneGraph>();

            Bundle bundle = ScriptableObject.CreateInstance<Bundle>();
            bundle.DataContainer = dataContainerModule;
            bundle.InteractionSystem = interactionSystemModule;
            bundle.Networking = networkingModule;
            bundle.DeviceManager = deviceManagerModule;
            bundle.SceneGraph = sceneGraphModule;
            bundle.Analytics = analyticsModule;

            return bundle;
        }
    }
}