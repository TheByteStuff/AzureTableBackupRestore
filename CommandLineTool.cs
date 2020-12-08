using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Binder;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using TheByteStuff.AzureTableUtilities;

namespace TheByteStuff.AzureTableBackupRestore
{
    public class CommandLineTool : CommandLineBase
    {
        private readonly static string ConfigFileNameParmName = "ConfigFileName";
        private readonly static string ConfigFilePathParmName = "ConfigFilePath";
        private readonly static string AzureBlobStorageConfigConnectionParmName = "AzureBlobStorageConfigConnection";
        private readonly static string AzureStorageConfigConnectionParmName = "AzureStorageConfigConnection";
        private readonly static string AzureStorageConfigConnectionDestinationParmName = "AzureStorageDestinationConfigConnection";
        private readonly static string CommandNameParmName = "Command";
        private readonly static string TargetParmName = "Target";
        private readonly static string CompressParmName = "Compress";
        private readonly static string ValidateParmName = "Validate";
        private readonly static string RetentionDaysParmName = "RetentionDays";
        private readonly static string TimeoutSecondsParmName = "TimeoutSeconds";
        private readonly static string TableNameParmName = "TableName";
        private readonly static string DestinationTableNameParmName = "DestinationTableName";
        private readonly static string OriginalTableNameParmName = "OriginalTableName";
        private readonly static string TableNameToDeleteParmName = "TableNameToDelete";
        private readonly static string BlobRootParmName = "BlobRoot";
        private readonly static string BlobFileNameParmName = "BlobFileName";
        private readonly static string WorkingDirectoryParmName = "WorkingDirectory";
        private readonly static string OutFileDirectoryParmName = "OutFileDirectory";
        private readonly static string RestoreFileNamePathParmName = "RestoreFileNamePath";

        private readonly static string HelpParmName = "help";
        private readonly static string HelpParmName2 = "-h";

        private SecureString AzureBlobStorageConfigConnection = new SecureString();
        private SecureString AzureStorageConfigConnection = new SecureString();
        private SecureString AzureStorageDestinationConfigConnection = new SecureString();

        private void WriteOutput(string s)
        {
            Console.WriteLine(s);
        }

        private void DisplayHelp()
        {
            string[] ArgOptions = new string[] { ConfigFileNameParmName //0
                , ConfigFilePathParmName // 1
                , AzureBlobStorageConfigConnectionParmName //2
                , AzureStorageConfigConnectionParmName //3
                , CommandNameParmName //4
                , TargetParmName //5
                , CompressParmName //6 
                , ValidateParmName //7
                , RetentionDaysParmName //8
                , TimeoutSecondsParmName //9
                , TableNameParmName //10
                , DestinationTableNameParmName //11
                , OriginalTableNameParmName //12
                , BlobRootParmName //13
                , BlobFileNameParmName //14
                , WorkingDirectoryParmName //15
                , OutFileDirectoryParmName //16
                , RestoreFileNamePathParmName //17
                , AzureStorageConfigConnectionDestinationParmName //18 
                , TableNameToDeleteParmName  //19
            };

            WriteOutput("Backup, Copy, Delete and Restore Azure Tables.  Backup maybe to local system file or Azure blob storage.  Restore may be from local system file or from Azure blob storage.  Command line parameter values will override a value in the settings file.");
            WriteOutput("");
            WriteOutput(String.Format("AzureTableBackupRestore [{0}] [{1}] [{2}] [{3}] [{18}]  [{4}] [{5}] [{6}] [{7}] [{8}] [{9}] [{10}] [{11}] [{12}] [{13}] [{14}] [{15}] [{16}] [{17}]", ArgOptions));
            WriteOutput("");
            WriteOutput(String.Format("{0}=<Configuration File Name> (default appsettings.json)", ArgOptions));
            WriteOutput(String.Format("{1}=<Configuration File Path> (default current directory)", ArgOptions));
            WriteOutput(String.Format("{2}=<ConnectionSpec> (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{3}=<ConnectionSpec> (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{18}=<ConnectionSpec> (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{4}=<Backup|Copy|Delete|Restore>", ArgOptions));
            WriteOutput(String.Format("{5}=<Blob|File|Row|Table> (indicates source/destination for backup/restore or Row vs Table for delete)", ArgOptions));
            WriteOutput(String.Format("{5}=<ToBlob|ToFile|FromBlob|FromFile|Blob|File> (indicates source/destination for backup/restore)", ArgOptions));
            WriteOutput(String.Format("{6}=<true|false>, valid on backup (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{7}=<true|false> (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{8}=<int> Number of days to retain file, valid on backup (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{9}=<int> Azure call timeout in seconds (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{10}=<name of table>, valid on backup (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{11}=<Table name to restore or copy to>, valid on copy/restore (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{19}=<Table name to delete>, valid on delete (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{12}=<Name of Original Table as backed up>, valid on restore (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{13}=<Azure Root Blob> (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{14}=<Azure Blob File Name>, valid on restore only (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{15}=<Local directory with write permission> (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{16}=<Local directory for file output>, valid on backup (or use Configuration File)", ArgOptions));
            WriteOutput(String.Format("{17}=<Path and Name of file to restore>, valid on restore (or use Configuration File)", ArgOptions));
            WriteOutput("");
            WriteOutput(String.Format("{0} and {1} are command line only, all other parms may be specified in config file as well.", ArgOptions));

            //WriteOutput("see more at...");
        }


        public static void Main(string[] args)
        {
            CommandLineTool instance = new CommandLineTool();
            instance.StoreArguments(args);

            try
            {
                if (instance.IsParmOnInput(HelpParmName) || instance.IsParmOnInput(HelpParmName2))
                {
                    instance.DisplayHelp();
                }
                else
                {
                    instance.WriteOutput(String.Format("using configuration file path '{0}'", instance.GetCommandLineParameterValue(ConfigFilePathParmName, Directory.GetCurrentDirectory())));
                    instance.WriteOutput(String.Format("using configuration file name '{0}'", instance.GetCommandLineParameterValue(ConfigFileNameParmName, "appsettings.json")));
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(instance.GetCommandLineParameterValue(ConfigFilePathParmName, Directory.GetCurrentDirectory()))
                        .AddJsonFile(instance.GetCommandLineParameterValue(ConfigFileNameParmName, "appsettings.json"));
                    IConfiguration config = builder.Build();

                    foreach (char c in instance.GetCommandLineParameterValue(AzureStorageConfigConnectionParmName, Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(config, AzureStorageConfigConnectionParmName)).ToCharArray())
                    {
                        instance.AzureStorageConfigConnection.AppendChar(c);
                    }
                    foreach (char c in instance.GetCommandLineParameterValue(AzureStorageConfigConnectionDestinationParmName, Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(config, AzureStorageConfigConnectionDestinationParmName)).ToCharArray())
                    {
                        instance.AzureStorageDestinationConfigConnection.AppendChar(c);
                    }
                    foreach (char c in instance.GetCommandLineParameterValue(AzureBlobStorageConfigConnectionParmName, Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(config, AzureBlobStorageConfigConnectionParmName)).ToCharArray())
                    {
                        instance.AzureBlobStorageConfigConnection.AppendChar(c);
                    }

                    string Command = instance.GetFromParmOrFile(config, CommandNameParmName);
                    if ("backup".Equals(Command) || "copy".Equals(Command) || "restore".Equals(Command) || "delete".Equals(Command))
                    {
                        if ("backup".Equals(Command))
                        {
                            instance.WriteOutput("Beginning backup process.");
                            instance.WriteOutput(String.Format("{0}", instance.Backup(instance.AzureBlobStorageConfigConnection, instance.AzureStorageConfigConnection, config)));
                        }
                        else if ("copy".Equals(Command))
                        {
                            instance.WriteOutput("Beginning copy process.");
                            instance.WriteOutput(String.Format("{0}", instance.Copy(instance.AzureStorageConfigConnection, instance.AzureStorageDestinationConfigConnection, config)));
                        }
                        else if ("delete".Equals(Command))
                        {
                            instance.WriteOutput("Beginning delete process.");
                            instance.WriteOutput(String.Format("{0}", instance.Delete(instance.AzureStorageConfigConnection, config)));
                        }
                        else
                        {
                            instance.WriteOutput("Beginning restore process.");
                            instance.WriteOutput(String.Format("{0}", instance.Restore(instance.AzureBlobStorageConfigConnection, instance.AzureStorageConfigConnection, config)));
                        }
                        instance.WriteOutput("AzureTableUtilities ending.");
                    }
                    else
                    {
                        instance.WriteOutput(String.Format("Valid {0} values are 'backup', 'copy, 'delete' and 'restore'", CommandNameParmName));
                        instance.DisplayHelp();
                    }
                }
            }
            catch (Exception ex)
            {
                instance.DisplayHelp();
                instance.WriteOutput(" ");
                instance.WriteOutput(String.Format("Exception: {0}, {1}", ex.Message, ex.StackTrace));
            }
        }

        private string GetFromParmOrFile(IConfiguration config, string ParmName)
        {
            return this.GetCommandLineParameterValue(ParmName, config[ParmName]);
        }

        private bool GetBoolFromParmOrFile(IConfiguration config, string ParmName)
        {
            WriteOutput(String.Format("Parsing {0}", ParmName));
            return "true".Equals(GetFromParmOrFile(config, ParmName).ToLower());
        }

        private int GetIntFromParmOrFile(IConfiguration config, string ParmName)
        {
            WriteOutput(String.Format("Parsing {0}", ParmName));
            return int.Parse(GetFromParmOrFile(config, ParmName));
        }

