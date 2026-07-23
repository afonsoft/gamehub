# https://aspnetboilerplate.com/Pages/Documents/Email-Sending

## Email Sending

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Email-Sending.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#introduction)

Sending emails is a very common task for most applications.
ASP.NET Boilerplate provides the basic infrastructure to send
emails in a simple way. It also separates the email server configuration from the sending
of emails.

### IEmailSender [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#iemailsender)

**IEmailSender** is a service to send emails without knowing the details. Example usage:

Copy

```
public class TaskManager : IDomainService
{
    private readonly IEmailSender _emailSender;

    public TaskManager(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public void Assign(Task task, Person person)
    {
        //Assign task to the person
        task.AssignedTo = person;

        //Send a notification email
        _emailSender.Send(
            to: person.EmailAddress,
            subject: "You have a new task!",
            body: $"A new task is assigned for you: <b>{task.Title}</b>",
            isBodyHtml: true
        );
    }
}
```

We simply [injected](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) **IEmailSender** and
used the **Send** method. The Send method has additional overloads. For example, it
can also get a MailMessage object (not available for .NET Core since .NET Core
does not include SmtpClient and MailMessage).

#### ISmtpEmailSender [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#ismtpemailsender)

There is also an **ISmtpEmailSender** which extends IEmailSender and adds a
**BuildClient** method to create an **SmtpClient** and then directly uses it
(not available for .NET Core since .NET Core does not include SmtpClient
and MailMessage). Using IEmailSender will be enough for most cases.

#### NullEmailSender [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#nullemailsender)

There is also a [null object\\
pattern](https://en.wikipedia.org/wiki/Null_Object_pattern)
implementation of IEmailSender, aptly named **NullEmailSender**. You can use it in
unit tests or inject IEmailSender with the [property\\
injection](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) pattern.

### Configuration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#configuration)

Email Sender uses a [settings management](https://aspnetboilerplate.com/Pages/Documents/Setting-Management) system
to read email-sending configurations. All the setting names are defined in the
Abp.Net.Mail.EmailSettingNames class as constant strings.

Their values and descriptions:

- Abp.Net.Mail. **DefaultFromAddress**: Used as the sender's email address
when you don't specify a sender when sending emails (just like in the
example above).
- Abp.Net.Mail. **DefaultFromDisplayName**: Used as the sender's display name
when you don't specify a sender when sending emails (just like in the
example above).
- Abp.Net.Mail. **Smtp.Host**: The IP/Domain of the SMTP server (default:
127.0.0.1).
- Abp.Net.Mail. **Smtp.Port**: The Port of the SMTP server (default: 25).
- Abp.Net.Mail. **Smtp.UserName**: Username, if the SMTP server requires
authentication.
- Abp.Net.Mail. **Smtp.Password**: Password, if the SMTP server requires
authentication.
- Abp.Net.Mail. **Smtp.Domain**: Domain for the username, if the SMTP
server requires authentication.
- Abp.Net.Mail. **Smtp.EnableSsl**: A value that indicates if the SMTP server
uses SSL or not ("true" or "false". Default: "false").
- Abp.Net.Mail. **Smtp.UseDefaultCredentials**: If true, uses default
credentials instead of the provided username and password ("true" or
"false". Default: "true").

### MailKit Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#mailkit-integration)

Since .NET Core does not support the standard System.Net.Mail.SmtpClient,
we need a 3rd-party vendor to send emails. Fortunately,
[MailKit](https://github.com/jstedfast/MailKit) provides a good
replacement for the default SmtpClient. It's also
[suggested](https://www.infoq.com/news/2017/04/MailKit-MimeKit-Official)
by Microsoft.

The Abp.MailKit package gracefully integrates in to ABP's email sending system, so you
can still use IEmailSender as described above to send emails via MailKit.

#### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#installation)

First, install the [Abp.MailKit](https://www.nuget.org/packages/Abp.MailKit)
NuGet package to your project:

Copy

```
Install-Package Abp.MailKit
```

#### Integration [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#integration)

Add the AbpMailKitModule to the dependencies of your
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System):

Copy

```
[DependsOn(typeof(AbpMailKitModule))]
public class MyProjectModule : AbpModule
{
    //...
}
```

#### Usage [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#usage)

You can use **IEmailSender** as described above since the Abp.MailKit
package [registers](https://aspnetboilerplate.com/Pages/Documents/Dependency-Injection) the MailKit implementation
for it. It also uses the same configuration.

#### Customization [Anchor](https://aspnetboilerplate.com/Pages/Documents/Email-Sending\#customization)

You may need to make additional configuration or customizations while
creating MailKit's SmtpClient. In that case, you can
[replace](https://aspnetboilerplate.com/Pages/Documents/Startup-Configuration) the IMailKitSmtpBuilder interface with
your own implementation. You can derive from the DefaultMailKitSmtpBuilder
to make it easier. For instance, you may want to accept all SSL
certificates. In that case, you can override the ConfigureClient method as
shown below:

Copy

```
public class MyMailKitSmtpBuilder : DefaultMailKitSmtpBuilder
{
    public MyMailKitSmtpBuilder(ISmtpEmailSenderConfiguration smtpEmailSenderConfiguration)
        : base(smtpEmailSenderConfiguration)
    {
    }

    protected override void ConfigureClient(SmtpClient client)
    {
        client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

        base.ConfigureClient(client);
    }
}
```

You can then replace the IMailKitSmtpBuilder interface with your
implementation in the [PreInitialize](https://aspnetboilerplate.com/Pages/Documents/Module-System) method of your
module:

Copy

```
[DependsOn(typeof(AbpMailKitModule))]
public class MyProjectModule : AbpModule
{
    public override void PreInitialize()
    {
        Configuration.ReplaceService<IMailKitSmtpBuilder, MyMailKitSmtpBuilder>();
    }

    //...
}
```

(Don't forget to add the "using Abp.Configuration.Startup;" statement since the
ReplaceService extension method is defined in that namespace)

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe