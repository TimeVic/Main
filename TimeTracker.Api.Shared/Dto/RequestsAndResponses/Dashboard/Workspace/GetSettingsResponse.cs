using System.ComponentModel.DataAnnotations;
using Api.Requests.Abstractions;
using TimeTracker.Api.Shared.Dto.Entity;
using TimeTracker.Business.Common.Mvc.Attribute.Validation;

namespace TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Workspace
{
    public class GetSettingsResponse : IResponse
    {
        public virtual WorkspaceSettingsClickUpDto? IntegrationClickUp { get; set; }
        
        public virtual WorkspaceSettingsRedmineDto? IntegrationRedmine { get; set; }
    }
}
