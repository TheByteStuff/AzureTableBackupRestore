# AzureTableBackupRestore
Reference implementation of a command line tool for backup, copy, delete and restore of Azure tables to/from local file or Azure blob storage using TheByteStuff.AzureTableUtilities found on NuGet: https://www.nuget.org/packages/TheByteStuff.AzureTableUtilities/

Backup/Copy/Delete/Restore parameters can be stored in a settings file (appsettings.json) or passed on the command line.

Filters need to be specified in the settings file.

Has been tested under Docker on a Mac with the included Dockerfile

ex to run assuming build has image name 'azcmd'
docker run -v /localfile/appsettings.json:/app/appsettings.json -v /localfile:/app/data azcmd
