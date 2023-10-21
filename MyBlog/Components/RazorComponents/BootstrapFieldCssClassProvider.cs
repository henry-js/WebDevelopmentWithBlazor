using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Components;

public class BootstrapFieldCssClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        var isValid = !editContext.GetValidationMessages(fieldIdentifier).Any();
        var isModified = editContext.IsModified(fieldIdentifier);
        return (isModified, isValid) switch
        {
            (true, true) => "form-control modified is-valid",
            (true, false) => "form-control modified is-invalid",
            (false, true) => "form-control",
            (false, false) => "form-control",
        };
    }
}

public class CustomCssClassProvider<ProviderType> : ComponentBase where ProviderType : FieldCssClassProvider, new()
{
    [CascadingParameter]
    EditContext? CurrentEditContext { get; set; }
    public ProviderType Provider { get; set; } = new ProviderType();
    protected override void OnInitialized()
    {
        if (CurrentEditContext == null)
            throw new InvalidOperationException($"{nameof(CustomCssClassProvider<ProviderType>)} requires a cascading parameter of type {nameof(EditContext)}. For example, you can use {nameof(CustomCssClassProvider<ProviderType>)} inside an EditForm.");

        CurrentEditContext.SetFieldCssClassProvider(Provider);
    }
}