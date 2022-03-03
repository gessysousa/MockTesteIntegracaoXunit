using System;
using System.Collections.Generic;
using System.Text;

namespace Alura.CoisasAFazer.Services.Handlers
{
    public class CommandResult
    {
        public bool IsSuccess { get; }

        public CommandResult(bool success)
        {
            IsSuccess = success;
        }
    }
}
