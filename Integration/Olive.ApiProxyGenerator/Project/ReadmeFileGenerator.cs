using System;
using System.Collections.Generic;
using System.Text;

namespace Olive.ApiProxy
{
    internal class ReadmeFileGenerator
    {
        internal static string Generate()
        {
            var content = @"
Sample Readme Content ...

Owners
GeeksLtd

Authors
Geeks Ltd

Copyright
Copyright ©2018 Geeks Ltd - All rights reserved.";
            return content;
        }

    }
}
