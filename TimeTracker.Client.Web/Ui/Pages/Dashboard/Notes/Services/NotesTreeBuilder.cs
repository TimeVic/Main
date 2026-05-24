using Microsoft.Extensions.Logging;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Models;

namespace TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Services;

public static class NotesTreeBuilder
{
    public static IReadOnlyList<NoteTreeNodeModel> BuildNotesTree(
        IEnumerable<NoteTreeNodeDto> flatNodes,
        ILogger? logger = null
    )
    {
        var nodesById = flatNodes
            .GroupBy(item => item.Id)
            .ToDictionary(
                item => item.Key,
                item => new NoteTreeNodeModel { Node = item.First() }
            );

        var rootNodes = new List<NoteTreeNodeModel>();
        var childrenByParent = new Dictionary<Guid, List<NoteTreeNodeModel>>();
        foreach (var treeNode in nodesById.Values)
        {
            var parentId = treeNode.Node.ParentId;
            if (parentId.HasValue && !nodesById.ContainsKey(parentId.Value))
            {
                logger?.LogWarning(
                    "Note node {NoteId} has missing parent {ParentId}; showing it at root.",
                    treeNode.Node.Id,
                    parentId.Value
                );
                parentId = null;
            }

            if (!parentId.HasValue)
            {
                rootNodes.Add(treeNode);
                continue;
            }

            if (!childrenByParent.TryGetValue(parentId.Value, out var siblings))
            {
                siblings = new List<NoteTreeNodeModel>();
                childrenByParent[parentId.Value] = siblings;
            }

            siblings.Add(treeNode);
        }

        foreach (var treeNode in nodesById.Values)
        {
            treeNode.Children = SortNodes(
                childrenByParent.TryGetValue(treeNode.Node.Id, out var children)
                    ? children
                    : Enumerable.Empty<NoteTreeNodeModel>()
            );
        }

        return SortNodes(rootNodes);
    }

    private static IReadOnlyList<NoteTreeNodeModel> SortNodes(IEnumerable<NoteTreeNodeModel> nodes)
    {
        return nodes
            .OrderBy(item => item.Node.Type == NoteNodeType.Folder ? 0 : 1)
            .ThenBy(item => item.Node.SortOrder)
            .ThenBy(item => item.Node.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }
}
