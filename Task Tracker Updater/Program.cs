using CsvHelper;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Task_Tracker_Updater.Services;

namespace Task_Tracker_Updater
{
    class Program
    {
        static async Task Main(string[] args)
        {
            VssConnection connection = new VssConnection(new Uri("https://tfs.opm.gov/tfs/RecruitmentSystems"), new VssClientCredentials());
            WorkItemService workItemService = new WorkItemService(connection);

            if (4 > args.Length)
            {
                Console.WriteLine("You must pass Iteration Path, Project, Start Date, and Target Date");
            }

            TaskTrackerArguments arguments = new TaskTrackerArguments()
            {
                IterationPath = args[0],
                Project = args[1],
                StartDate = args[2],
                TargetDate = args[3]
            };

            Wiql wiql = new Wiql()
            {
                Query = $"Select [Title], [System.IterationId] From WorkItems Where [System.TeamProject] = '{arguments.Project}' " +
                $"And [System.IterationPath] = '{arguments.IterationPath}' And [System.WorkItemType] <> 'Task' And [Required Attendee 1] = '{connection.AuthorizedIdentity.DisplayName}'"
            };


            List<WorkItem> workItems = await workItemService.GetWorkItemsByWIQL(wiql);
            List<CsvWorkItem> csvWorkItems = new List<CsvWorkItem>();
            foreach (WorkItem item in workItems)
            {
                csvWorkItems.Add(new CsvWorkItem()
                {
                    ID = item.Id.HasValue ? item.Id.Value : 0,
                    Title = item.Fields["System.Title"].ToString(),
                    StartDate = arguments.StartDate,
                    TargetDate = arguments.TargetDate
                });
            }
            Directory.CreateDirectory(@"C:\TaskTracker\CSV");
            using (var writer = new StreamWriter(@"C:\TaskTracker\CSV\workitems.csv"))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(csvWorkItems);
            }

        }

        public struct CsvWorkItem
        {
            public int ID { get; set; }
            public string Title { get; set; }
            public string StartDate { get; set; }
            public string TargetDate { get; set; }
            public string EndDate { get; set; }
        }

        public struct TaskTrackerArguments
        {
            public string IterationPath;
            public string Project;
            public string StartDate;
            public string TargetDate;
        }
    }
}
