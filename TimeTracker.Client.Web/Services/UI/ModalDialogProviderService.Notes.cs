using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Services.UI.Modal;
using TimeTracker.Client.Web.Ui.Pages.Dashboard.Notes.Components.Modals;

namespace TimeTracker.Client.Web.Services.UI;

public partial class ModalDialogProviderService
{
    public Task<AppModalResult> ShowCreateFolderModal(
        Guid? parentId = null,
        NoteVisibility visibility = NoteVisibility.Workspace,
        EventCallback<CreateNoteFolderRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(CreateFolderModal.ParentId)] = parentId,
            [nameof(CreateFolderModal.Visibility)] = visibility
        };
        if (onSubmit.HasValue)
        {
            parameters[nameof(CreateFolderModal.OnSubmit)] = onSubmit.Value;
        }

        return _appModalDialogService.ShowAsync<CreateFolderModal>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowCreateNoteModal(
        Guid? parentId = null,
        NoteVisibility visibility = NoteVisibility.Workspace,
        EventCallback<CreateNoteDocumentRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(CreateNoteModal.ParentId)] = parentId,
            [nameof(CreateNoteModal.Visibility)] = visibility
        };
        if (onSubmit.HasValue)
        {
            parameters[nameof(CreateNoteModal.OnSubmit)] = onSubmit.Value;
        }

        return _appModalDialogService.ShowAsync<CreateNoteModal>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowMoveNoteNodeModal(
        NoteTreeNodeDto node,
        IReadOnlyList<NoteTreeNodeDto> nodes,
        EventCallback<MoveNoteNodeRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(MoveNoteNodeModal.Node)] = node,
            [nameof(MoveNoteNodeModal.Nodes)] = nodes
        };
        if (onSubmit.HasValue)
        {
            parameters[nameof(MoveNoteNodeModal.OnSubmit)] = onSubmit.Value;
        }

        return _appModalDialogService.ShowAsync<MoveNoteNodeModal>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Medium,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }

    public Task<AppModalResult> ShowRenameNoteNodeModal(
        NoteTreeNodeDto node,
        EventCallback<RenameNoteNodeRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(RenameNoteNodeModal.Node)] = node
        };
        if (onSubmit.HasValue)
        {
            parameters[nameof(RenameNoteNodeModal.OnSubmit)] = onSubmit.Value;
        }

        return _appModalDialogService.ShowAsync<RenameNoteNodeModal>(
            parameters: parameters,
            options: new AppModalOptions
            {
                Size = AppModalSize.Small,
                HasCloseButton = true,
                IsCloseOnBackdropClick = true,
                IsCloseOnEscapeKey = true
            },
            onClose: onClose
        );
    }
}
