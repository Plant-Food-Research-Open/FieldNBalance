using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace SVSModel.Configuration
{
    public class Tryout
    {
        public string message;
        public int code;

        // Created a class constructor with multiple parameters
        // for some reason 
        public Tryout(string messageName, int codeNum)
        {
            message = messageName;
            code = codeNum;
        }
        
    }
}


