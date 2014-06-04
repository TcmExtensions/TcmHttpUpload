## Introduction #

TcmHttpUpload is a simple drop-in replacement for the SDL Tridion HttpUpload module.
It was created as a resolution to an issue related to throttling in SDL Tridion 2011.

_Note: The throttling issues have been resolved in Tridion 2013 and newer, however cleanup is still required._

### Download #

Pre-compiled binary versions available for download can be found here:
[TcmHttpUpload on Google Drive](https://drive.google.com/folderview?id=0B7HbFVRJj_UnZU1OU0l3NjBDMGc&usp=sharing)


### Issue Experienced #

While using SDL Tridion 2011 SP1 in a production environment with a heavy publishing load (10,000+ items per day), the Tridion supplied HttpUpload module was causing a problem in the publishing throughput.

Normally the HttpUpload module is supposed to return the amount of files waiting at the deployer side as well as the amount of files in active deployment.
It does this by sending the information back to the cd_transport deployer by means of the actual filenames in progress.

The problem lies in the following piece of code:

    private static void HandleList(string listExtension, HttpResponse response)
    {
        string[] files = Directory.GetFiles(HttpsReceiver.CurrentInstance.IncomingFolder, listExtension);
        string s = "";
        
        foreach (string str3 in files)
        {
            s = s + str3 + ":";
        }
        
        response.Write(s);
    }


Some of the calls executed by this function are:

    http://staging.company.com/httpupload.aspx?action=list&extension=.progress
    http://staging.company.com/httpupload.aspx?action=list&extension=.Content.zip

This means the function is called with ".progress" or ".Content.zip" as parameter.
Subsequently its calling [Directory.GetFiles()](http://msdn.microsoft.com/en-us/library/system.io.directory.getfiles(v=vs.110).aspx) to filter on that extension, however the wildcard mask is missing when doing this, which means its actually searching for a file named exactly ".Content.zip" or ".progress" which does not exist.

Besides resolving that bug, another bug is present.
The code returns the full path to filenames seperated by colon (:).
This creates problems as the returned filenames on a Windows system will look like:

    C:\Tridion\Incoming\tcm_0-113107-66560.Content:C:\Tridion\Incoming\tcm_0-113108-66560.Content

Because there is a colon present in the windows file path also, the cd\_transport service will think there are twice as many files returned in the list as there actually are. (cd\_transport only counts the number of files and takes no action on the returned names other than establishing if the deployer is busy).

TcmHttpUpload resolves this issue by searching with a proper wildcard mask "\*.progress" and "\*.Content.zip" and returns just the file names colon-seperated, i.e.

    tcm_0-113107-66560.Content:tcm_0-113108-66560.Content


### Resolution #

TcmHttpUpload is a separate stand-alone HttpModule available both in .NET 2 and .NET 4 versions.
It supports all the interactions requested by the Tridion cd\_transport service.

Besides resolving the above issue, it additionally tries to improve performance where possible by for example using more streamlined XML construction using XmlWriter directly into the HttpResponse output. A pre-configured web.config is included which tries to set the most optimum settings for the use of HttpUpload against both IIS6/IIS7 and .NET2/4.

Additionally requests for meta.xml (static file once a deployer has been initialized) are cached.

Further TcmHttpUpload can be configured to remove _stale_ state files. When publishing the Tridion deployer tends to leave fies which it either does not delete or is unable to delete due to file locking on the server.
These tend to build up over time and cause the HTTP responses from the !HttpUpload module back to the cd_transport service to grow. In our production environment with a high publishing load we have found this to adversely impact the performance.

Note that TcmHttpUpload currently does not support the .NET in-process deployer model.
I prefer to run the default out-of-process deployer (cd\_deployer as a service) as it seems to be more stable over long periods of time.

[![githalytics.com alpha](https://cruel-carlota.pagodabox.com/cb439853d159fe1de33e3197f1caf6f7 "githalytics.com")](http://githalytics.com/github.com/TcmExtensions)
