## TFS WebHooks

This project aims to work around the limitation of having build triggers listen to only one repository in TFS.

### What are Service hooks?

[The documentation will explain it better](https://www.visualstudio.com/en-us/docs/service-hooks/services/webhooks)

### But I don't have time to read documentation!

OK.
In a nutshell, TFS can notify a consumer when specific events happen. Some examples of events are:

 - A build has completed
 - A work item has been created
 - A pull request has been created

This allows us to trigger actions that TFS doesn't support natively when these events happen.

### So how is this useful?

In our case, TFS doesn't provide us with the capability of queing a build when a PR in any other repository than the master repository is merged.
Luckily, TFS Web hooks supports, among [lots of events](https://www.visualstudio.com/docs/integrate/get-started/service-hooks/events), one when [a pull request is updated](https://www.visualstudio.com/docs/integrate/get-started/service-hooks/events#git.pullrequest.updated).

*I hear you screaming that an even better event exists, when [a pull request is merged](https://www.visualstudio.com/docs/integrate/get-started/service-hooks/events#git.pullrequest.merged). Unfortunately, this documentation is for VSTS, and TFS (2015 Update 2 at the time of writing) supports only a subset of these events as it's not released as frequently.*

Still, we can subscribe to this event and queue a build by calling back the [TFS REST API](https://www.visualstudio.com/en-us/docs/integrate/api/overview).

### Cool. How does this work?

This project relies on two projects that make the implementation very easy:

 - [ASP.NET WebHooks](https://github.com/aspnet/WebHooks) is a library that sits on top of ASP.NET Web API and allows us to be a consumer of web hooks. It has integrated support for [VSTS and TFS web hooks](https://github.com/aspnet/WebHooks/tree/master/samples/VstsReceiver)
 - The [.NET Client libraries for VSTS and TFS](https://www.visualstudio.com/en-us/docs/integrate/get-started/client-libraries/dotnet). They provide a strongly-typed wrapper around the TFS REST API.

### Anything more I need to know?

We don't want to queue a build when any PR on any repository gets merged. For example, a build is usually kicked off when a pull request on the main repo is merged. Similarly, it doesn't make sense to queue a build if PRs get merged on repos that are not associated to the main repo.

To overcome this, we can filter the pull requests based on:

 - The repository on which the PR has been created
 - The target branch of the PR
 - The status of the PR

These parameters are set in the `web.config`:

| Key name | Description |
|----------|-------------|
| PullRequestTargetBranchName | The name of the target branch of the PR |
| PullRequestStatus | The status of the PR |
| PullRequestRepositoryPattern | A Regex-compatible pattern for the name of the repository |

### What about the build we want to queue?

| Key name | Description |
|----------|-------------|
| TeamProjectCollectionUri | The URL of the target Team Project Collection |
| TargetTeamProjectName | The name of the Team Project that contains the build definition |
| TargetBuildDefinitionName | The name of the build definition |

### What about these `MS_WebHook*` keys?

#### `MS_WebHookReceiverSecret_VSTS`

This key specifies the secret for the VSTS web hooks receiver. You don't want everyone to point their subscriptions to your Web API so that's how the team behind ASP.NET WebHooks decided to implement authorization.
This secret has to be appended to the URL you set up in TFS, in a query-string parameter called `code`.

A URL might then looks like `https://<host>/api/webhooks/incoming/vsts?code=<code>`.
Let's cut this URL down to pieces:

 - `<host>` is where your API is hosted
 - `/api/webhooks` is the route prefix set by ASP.NET WebHooks
 - `/incoming` means we are receiving a web hook (the library also allows you to be the producer of web hooks)
 - `/vsts` means we want to contact the VSTS-specific endpoint
 - `?code=<code>` to authorize VSTS or TFS to call your API

#### `MS_WebHookDisableHttpsCheck`

By default, ASP.NET WebHooks enforces that your API be available over HTTPS. This key allows to remove that requirement.
