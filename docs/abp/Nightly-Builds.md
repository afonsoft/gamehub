# https://aspnetboilerplate.com/Pages/Documents/Nightly-Builds

## Nightly Builds

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

[Edit on GitHub](https://github.com/aspnetboilerplate/aspnetboilerplate/blob/master/doc/WebSite/Nightly-Builds.md)

In this document

# Nightly Builds [Anchor](https://aspnetboilerplate.com/Pages/Documents/Nightly-Builds\#nightly-builds)

All framework packages are deployed to MyGet every night in weekdays. So, you can use or test the latest code without waiting the next release.

## Configure Visual Studio [Anchor](https://aspnetboilerplate.com/Pages/Documents/Nightly-Builds\#configure-visual-studio)

> Requires Visual Studio 2017+

1. Go to `Tools > Options > NuGet Package Manager > Package Source`.
2. Click the green `+` icon.
3. Set `ABP Nightly` as _Name_ and `https://www.myget.org/F/abp-nightly/api/v3/index.json` as the _Source_ as shown below:

![Nightly builds add nuget source](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/night-build-add-nuget-source.png)
4\. Click the \`Update\` button.
5\. Click the \`OK\` button to save changes.

## Install Package [Anchor](https://aspnetboilerplate.com/Pages/Documents/Nightly-Builds\#install-package)

Now, you can install preview / nightly packages to your project from Nuget Browser or Package Manager Console.

![Nightly builds add nuget package](https://raw.githubusercontent.com/aspnetboilerplate/aspnetboilerplate/master/doc/WebSite/images/night-build-add-nuget-package.png)

1. In the nuget browser, select "Include prereleases".
2. Change package source to "All".
3. Search a package. You will see prereleases of the package formatted as `(VERSION)-preview(DATE)` (like _v4.6.0-preview20190508_ in this sample).
4. You can click to the `Install` button to add package to your project.

|     |     |
| --- | --- |
|  |  |

Twitter Widget Iframe