        private string Restore(SecureString AzureBlobStorageConfigConnection, SecureString AzureStorageConfigConnection, IConfiguration config)
        {
            try
            {
                RestoreAzureTables me = new RestoreAzureTables(AzureStorageConfigConnection, AzureBlobStorageConfigConnection);

                string Target = GetFromParmOrFile(config, TargetParmName).ToLower();
                if (!String.IsNullOrEmpty(Target))
                {
                    if (Target.Contains("file"))
                    {
                        return me.RestoreTableFromFile(GetFromParmOrFile(config, DestinationTableNameParmName),
                            GetFromParmOrFile(config, RestoreFileNamePathParmName),
                            GetIntFromParmOrFile(config, TimeoutSecondsParmName));
                    }
                    else if (Target.Contains("blob"))
                    {
                        return me.RestoreTableFromBlob(GetFromParmOrFile(config, DestinationTableNameParmName),
                            GetFromParmOrFile(config, OriginalTableNameParmName),
                            GetFromParmOrFile(config, BlobRootParmName),
                            GetFromParmOrFile(config, WorkingDirectoryParmName),
                            GetFromParmOrFile(config, BlobFileNameParmName),
                            GetIntFromParmOrFile(config, TimeoutSecondsParmName));
                    }
                    else
                    {
                        throw new Exception("Missing or invalid configuration for requested command.");
                    }
                }
                else
                {
                    throw new Exception("Missing or invalid configuration for requested command.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string Backup(SecureString AzureBlobStorageConfigConnection, SecureString AzureStorageConfigConnection, IConfiguration config)
        {
            try
            {
                BackupAzureTables me = new BackupAzureTables(AzureStorageConfigConnection, AzureBlobStorageConfigConnection);

                string Target = GetFromParmOrFile(config, TargetParmName).ToLower();
                if (!String.IsNullOrEmpty(Target))
                {
                    var sectionFilters = config.GetSection("Filters");
                    List<Filter> filters = sectionFilters.Get<List<Filter>>();

                    if (!Filter.AreFiltersValid(filters))
                    {
                        throw new Exception("One or more of the supplied filter cirteria is invalid.");
                    }

                    if (Target.Contains("file"))
                    {
                        return me.BackupTableToFile(GetFromParmOrFile(config, TableNameParmName),
                            GetFromParmOrFile(config, OutFileDirectoryParmName),
                            GetBoolFromParmOrFile(config, CompressParmName),
                            GetBoolFromParmOrFile(config, ValidateParmName),
                            GetIntFromParmOrFile(config, TimeoutSecondsParmName),
                            filters);
                    }
                    else if (Target.Contains("blob"))
                    {
                        return me.BackupTableToBlob(GetFromParmOrFile(config, TableNameParmName),
                            GetFromParmOrFile(config, BlobRootParmName),
                            GetFromParmOrFile(config, OutFileDirectoryParmName),
                            GetBoolFromParmOrFile(config, CompressParmName),
                            GetBoolFromParmOrFile(config, ValidateParmName),
                            GetIntFromParmOrFile(config, RetentionDaysParmName),
                            GetIntFromParmOrFile(config, TimeoutSecondsParmName),
                            filters);
                    }
                    else
                    {
                        throw new Exception("Missing or invalid configuration for requested command.");
                    }
                }
                else
                {
                    throw new Exception("Missing or invalid configuration for requested command.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string Copy(SecureString AzureStorageSourceConfigConnection, SecureString AzureStorageDestinationConfigConnection, IConfiguration config)
        {
            try
            {
                CopyAzureTables me = new CopyAzureTables(AzureStorageSourceConfigConnection, AzureStorageDestinationConfigConnection);

                var sectionFilters = config.GetSection("Filters");
                List<Filter> filters = sectionFilters.Get<List<Filter>>();

                if (!Filter.AreFiltersValid(filters))
                {
                    throw new Exception("One or more of the supplied filter cirteria is invalid.");
                }

                return me.CopyTableToTable(GetFromParmOrFile(config, TableNameParmName),
                    GetFromParmOrFile(config, DestinationTableNameParmName),
                    GetIntFromParmOrFile(config, TimeoutSecondsParmName),
                    filters);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string Delete(SecureString AzureBlobStorageConfigConnection, IConfiguration config)
        {
            try
            {
                DeleteAzureTables me = new DeleteAzureTables(AzureStorageConfigConnection);

                string Target = GetFromParmOrFile(config, TargetParmName).ToLower();
                if (!String.IsNullOrEmpty(Target))
                {
                    var sectionFilters = config.GetSection("Filters");
                    List<Filter> filters = sectionFilters.Get<List<Filter>>();

                    if (!Filter.AreFiltersValid(filters))
                    {
                        throw new Exception("One or more of the supplied filter cirteria is invalid.");
                    }

                    if (Target.Contains("rows"))
                    {
                        return me.DeleteAzureTableRows(GetFromParmOrFile(config, TableNameToDeleteParmName)
                         , GetIntFromParmOrFile(config, TimeoutSecondsParmName)
                         ,filters
                         );
                    }
                    else if (Target.Contains("table"))
                    {
                        return me.DeleteAzureTable(GetFromParmOrFile(config, TableNameToDeleteParmName)
                         , GetIntFromParmOrFile(config, TimeoutSecondsParmName)
                         );
                    }
                    else
                    {
                        throw new Exception("Missing or invalid configuration for requested command.");
                    }
                }
                else
                {
                    throw new Exception("Missing or invalid configuration for requested command.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}