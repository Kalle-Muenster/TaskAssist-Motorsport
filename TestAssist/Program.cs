using System;
using System.Collections.Generic;
using Consola;
using Consola.Test;

namespace TaskAssist.Tests {

    public class Program {
        public static int Main( string[] args )
        {
            List<string> Args = new List<string>( args );

            TestResults resultflags = TestResults.TextOutput;
            if( Args.Contains("-v") | Args.Contains("--verbose") )
                resultflags |= TestResults.Verbose;
            if( Args.Contains("-x") | Args.Contains("--xml") )
                resultflags |= TestResults.XmlOutput; 

            Test test = new Motortests( resultflags ).Run();

            return test.wasErrors() ? -1
                 : test.hasPassed() ? 0
                 : test.getFailures();
        }
    }
}
