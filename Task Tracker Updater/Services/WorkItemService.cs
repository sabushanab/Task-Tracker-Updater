using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Task_Tracker_Updater.Services
{
    public class WorkItemService
    {
        private WorkItemTrackingHttpClient _client { get; set; }
        private string[] _fields = new string[3] { "System.Id", "System.Title", "System.IterationId" };

        public WorkItemService(VssConnection connection)
        {
            _client = connection.GetClient<WorkItemTrackingHttpClient>();
        }

        public async Task<List<WorkItem>> GetWorkItemsByWIQL(Wiql query)
        {
            List<int> ids = await GetWorkItemIds(query);
            if (ids.Count == 0)
            {
                return new List<WorkItem>();
            }
            return await _client.GetWorkItemsAsync(ids, _fields);
        }

        private async Task<List<int>> GetWorkItemIds(Wiql query)
        {
            var result = await _client.QueryByWiqlAsync(query);

            List<int> ids = new List<int>();
            foreach (var item in result.WorkItems)
            {
                ids.Add(item.Id);
            }
            return ids;
        }
    }
}
