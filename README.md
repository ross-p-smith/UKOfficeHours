# UKOfficeHours
===============

Home for the source code of the DX UK Office Hours Booking Site
---------------------------------------------------------------

Deploys a multi-site app architecture in a resource group, with table storage, a web front end in app service and a functions middle tier

The service is simply a simple example booking system, written as a Knockout / Bootstrap SPA
The front end is pure HTML5/js, using knockout for data binding, moment.js for date handling and bootstrap for UI styling.
It uses Azure Functions for it's API / Middle Tier and is deployed via the KUDU Zip API to Azure app service
Azure Table Storage is used on the back end for data nosql storage.

Deploying
---------

The provided poshdeploy script should be run to configure the environment 

Login first with Login-AzureRmAccount, or VSTS, then run poshdeploy.ps1
This should run on any machine with the azure sdk installed -- powershell hints below.

1. Download the repo
2. cd to <Repo>\UKOfficeHours\UKOfficeHours
3. .\armdeploy\poshdeploy -deployname "<two letter environment code>"

Valid environments for our internal usage deployment are "lo","cd","de","ts","pr", but any free combination can in theory be used.
"lo" local services configured to interact with a local frontend
"co" continuous delivery
"tr" training environment with realistic user dummy data
"tp" test environment (with performance settings for perf and load test)
"pr" production / live environment

Skipping -deployname param will run a dummy ci deployment to a resgroup of "dc" - a dummy ci testing environment.

The script should then take care of everything else apart from setting the Azure AD App Manifest to allow oauth implicit flows, which you will have to do manually in the Azure AD part of the azure portal. Set "oauth2AllowImplicitFlow": true in the application manifest

For local debugging to work, you also probably want to add your local endpoint addresses to the Azure AD endpoints list.
Apart from that, get the repo and do this to deploy a dev environment.

PS C:\Users\wieastbu\Source\Repos\UKOfficeHours\UKOfficeHours> .\armdeploy\poshdeploy "de" 



Visual Studio Team System is used to trigger and host the build and deploy system, whilst the primary code repo is here.
<img src="https://dxukprogrammatic.visualstudio.com/_apis/public/build/definitions/fe221f9a-c953-4f87-8184-d1d51aec1f9e/1/badge">



