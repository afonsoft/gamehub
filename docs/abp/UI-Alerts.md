# https://aspnetboilerplate.com/Pages/Documents/UI-Alerts

## UI Alerts

|     |     |     |
| --- | --- | --- |
| |     |     |
| --- | --- |
|  | × | | search |  |

Custom Search

|     |     |
| --- | --- |
|  | Sort by<br>Relevance<br>Date |

Version

latest (v10.4)v10.3v10.2v10.1v10.0v9.4.2v9.4.1v9.4.0v9.3.0v9.2.0v9.1.3v9.1v9.0v8.4v8.3v8.2v8.1v8.0v7.4v7.3v7.2v7.1v7.0-rc1v7.0v6.6.1v6.6.0v6.5.0v6.4-rc1v6.4.0v6.3.1v6.3v6.2v6.1.1v6.1.0v6.0v5.14v5.13v5.12v5.10.1v5.10v5.9v5.8v5.7v5.6v5.5v5.4v5.3v5.2v5.1.0v5.0.0v4.21v4.20v4.19v4.18v4.17v4.16v4.15v4.14v4.13v4.12v4.11.0v4.10.1v4.10.0v4.9.0v4.8.1v4.8.0v4.7.0v4.6.0v4.5.0v4.4.0v4.3.0v4.2.0v4.1.0v4.0.2v4.0.1v4.0.0v3.9.0v3.8.3v3.8.2v3.8.1v3.8.0v3.7.2v3.7.1v3.7.0v3.6.2v3.6.1v3.6.0v3.5.0v3.4.0v3.3.0v3.2.5v3.2.4v3.2.3v3.2.2v3.2.1v3.2.0v3.1.2v3.1.1v3.1.0v3.0.0-beta3v3.0.0-rc2v3.0.0-beta2v3.0.0-rc1v3.0.0-beta1v3.0.0v2.3.0v2.2.2v2.2.1v2.2.0v2.1.3v2.1.2v2.1.1v2.1.0-beta4v2.1.0-beta3v2.1.0-beta2v2.1.0-beta1v2.1.0v2.0.2v2.0.1v2.0.0-preview4v2.0.0-rc3v2.0.0-preview3v2.0.0-rc2v2.0.0-preview1v2.0.0v2.0.0-rcv1.5.2v1.5.1v1.5.0v1.4.3v1.4.2v1.4.1v1.4.0.0v1.3.1.0v1.3.0.0v1.2.2.0v1.2.1.0v1.2.0.0v1.1.3.0v1.1.1.0v1.1.0.0v1.0.0.0v0.10.3.2Menu

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/UI-Alerts.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/UI-Alerts\#introduction)

Alerts is a common way to show messages to the user after a web request. Examples:

![UI Alerts](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/UI-Alerts.png)

ASP.NET Boilerplate provides a simple alert infrastructure for **MVC** applications (both for ASP.NET Core MVC & ASP.NET MVC 5.x).

> UI Alert system is designed for the actions those return **views**. An action returns a JSON/object result can not use the UI alert system (because it's something related to UI rather than APIs).

### Add Alerts [Anchor](https://aspnetboilerplate.com/Pages/Documents/UI-Alerts\#add-alerts)

`IAlertManager` can be injected and used to add alerts into its `Alerts` collection. `AbpControllerBase` and `AbpPageModel` base classes already injects it and have a shortcut to use the `Alerts` collection.

> All alert messages added in the same web request are added in the same collection even if they are added by different classes/controllers/services.

Example MVC Controller action (that produces the output shown in the figure above):

Copy

```c
public class AlertsTestController : AbpControllerBase
{
    public IActionResult Index()
    {
        Alerts.Danger("Danger alert message!", "Test Alert");//AlertDisplayType.PageAlert is default.
        Alerts.Warning("Warning alert message!", "Test Alert");
        Alerts.Info("Info alert message!", "Test Alert");
        Alerts.Success("Success alert message!", "Test Alert");

        Alerts.Danger("Danger toast message!", "Test Toast", displayType: AlertDisplayType.Toastr);
        Alerts.Warning("Warning toast message!", "Test Toast", displayType: AlertDisplayType.Toastr);
        Alerts.Info("Info toast message!", "Test Toast", displayType: AlertDisplayType.Toastr);
        Alerts.Success("Success toast message!", "Test Toast", displayType: AlertDisplayType.Toastr);

        Alerts.Danger("Danger toast message!", "Test Toast", dismissible: false, displayType: AlertDisplayType.Toastr);
        Alerts.Warning("Warning toast message!", "Test Toast", dismissible: false, displayType: AlertDisplayType.Toastr);
        Alerts.Info("Info toast message!", "Test Toast", dismissible: false, displayType: AlertDisplayType.Toastr);
        Alerts.Success("Success toast message!", "Test Toast", dismissible: false, displayType: AlertDisplayType.Toastr);

        return View();
    }
}
```

### Show Alerts [Anchor](https://aspnetboilerplate.com/Pages/Documents/UI-Alerts\#show-alerts)

MVC based [startup templates](https://aspnetboilerplate.com/Templates) already shows alerts on the page by default. So, nothing to do if you are using one of the startup templates.

##### Adding Alerts to Your Custom Template [Anchor](https://aspnetboilerplate.com/Pages/Documents/UI-Alerts\#adding-alerts-to-your-custom-template)

Alert messages are served by `IAlertMessageRenderer`s.

We have two type of alert message renderer by default.

- _PageAlertMessageRenderer_
It returns bootstrap alert div so that you can add it in your page.

- _ToastrAlertMessageRenderer_
It return toastr scripts. That's why it should be located in `scripts` section in your page.


> Important Note: To show toastr, you should add its css and js reference. (see the example below.)


To show them in your page you can use `IAlertMessageRendererManager`

Copy

```html
@using Abp.Web.Mvc.Alerts
@inject IAlertMessageRendererManager AlertMessageRendererManager

@section styles{
    <link href="~/lib/toastr/toastr.css" rel="stylesheet" />
}
<!--...-->
<div id="AbpPageAlerts">
    @Html.Raw(AlertMessageRendererManager.Render(AlertDisplayType.PageAlert))
</div>
<!--...-->

@section scripts{
    <script src="~/lib/toastr/toastr.min.js" type="text/javascript"></script>
    <script src="~/Abp/Framework/scripts/libs/abp.toastr.js" type="text/javascript"></script>

    @Html.Raw(AlertMessageRendererManager.Render(AlertDisplayType.Toastr))
}
```

##### Adding New Alert Renderer [Anchor](https://aspnetboilerplate.com/Pages/Documents/UI-Alerts\#adding-new-alert-renderer)

Create new class inherited from `IAlertMessageRenderer`.

Name your alert renderer type.

(`PageAlert` and `Toastr` are already used by `AlertDisplayType`. Pick another name.).

Copy

```c
    public class MyOwnAlertMessageRenderer : IAlertMessageRenderer
    {
        public string DisplayType { get; } = "MyOwnAlertRenderer";//Name your renderer

        public string Render(List<AlertMessage> alertList)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var alertMessage in alertList)
            {
                sb.Append($"<div>{alertMessage.Text}</div>");//your alert template
            }
            return sb.ToString();
        }
    }
```

Then you can use alert with your own renderer.

Copy

```c
 Alerts.Danger("Danger toast message!", "Test Toast", dismissible: false, displayType: "MyOwnAlertRenderer");
```

Copy

```c
 @Html.Raw(AlertMessageRendererManager.Render("MyOwnAlertRenderer"))
```

> If you want to access to the alerts added by the current request, you can always inject the `IAlertManager` and use its `Alerts` collection.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe