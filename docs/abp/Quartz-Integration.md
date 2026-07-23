# https://aspnetboilerplate.com/Pages/Documents/Quartz-Integration

## Quartz Integration

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Quartz-Integration.md)

In this document

### Introduction [Anchor](https://aspnetboilerplate.com/Pages/Documents/Quartz-Integration\#introduction)

[Quartz](http://www.quartz-scheduler.net/) is a full-featured, open-source job scheduling system that can be used from the smallest apps to large-scale enterprise systems.

The [Abp.Quartz](https://www.nuget.org/packages/Abp.Quartz) package simply integrates Quartz to ASP.NET Boilerplate.

ASP.NET Boilerplate has a built-in [persistent background job queue and\\
background worker](https://aspnetboilerplate.com/Pages/Documents/Background-Jobs-And-Workers) system. Quartz can
be a good alternative if you have advanced scheduling requirements for
your background workers. [Hangfire](https://aspnetboilerplate.com/Pages/Documents/Hangfire-Integration) can also
be a good alternative for persistent background job queues.

### Installation [Anchor](https://aspnetboilerplate.com/Pages/Documents/Quartz-Integration\#installation)

Install the [**Abp.Quartz**](https://www.nuget.org/packages/Abp.Quartz)
NuGet package to your project and add a **DependsOn** attribute to your
[module](https://aspnetboilerplate.com/Pages/Documents/Module-System) for **AbpQuartzModule**:

Copy

```
[DependsOn(typeof (AbpQuartzModule))]
public class YourModule : AbpModule
{
    //...
}
```

### Creating Jobs [Anchor](https://aspnetboilerplate.com/Pages/Documents/Quartz-Integration\#creating-jobs)

To create a new job, you can either implement Quartz's IJob interface,
or derive from the JobBase class (defined in the Abp.Quartz package) that has
some helper properties/methods for logging and localization, for
example. A simple job class is shown below:

Copy

```
public class MyLogJob : JobBase, ITransientDependency
{
    public override Task Execute(IJobExecutionContext context)
    {
        Logger.Info("Executed MyLogJob..!");
        return Task.CompletedTask;
    }
}
```

We simply implemented the **Execute** method to write a log. You can see
Quartz's [documentation](http://www.quartz-scheduler.net/) for more info.

### Schedule Jobs [Anchor](https://aspnetboilerplate.com/Pages/Documents/Quartz-Integration\#schedule-jobs)

The **IQuartzScheduleJobManager** is used to schedule jobs. You can inject
it to your class (or you can Resolve and use it in your module's
PostInitialize method) to schedule jobs. An example Controller that
schedules a job:

Copy

```
public class HomeController : AbpController
{
    private readonly IQuartzScheduleJobManager _jobManager;

    public HomeController(IQuartzScheduleJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public async Task<ActionResult> ScheduleJob()
    {
        await _jobManager.ScheduleAsync<MyLogJob>(
            job =>
            {
                job.WithIdentity("MyLogJobIdentity", "MyGroup")
                    .WithDescription("A job to simply write logs.");
            },
            trigger =>
            {
                trigger.StartNow()
                    .WithSimpleSchedule(schedule =>
                    {
                        schedule.RepeatForever()
                            .WithIntervalInSeconds(5)
                            .Build();
                    });
            });

        return Content("OK, scheduled!");
    }
}
```

### More [Anchor](https://aspnetboilerplate.com/Pages/Documents/Quartz-Integration\#more)

Please see Quartz's [documentation](http://www.quartz-scheduler.net/) for more information.

Twitter Widget Iframe

|     |     |
| --- | --- |
|  |  |