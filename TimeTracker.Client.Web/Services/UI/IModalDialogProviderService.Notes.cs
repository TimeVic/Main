using Microsoft.AspNetCore.Components;
using TimeTracker.Api.Shared.Dto.Entity.Notes;
using TimeTracker.Api.Shared.Dto.RequestsAndResponses.Dashboard.Notes;
using TimeTracker.Business.Common.Constants.Notes;
using TimeTracker.Client.Core.Services.UI.Modal;

namespace TimeTracker.Client.Web.Services.UI;

public partial interface IModalDialogProviderService
{
    Task<AppModalResult> ShowCreateFolderModal(
        Guid? parentId = null,
        NoteVisibility visibility = NoteVisibility.Workspace,
        EventCallback<CreateNoteFolderRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    );

    Task<AppModalResult> ShowCreateNoteModal(
        Guid? parentId = null,
        NoteVisibility visibility = NoteVisibility.Workspace,
        EventCallback<CreateNoteDocumentRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    );

    Task<AppModalResult> ShowMoveNoteNodeModal(
        NoteTreeNodeDto node,
        IReadOnlyList<NoteTreeNodeDto> nodes,
        EventCallback<MoveNoteNodeRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    );

    Task<AppModalResult> ShowRenameNoteNodeModal(
        NoteTreeNodeDto node,
        EventCallback<RenameNoteNodeRequest>? onSubmit = null,
        Action<AppModalResult>? onClose = null
    );
}
