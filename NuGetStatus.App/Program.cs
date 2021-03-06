﻿using NuGetStatus.Library;
using NuGetTools.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NuGetStatus.App
{
    class Program
    {
        private const string MARKER = "================================================================================================================";
        private const string VALIDATE_VSIX = "Validate Vsix Localization";
        private const string VALIDATE_REPO = "Validate Repository Artifacts Localization";

        static void Main(string[] args)
        {
            var project = new Project()
            {
                Id = EnvVars.DevDivProjectGuid,
                Name = Constants.DevDiv
            };

            var logger = new Logger();
            var definitionId = EnvVars.NuGetOfficialBuildDefinitionId;
            var definition = VSTSUtil.GetBuildDefintionAsync(project, Int32.Parse(definitionId), logger).Result;
            var latestBuild = VSTSUtil.GetLatestBuildAsync(definition, logger).Result;

            if (latestBuild != null)
            {
                latestBuild.PopulateTimeLine(logger).Wait();
            }

            var validations = latestBuild.TimelineRecords.Where(r => r.Name == VALIDATE_VSIX || r.Name == VALIDATE_REPO);
            var summaries = new List<LocValidationSummary>();

            foreach (var validation in validations)
            {
                var summary = GetLocalizationSummary(validation.Log.GetLogContentAsync(logger).Result);

                if (!string.IsNullOrEmpty(summary))
                {
                    summaries.Add(new LocValidationSummary() { Summary = summary, Title = validation.Name });
                }
            }
        }

        static string GetLocalizationSummary(string log)
        {
            var lines = log.Split('\n');
            var summary = new StringBuilder();
            var logging = false;

            foreach (var line in lines)
            {
                if (logging)
                {
                    summary.AppendLine(line);
                }
                else
                {
                    if (line.Contains(MARKER))
                    {
                        summary.AppendLine(line);
                        logging = true;
                    }
                }
            }

            return summary.ToString();
        }
    }
}
