# https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management

## Language Management

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Zero/Language-Management.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#introduction)

ASP.NET Boilerplate defines a strong UI [localization\\
system](https://aspnetboilerplate.com/Pages/Documents/Localization) which is used both on the server and
client sides. It allows us to easily configure application languages and
define localization texts (strings) in different sources (Resource files
and XML files are two pre-defined sources).

While it's good for most cases, we may want to define languages and
texts **dynamically** and on a **database**. Module Zero allows us to
dynamically manage application **languages** and **texts** **per**
**tenant**.

#### About Localization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#about-localization)

We strongly recommend you read the [localization\\
documentation](https://aspnetboilerplate.com/Pages/Documents/Localization) before this document.

### How To Enable [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#how-to-enable)

#### Startup Template [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#startup-template)

If you create your project from the [startup templates](https://aspnetboilerplate.com/Templates), you can
skip this section since the template comes with the database-based
localization enabled by default. If you created your project before this
feature, please read this to enable it for your application.

Database localization is designed to be backwards-compatible with ASP.NET
Boilerplate's existing localization system. It actually replaces all
the existing dictionary-based localization sources with
**MultiTenantLocalizationSource**.

MultiTenantLocalizationSource wraps existing
**DictionaryBasedLocalizationSource** based sources. We generally
wrap [XML based\\
localization](https://aspnetboilerplate.com/Pages/Documents/Localization#xml-files) sources. It
can not wrap **Resource File** sources since resource files are designed
as hard-coded and static files which are not proper for dynamic
localization.

Since it's a wrapper, the underlying XML files are used as a fallback source
if a text is not localized in the database. It may seem complicated,
but it's easy to implement for your application. Let's see how to enable the
database-based localization.

#### EnableDbLocalization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#enabledblocalization)

First, we enable it:

Copy

```
Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();
```

This should be done in the **top level** module's
**[PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System#preinitialize)**
method (it's the web module for a web application. Import the
Abp.Zero.Configuration namespace (using Abp.Zero.Configuration) to see
the Zero() extension methods).

This configuration makes all the magic happen, but we must add
some more code to make it work properly.

#### Seed Database Languages [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#seed-database-languages)

Since ABP will get a list of languages from the database, we must
**insert** the default languages into it. If you're using
EntityFramework, you can [use this seed code](https://github.com/aspnetboilerplate/module-zero-template/blob/master/src/AbpCompanyName.AbpProjectName.EntityFramework/Migrations/SeedData/DefaultLanguagesCreator.cs):

#### Remove Static Language Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#remove-static-language-configuration)

If you have a static language configuration like the one shown below, you can
**delete** these lines from your configuration code, since they will get
the languages from the database.

Copy

```
Configuration.Localization.Languages.Add(new LanguageInfo("en", "English", "famfamfam-flag-england", true));
```

#### Note On Existing XML Localization Sources [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#note-on-existing-xml-localization-sources)

**Do not** delete your XML localization files and source configuration
code. These files are used as a **fallback source** and all
localization keys are obtained from this source.

So when you need a new localized text, **define it** into the XML files
as you do normally. You must at least define it in the **default**
language's XML file. Note: you don't need to add the default values of
the localized texts to the database migration code.

### Managing Languages [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#managing-languages)

The **IApplicationLanguageManager** interface is
[injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and used to manage
languages. It has methods like GetLanguagesAsync, AddAsync, RemoveAsync,
UpdateAsync... to manage languages for the host and tenants.

#### Language List Logic [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#language-list-logic)

The list of languages are stored per tenant and for the host, and calculated
as follows:

- There is a list of languages defined for **the host**. This list
is considered as the **default** for all tenants.
- There is a separated list of languages for **each tenant**. This
list **inherits** the host list and **adds** tenant-specific languages.
Tenants can not delete or update host-defined (default) languages
(but can override localization texts as we will see later).

#### ApplicationLanguage Entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#applicationlanguage-entity)

The ApplicationLanguage entity represents a language for a tenant or the
host.

Copy

```
[Serializable]
[Table("AbpLanguages")]
public class ApplicationLanguage : FullAuditedEntity, IMayHaveTenant
{
    //...
}
```

Its basic properties are:

- **TenantId** (nullable): Contains the related tenant's Id if this
language is tenant-specific. It's null if this is a host language.
- **Name**: Name of the language. This **must be a culture code** from
[this list](https://learn.microsoft.com/en-us/previous-versions/commerce-server/ee825488(v=cs.20)).
- **DisplayName**: Shown name of the language. This can be an
arbitrary name, but it generally is the
[CultureInfo.DisplayName](https://learn.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.displayname?view=net-8.0).
- **Icon**: An arbitrary icon/flag for the language. This can be used
to show flag of the language on the UI.

The ApplicationLanguage also inherits from **FullAuditedEntity**.
This means it's a **soft-delete** entity and automatically **audited**
(see the [entity document](https://aspnetboilerplate.com/Pages/Documents/Entities) for more info).

The ApplicationLanguage entities are stored in the **AbpLanguages** table in the
database.

### Managing Localization Texts [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#managing-localization-texts)

The **IApplicationLanguageTextManager** interface is
[injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) and used to manage
localization texts. It has the needed methods to get/set a localization text
for a tenant or the host.

#### Localizing A Text [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#localizing-a-text)

Let's see what happens when you want to localize a text;

- First, it tries to get the **current culture** using the
CurrentThread.CurrentUICulture.

  - It checks if the given text is defined (overridden) for the **current**
    **tenant** using
    [IAbpSession.TenantId](https://aspnetboilerplate.com/Pages/Documents/Abp-Session) in
    the **current culture** in the database. It returns the value if
    it is defined.
  - It then checks if a given text is defined (overridden) for the
    **host** in the **current culture** in the database. It returns the
    value if it is defined.
  - It then checks if a given text is defined in the underlying XML
    file in the **current culture**. It returns the value if it is defined.
- Second, it will try to find the **fallback culture**. It's calculated like this:
If the current culture is "en-GB", then the fallback culture is "en".

  - It checks if a given text is defined (overrided) for the **current**
    **tenant** in the **fallback culture** in the database. It returns the
    value if it is defined.
  - It then checks if the given text is defined (overridden) for the
    **host** in the **fallback culture** in the database. It returns the
    value if it is defined.
  - It then checks if the given text is defined in the underlying XML
    file in the **fallback culture**. It returns the value if it is defined.
- Third, it will try to find the **default culture**.

  - It checks if a given text is defined (overridden) for the **current**
    **tenant** in the **default culture** in the database. It returns the
    value if it is defined.
  - It then checks if the given text is defined (overridden) for the
    **host** in the **default culture** in the database. It returns the
    value if it is defined.
  - It then checks if a given text is defined in the underlying XML
    file in the **default culture**. It returns the value if it is defined.
- If all attempts fail, it will get the same text or an throw exception.
  - If the given text (key) is not found at all, ABP throws an exception or
    returns the same text (key) by wrapping it with \[ and \] (it can
    be configured on startup, see the [localization\\
    document](https://aspnetboilerplate.com/Pages/Documents/Localization)).

Getting a localized text is a bit complicated, but it works fast
since it uses the [cache](https://aspnetboilerplate.com/Pages/Documents/Caching).

#### ApplicationLanguageText Entity [Anchor](https://aspnetboilerplate.com/Pages/Documents/Zero/Language-Management\#applicationlanguagetext-entity)

The ApplicationLanguageText entity is used to store localized values in the
database.

Copy

```
[Serializable]
[Table("AbpLanguageTexts")]
public class ApplicationLanguageText : AuditedEntity<long>, IMayHaveTenant
{
    //...
}
```

It's basic properties are;

- **TenantId** (nullable): Contains the related tenant's Id if this
localized text is tenant-specific. It's null if this is a
host-localized text.
- **LanguageName**: Name of the language. This **must be a culture**
**code** from [this list](https://learn.microsoft.com/en-us/previous-versions/commerce-server/ee825488(v=cs.20)).
This matches to the ApplicationLanguage.Name but is not a forced foreign
key to make it independent from the language entry.
IApplicationLanguageTextManager handles it properly.
- **Source**: Localization source name.
- **Key**: Localization text's key/name.
- **Value**: Localized value.

ApplicationLanguageText entities are stored in the **AbpLanguageTexts**
table in the database.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe