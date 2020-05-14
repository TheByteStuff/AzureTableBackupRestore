# AzureTableBackupRestore
Command line tool for backup and restore of Azure tables to/from local file or Azure blob storage

Backup/restore parameters can be stored in a settings file (appsettings.json) or passed on the command line.




Has been tested under Docker on a Mac with the included Dockerfile

ex to run assuming build has image name 'azcmd'
docker run -v /localfile/appsettings.json:/app/appsettings.json -v /localfile:/app/data azcmd

