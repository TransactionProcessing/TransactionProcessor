using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.DataTransferObjects.Requests;

public class RequestWithHostDateTime {
    public DateTimeOffset HostDateTime { get; set; }
}
