using Fluxor;
using TimeTracker.Api.Shared.Dto.Entity;

namespace TimeTracker.Client.Core.Store.Ui;

[FeatureState]
public record UiState
{
    public bool IsMainMenuOpened { get; set; } = true;
}
