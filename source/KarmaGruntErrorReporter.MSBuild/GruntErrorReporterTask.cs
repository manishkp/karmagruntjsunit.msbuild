// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KarmaGruntErrorReporterTask.cs" company="">
//   KarmaGruntErrorReporterTask
// </copyright>
// <summary>
//   Build task to convert Resource file to Java script Object Notation file
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KarmaGruntErrorReporter.MSBuild
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using Microsoft.Build.Framework;

    /// <summary>
    /// Build task to convert Resource file to Java script Object Notation file
    /// </summary>
    public class GruntErrorReporterTask : ITask
    {
        /// <summary>
        /// The karma error report file.
        /// </summary>
        private const string TestResultsXml = "test-results.testxml";

        /// <summary>
        /// Gets or sets Build Engine
        /// </summary>
        public IBuildEngine BuildEngine { get; set; }

        /// <summary>
        /// Gets or sets Host Object
        /// </summary>
        public ITaskHost HostObject { get; set; }

        /// <summary>
        /// Gets or sets Project Full Path
        /// </summary>
        [Required]
        public string ProjectFullPath { get; set; }

        /// <summary>
        /// Gets or Sets Grunt Output
        /// </summary>
        [Required]
        public string GruntOutput { get; set; }

        /// <summary>
        /// Executes the Task
        /// </summary>
        /// <returns>True if success</returns>
        public bool Execute()
        {
            var buildFailed = false;
            var testResultXmlFile = Path.Combine(this.ProjectFullPath, TestResultsXml);
            if (!File.Exists(testResultXmlFile))
            {
                 this.BuildEngine.LogMessageEvent(
                    new BuildMessageEventArgs(
                        string.Format(
                            "Test Results file not generated, the tests doesnt seem to have run."),
                        string.Empty,
                        "KarmaGruntUnitTestRunner",
                        MessageImportance.Normal));

                if (!string.IsNullOrEmpty(this.GruntOutput))
                {
                    this.BuildEngine.LogErrorEvent(
                        new BuildErrorEventArgs(
                            "GruntConfigError",
                            string.Empty,
                            "gruntfile.js",
                            0,
                            0,
                            0,
                            0,
                            this.GruntOutput,
                            string.Empty,
                            "Grunt"));

                    buildFailed = true;
                }

                return buildFailed;
            }

            var results = XElement.Load(testResultXmlFile);
            var testResults = results.Descendants("Test").ToList();

            foreach (var testResult in testResults)
            {
                var testName = testResult.Element("Name").Value;
                var outcome = testResult.Element("Outcome");
                if (outcome != null
                    && string.Compare(outcome.Value, "Failed", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var failure = testResult.Element("ErrorStackTrace").Value.Trim();
                    this.BuildEngine.LogErrorEvent(
                        new BuildErrorEventArgs(
                            testName,
                            string.Empty,
                            "gruntfile.js",
                            0,
                            0,
                            0,
                            0,
                            string.Format("'{0}' Failed, Error: '{1}'", testName, failure),
                            string.Empty,
                            "UnitTests"));

                    buildFailed = true;
                }
                else
                {
                    this.BuildEngine.LogMessageEvent(
                        new BuildMessageEventArgs(
                            string.Format("Test {0} : Passed.", testName),
                            string.Empty,
                            "KarmaGruntUnitTestRunner",
                            MessageImportance.High));
                }
            }

            if (testResults.Count <= 0)
            {
               // This may be the case where tests coudnt run, so show the grunt output  
                if (!string.IsNullOrEmpty(this.GruntOutput))
                {
                    this.BuildEngine.LogErrorEvent(
                        new BuildErrorEventArgs(
                            "GruntConfigError",
                            string.Empty,
                            "gruntfile.js",
                            0,
                            0,
                            0,
                            0,
                            this.GruntOutput,
                            string.Empty,
                            "Grunt"));

                    buildFailed = true;
                }
            }
          
            return !buildFailed;
        }
    }
}