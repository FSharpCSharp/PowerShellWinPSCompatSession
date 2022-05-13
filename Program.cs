// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Microsoft.PowerShell;
using MyApp.Properties;

namespace MyApp;

public class Program
{
    /// <summary>
    ///     Managed entry point shim, which starts the actual program.
    /// </summary>
    public static int Main(string[] args)
    {
        Console.WriteLine(Resources.Program_Main_Start_Execution);
        var baseDir = Path.Combine(Environment.CurrentDirectory, "Scripts");
        var fileName = Path.Combine(baseDir, "Test.ps1");
        // CreateDefault2 is intentional.
        var iss = InitialSessionState.CreateDefault();
        iss.ExecutionPolicy = ExecutionPolicy.Bypass;

        // NOTE: instantiate custom host myHost for the next line to capture stdout and stderr output
        //       in addition to just the PSObjects
        using (var runspace = RunspaceFactory.CreateRunspace(iss))
        {
            runspace.Open();
            //runspace.SessionStateProxy.Path.SetLocation(baseDir);

            using (var powerShell = PowerShell.Create())
            {
                powerShell.Runspace = runspace;
                //powerShell.AddCommand(".\\Test.ps1");

                powerShell.AddCommand(fileName);

                powerShell.Streams.Error.DataAdded += Error_DataAdded;
                powerShell.Streams.Warning.DataAdded += Warning_DataAdded;
                powerShell.Streams.Information.DataAdded += Information_DataAdded;

                var results = powerShell.Invoke();

                foreach (var result in results)
                    Console.WriteLine(result.BaseObject.ToString());
            }

            runspace.Close();
        }

        return 0;
    }

    private static IEnumerable<string> TrimAndSplitStreamData(string data)
    {
        return data.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
    }

    private static void Information_DataAdded(object? sender, DataAddedEventArgs e)
    {
        if (sender is not PSDataCollection<InformationRecord> streamObjectsReceived) 
            return;

        var currentStreamRecord = streamObjectsReceived[e.Index];
        var splittedData = TrimAndSplitStreamData(currentStreamRecord.MessageData.ToString());
        var data = splittedData.ToList();
        foreach (var item in data)
            Console.WriteLine(item);
    }

    private static void Warning_DataAdded(object? sender, DataAddedEventArgs e)
    {
        if (sender is not PSDataCollection<WarningRecord> streamObjectsReceived) 
            return;

        var currentStreamRecord = streamObjectsReceived[e.Index];

        var splittedData = TrimAndSplitStreamData(currentStreamRecord.Message);
        var data = splittedData.ToList();
        foreach (var item in data)
            Console.WriteLine(item);
    }

    private static void Error_DataAdded(object? sender, DataAddedEventArgs e)
    {
        if (sender is not PSDataCollection<ErrorRecord> streamObjectsReceived) 
            return;
        var currentStreamRecord = streamObjectsReceived[e.Index];
        Console.WriteLine(currentStreamRecord.Exception);
    }
}