using System;
using System.Collections.Generic;
using System.Text;

namespace TheByteStuff.AzureTableBackupRestore
{
    public abstract class CommandLineBase
    {
        private Dictionary<string, string> Arguments = new Dictionary<string, string>();

        public string GetCommandLineParameterValue(string ParameterName)
        {
            return GetCommandLineParameterValue(ParameterName, "");
        }

        public string GetCommandLineParameterValue(string ParameterName, String defaultValue)
        {
            try
            {
                return Arguments[ParameterName];
            }
            catch (KeyNotFoundException)
            {
                return defaultValue;
            }
        }

        public bool IsParmOnInput(string ParameterName)
        {
            string parmValue = GetCommandLineParameterValue(ParameterName);
            bool isParm = false;
            if (!String.IsNullOrWhiteSpace(parmValue))
            {
                isParm = true;
            }
            return isParm;
        }

        public void StoreArguments(string[] Args)
        {
            if (Args != null)
            {
                foreach (string Arg in Args)
                {
                    try
                    {
                        string[] Param = Arg.Split('=');
                        if (Param.Length == 2)
                        {
                            Arguments.Add(Param[0], Param[1]);
                        }
                        else
                        {
                            //if command line argument parameter does not split into 2, just add it to the list
                            Arguments.Add(Arg, Arg);
                        }
                    }
                    catch (ArgumentException aex)
                    {
                        Console.WriteLine("error processing command line argument:" + Arg + ":" + aex.Message);
                        Console.WriteLine("error processing command line argument:" + Arg + ":" + aex.StackTrace);
                    }
                }
            }
        }

        public bool HasData(string Value)
        {
            return !string.IsNullOrWhiteSpace(Value);
        }


        public int CountChars(string Data, char CharValue)
        {
            int Result = 0;

            foreach (char C in Data)
            {
                if (C.Equals(CharValue))
                {
                    Result++;
                }
            }
            return Result;
        }
    }
}