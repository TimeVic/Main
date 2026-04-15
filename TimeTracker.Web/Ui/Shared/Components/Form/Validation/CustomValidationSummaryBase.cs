using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace TimeTracker.Web.Ui.Shared.Components.Form.Validation
{
    public class CustomValidationSummaryBase : ComponentBase, IDisposable
    {
        [CascadingParameter]
        protected EditContext? CurrentEditContext { get; set; }

        protected ICollection<string> _validationMessages = new List<string>();

        protected override void OnInitialized()
        {
            if (CurrentEditContext == null)
            {
                throw new InvalidOperationException($"{nameof(CustomValidationSummaryBase)} requires a cascading parameter of type {nameof(EditContext)}. For example, you can use {nameof(CustomValidationSummaryBase)} inside an {nameof(EditForm)}.");
            }

            CurrentEditContext.OnValidationStateChanged += HandleValidationStateChanged;
            UpdateValidationMessages();
        }

        private void HandleValidationStateChanged(object? sender, ValidationStateChangedEventArgs e)
        {
            UpdateValidationMessages();
            StateHasChanged();
        }

        private void UpdateValidationMessages()
        {
            if (CurrentEditContext != null)
            {
                _validationMessages = CurrentEditContext.GetValidationMessages().ToList();
            }
        }

        public void Dispose()
        {
            if (CurrentEditContext != null)
            {
                CurrentEditContext.OnValidationStateChanged -= HandleValidationStateChanged;
            }
        }
    }
}
