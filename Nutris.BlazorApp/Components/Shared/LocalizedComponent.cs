using Microsoft.AspNetCore.Components;
using NutrisBlazor.Services;

namespace Nutris.BlazorApp.Components.Shared
{
    public abstract class LocalizedComponent : ComponentBase, IDisposable
    {
        [Inject] protected ILocalizationService Localizer { get; set; } = default!;

        protected override void OnInitialized()
        {
            Localizer.OnLanguageChanged += HandleLanguageChanged;
        }

        private void HandleLanguageChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            Localizer.OnLanguageChanged -= HandleLanguageChanged;
        }

        protected string T(string key) => Localizer.Translate(key);
    }
}