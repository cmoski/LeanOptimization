﻿using Moq;
using NUnit.Framework;
using Optimization.Batcher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Optimization.Tests
{
    [TestFixture()]
    public class DynastyTests
    {

        [TestCase(true)]
        [TestCase(false)]
        public void OptimizeTest(bool hasSegmentHours)
        {
            var file = new Mock<IFileSystem>();
            var log = new Mock<ILogWrapper>();
            var q = new Queue<string>();

            file.Setup(f => f.File.ReadAllText(It.IsAny<string>())).Returns((string path) =>
            {
                string filename = System.IO.Path.GetFileName(path);
                if (hasSegmentHours && filename == "dynasty.json")
                {
                    filename = filename.Replace(".json", "_segments.json");
                }

                return System.IO.File.ReadAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename.Replace(".json", "_test.json")));
            });

            file.Setup(f => f.File.WriteAllText(It.IsAny<string>(), It.IsAny<string>())).Callback((string path, string contents) =>
            {
                System.IO.File.WriteAllText(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path), contents);
            });

            List<string> actual = new List<string>();

            log.Setup(l => l.Result(It.IsAny<string>())).Callback<string>(m => { actual.Add(m); });

            OptimizerInitializerTests.SetEntryAssembly(Assembly.GetCallingAssembly());

            var unit = new Dynasty(file.Object, log.Object, new MockGeneManagerFactory());

            unit.Optimize();

            Assert.IsTrue(actual.ElementAt(1).StartsWith(GeneManager.Termination));
            Assert.IsTrue(actual.ElementAt(2).StartsWith("Algorithm:"));
            Assert.IsTrue(actual.ElementAt(3).StartsWith("take:"));
            if (hasSegmentHours)
            {
                Assert.IsTrue(actual.First().Equals("For period: 01/02/2016 00:00:00 02/02/2016 00:00:00"));
                Assert.IsTrue(actual.ElementAt(4).Equals("For period: 02/02/2016 00:00:00 04/02/2016 00:00:00"));
                Assert.IsTrue(actual.ElementAt(8).Equals("For period: 04/02/2016 00:00:00 07/02/2016 00:00:00"));
                Assert.IsTrue(actual.ElementAt(12).Equals("For period: 07/02/2016 00:00:00 08/02/2016 00:00:00"));
            }
            else
            {
                Assert.IsTrue(actual.First().Equals("For period: 01/02/2016 00:00:00 11/02/2016 00:00:00"));
                Assert.IsTrue(actual.ElementAt(4).Equals("For period: 11/02/2016 00:00:00 21/02/2016 00:00:00"));
                Assert.IsTrue(actual.ElementAt(8).Equals("For period: 21/02/2016 00:00:00 02/03/2016 00:00:00"));
            }

        }

    }

}