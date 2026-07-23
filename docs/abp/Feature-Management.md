# https://aspnetboilerplate.com/Pages/Documents/Feature-Management

## Feature Management

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Feature-Management.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#introduction)

Most **SaaS** (multi-tenant) applications have **editions** (packages)
that have different **features**. This way, they can provide different
**price and feature options** to their tenants (customers).

ASP.NET Boilerplate provides a **feature system** to make it easier. You
can **define** features, **check** if a feature is **enabled** for a
tenant, and **integrate** the feature system to other ASP.NET Boilerplate
concepts (like [authorization](https://aspnetboilerplate.com/Pages/Documents/Authorization) and
[navigation](https://aspnetboilerplate.com/Pages/Documents/Navigation)).

#### About IFeatureValueStore [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#about-ifeaturevaluestore)

The feature system uses the **IFeatureValueStore** to get the values of features.
While you can implement it in your own way, it's fully implemented in the
**Module Zero** project. If it's not implemented, NullFeatureValueStore
is used to return null for all features (so the default feature values are
used in this case).

### Feature Types [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#feature-types)

There are two fundamental feature types.

#### Boolean Feature [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#boolean-feature)

Can be "true" or "false". This type of a feature can be **enabled** or
**disabled** (for an edition or for a tenant).

#### Value Feature [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#value-feature)

Can be an **arbitrary value**. While it's stored and retrieved as a string,
numbers also can be stored as strings.

For example, our application may be a task management application and we
may have a limit for creating tasks in a month. Imagine that we have two
different editions/packages; one allows for creating 1,000 tasks per month,
while the other allows for creating 5,000 tasks per month. This feature
should be stored as a value, not simply as true or false.

### Defining Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#defining-features)

A feature should be defined before it is checked. A
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System) can define its own features by
deriving from the **FeatureProvider** class. Here's a very simple feature
provider that defines 3 features:

Copy

```
public class AppFeatureProvider : FeatureProvider
{
    public override void SetFeatures(IFeatureDefinitionContext context)
    {
        var sampleBooleanFeature = context.Create("SampleBooleanFeature", defaultValue: "false");
        sampleBooleanFeature.CreateChildFeature("SampleNumericFeature", defaultValue: "10");
        context.Create("SampleSelectionFeature", defaultValue: "B");
    }
}
```

After creating a feature provider, we must register it in our module's
[PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System#preinitialize) method
as shown below:

Copy

```
Configuration.Features.Providers.Add<AppFeatureProvider>();
```

#### Basic Feature Properties [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#basic-feature-properties)

A feature definition requires at least two properties:

- **Name**: A unique name (string) to identify the feature.
- **Default value**: A default value. This is used when we need the
value of the feature and it's not available for current tenant.

Here, we defined a boolean feature named "SampleBooleanFeature", with the
default value of "false" (not enabled). We also defined two value
features. Note that SampleNumericFeature is defined as a child of
SampleBooleanFeature.

Tip: Create a const string for a feature name and use it everywhere to
prevent typing errors.

#### Other Feature Properties [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#other-feature-properties)

While the unique name and default value properties are required, there are
some optional properties for more fine-tuned control.

- **Scope**: A value in the FeatureScopes enum. It can be **Edition** (if
this feature can be set only for edition level), **Tenant** (if this
feature can be set only for tenant level) or **All** (if this
feature can be set for editions and tenants, where a tenant setting
overrides its edition's setting). Default value is **All**.
- **DisplayName**: A localizable string to show the feature's name to
users.
- **Description**: A localizable string to show the feature's detailed
description to users.
- **InputType**: A UI input type for the feature. This can be defined,
and then used while creating an automatic feature screen (Available input types are CheckboxInputType, ComboboxInputType and SingleLineStringInputType).
- **Attributes**: An arbitrary custom dictionary of key-value pairs
that are related to the feature.

Let's see some more detailed definitions for the features above:

Copy

```
public class AppFeatureProvider : FeatureProvider
{
    public override void SetFeatures(IFeatureDefinitionContext context)
    {
        var sampleBooleanFeature = context.Create(
            AppFeatures.SampleBooleanFeature,
            defaultValue: "false",
            displayName: L("Sample boolean feature"),
            inputType: new CheckboxInputType()
            );

        sampleBooleanFeature.CreateChildFeature(
            AppFeatures.SampleNumericFeature,
            defaultValue: "10",
            displayName: L("Sample numeric feature"),
            inputType: new SingleLineStringInputType(new NumericValueValidator(1, 1000000))
            );

        context.Create(
            AppFeatures.SampleSelectionFeature,
            defaultValue: "B",
            displayName: L("Sample selection feature"),
            inputType: new ComboboxInputType(
                new StaticLocalizableComboboxItemSource(
                    new LocalizableComboboxItem("A", L("Selection A")),
                    new LocalizableComboboxItem("B", L("Selection B")),
                    new LocalizableComboboxItem("C", L("Selection C"))
                    )
                )
            );
    }

    private static ILocalizableString L(string name)
    {
        return new LocalizableString(name, AbpZeroTemplateConsts.LocalizationSourceName);
    }
}
```

Note that the Input type definitions are not used by ASP.NET Boilerplate.
They can be used by applications to create inputs for features.
ASP.NET Boilerplate just provides the infrastructure to make it easier.

#### Feature Hierarchy [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#feature-hierarchy)

As shown in the sample feature providers, a feature can have **child features**.
A Parent feature is generally defined as a **boolean**
feature. Child features will be available only if the parent is enabled.
ASP.NET Boilerplate **does not** enforce this, but we recommend it.
The application should take care of it.

### Checking Features [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#checking-features)

We define a feature to check its value in the application to allow or
block some application features per tenant. There are different ways of
checking it.

#### Using RequiresFeature Attribute [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#using-requiresfeature-attribute)

We can use the **RequiredFeature** attribute for a method or a class as
shown below:

Copy

```
[RequiresFeature("ExportToExcel")]
public async Task<FileDto> GetReportToExcel(...)
{
    ...
}
```

This method is executed only if the "ExportToExcel" feature is enabled for
the **current tenant** (current tenant is obtained from
[IAbpSession](https://aspnetboilerplate.com/Pages/Documents/Abp-Session)). If it's not enabled, an
**AbpAuthorizationException** is thrown automatically.

As such, the RequiresFeature attribute should only be used for **boolean type**
**features**. Otherwise, you may get exceptions.

##### RequiresFeature attribute notes [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#requiresfeature-attribute-notes)

ASP.NET Boilerplate uses the power of dynamic method interception for
feature checking. There are some restrictions for the methods that can use the
RequiresFeature attribute.

- You can not use it for private methods.
- You can not use it for static methods.
- You can not use it for methods of a non-injected class (We must use
[dependency injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection)).

Also,

- You can use it for any **public** method if the method is called over an
**interface** (like Application Services used over interface).
- A method should be **virtual** if it's called directly from a class
reference (like ASP.NET MVC or Web API Controllers).
- A method should be **virtual** if it's **protected**.

#### Using IFeatureChecker [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#using-ifeaturechecker)

We can inject and use IFeatureChecker to check a feature manually (it's
automatically injected and directly usable for application services, MVC,
and Web API controllers).

##### IsEnabled [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#isenabled)

This is used to simply check if a given feature is enabled or not. Example:

Copy

```
public async Task<FileDto> GetReportToExcel(...)
{
    if (await FeatureChecker.IsEnabledAsync("ExportToExcel"))
    {
        throw new AbpAuthorizationException("You don't have this feature: ExportToExcel");
    }

    ...
}
```

The IsEnabledAsync and other methods also have sync versions.

The IsEnabled method should be used for **boolean type features**,
otherwise you may get exceptions.

If you just want to check a feature and throw an exception as shown in the
example, you can use the **CheckEnabled** method.

##### GetValue [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#getvalue)

Used to get the current value of a feature for value-type features. Example:

Copy

```
var createdTaskCountInThisMonth = GetCreatedTaskCountInThisMonth();
if (createdTaskCountInThisMonth >= FeatureChecker.GetValue("MaxTaskCreationLimitPerMonth").To<int>())
{
    throw new AbpAuthorizationException("You exceed task creation limit for this month, sorry :(");
}
```

The FeatureChecker methods also have overrides to check features not only for the
current tenantId, but for a **specified** tenantId as well.

#### Client Side (Razor Views) [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#client-side-razor-views-)

The base view class defines the **IsFeatureEnabled** method to check if the current feature enabled.

##### isEnabled [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#isenabled-1)

Copy

```csharp
    @if (IsFeatureEnabled("App.ChatFeature"))
    {
        <div class="row text-end mb-3">
            <div class="col">
                <button class="btn btn-primary" id="AddFriendButton">Add Friend</button>
            </div>
        </div>
    }
```

##### getValue [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#getvalue-1)

Copy

```csharp
    @if (CurrentUser.Friends.Count() < Convert.ToInt32(GetFeatureValue("App.FriendLimit")))
    {
        <div class="row text-end mb-3">
            <div class="col">
                <button class="btn btn-primary" id="AddFriendButton">("AddFriendButton")</button>
            </div>
        </div>
    }
```

#### Client Side (Javascript) [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#client-side-javascript-)

In the client side, we can use the **abp.features** namespace to get the current values of features.

##### isEnabled [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#isenabled-2)

Copy

```csharp
    var isEnabled = abp.features.isEnabled('SampleBooleanFeature');
```

##### getValue [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#getvalue-2)

Copy

```csharp
    var value = abp.features.getValue('SampleNumericFeature');
```

#### Ignore Feature Check For Host Users [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#ignore-feature-check-for-host-users)

If you enabled Multi-Tenancy, then you can also ignore feature check for host users by configuring it in PreInitialize method of our module as shown below:

Copy

```
Configuration.MultiTenancy.IgnoreFeatureCheckForHostUsers = true;
```

**Note:**`IgnoreFeatureCheckForHostUsers` default value is `false`;

### Feature Manager [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#feature-manager)

If you need the definitions of features, you can inject and use
**IFeatureManager**.

### A Note For Editions [Anchor](https://aspnetboilerplate.com/Pages/Documents/Feature-Management\#a-note-for-editions)

The ASP.NET Boilerplate framework does not have a built-in edition system because
such a system requires a database (to store editions, edition features,
tenant-edition mappings and so on...). Therefore, the edition system is
implemented in [Module Zero](https://aspnetboilerplate.com/Pages/Documents/Zero/Edition-Management).
You can use it as a ready-made edition system or implement
one yourself.

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |