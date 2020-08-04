# Google reCaptcha

After creating a new site in the Google's reCaptcha console apply the following changes in you project.
- Add the Site and Secret keys in `appsettings.json`
```json
{
    ...
    "Recaptcha": {
        "SiteKey": "6Lc****",
        "SecretKey": "6Lc****"
      }
    ...
}
```
- Inject the required services to your service collection using `services.AddRecaptcha();`.
- Import the tag helpers from **_ViewImport.cshtml** like `@addTagHelper *, Olive.Mvc.Recaptcha`.
- Add the javascript tag in your layout like:
```html
@if (!Request.IsAjaxRequest())
{
    <script src="/lib/requirejs/require.js" data-main="/scripts/references.js?v=1"></script>
    <recaptcha-script />
}
```
- Add captcha to your module with somethig like `CustomField().ControlMarkup("<recaptcha />");`.
- Validate you action with an attribute:
```csharp
Button("Register").IsDefault()
    .ExtraActionMethodAttributes("[ValidateRecaptcha]");
```