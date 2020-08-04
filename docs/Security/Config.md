# Owin configurations

### Create a site

1. Go to https://www.google.com/recaptcha/admin/create
2. Select `reCAPTCHA v2` and then `"I'm not a robot" tickbox`.
3. Set domain and for development add **localhost** as domain but you need to set it after creating it.
4. Save it and copy Site and Secret keys.
5. Open *appsettings.json* in your website folder and set the following:

```json
"Recaptcha": {
    "SiteKey": "****",
    "SecretKey": "*****"
  }
 ```
5. Go back to the setting of the newly created site and uncheck `Verify the origin of reCAPTCHA solutions`.

### Changes on project

1. Install `Olive.Mvc.Recaptcha`.
2. Add required services by calling `services.AddRecaptcha();` in you **Startup.cs**.
3. Add `<recaptcha-script />` to your layout.
```html
@if (!Request.IsAjaxRequest())
{
    <script src="/lib/requirejs/require.js" data-main="/scripts/references.js?v=1"></script>
    <recaptcha-script />
}
```
4. Add `<recaptcha />` to your markup just before buttons.
```csharp
CustomField().ControlMarkup("<recaptcha />");
```
5. Import tag helpers from `Olive.Mvc.Recaptcha` in `_ViewImport.cshtml`.
```
@addTagHelper *, Olive.Mvc.Recaptcha
```