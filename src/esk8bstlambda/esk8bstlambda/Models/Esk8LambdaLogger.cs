using Amazon.Lambda.Core;
using Esk8Bst.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace esk8bstlambda.Models
{
    internal class Esk8LambdaLogger : ILogger {
        private readonly ILambdaLogger Logger;
        
        public Esk8LambdaLogger(ILambdaLogger logger) {
            this.Logger = logger;
        }

        public void Log(string line) {
            this.Logger.Log(line);
        }
    }
}
