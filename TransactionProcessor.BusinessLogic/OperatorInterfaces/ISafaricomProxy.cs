using System;
using System.Collections.Generic;
using System.Text;

namespace TransactionProcessor.BusinessLogic.OperatorInterfaces
{
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IOperatorProxy
    {
        Task ProcessMessage(CancellationToken cancellationToken);
    }

    [ExcludeFromCodeCoverage]
    public class SafaricomPinlessProxy : IOperatorProxy
    {
        public async Task ProcessMessage(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    /*http://{{linuxservername}}:8000/api/safaricom?
     VENDOR=D-S136 - Fixed
     &REQTYPE=EXRCTRFREQ - Fixed
     &DATA=
     <?xml version="1.0" encoding="utf-8"?>
        <ns0:COMMAND xmlns:ns0="http://safaricom.co.ke/Pinless/keyaccounts/">
            <ns0:TYPE>EXRCTRFREQ</ns0:TYPE> - Fixed
            <ns0:DATE>02-JUL-2018</ns0:DATE> - From Sales Transaction 
            <ns0:EXTNWCODE>SA</ns0:EXTNWCODE> - Fixed
            <ns0:MSISDN>700945625</ns0:MSISDN> - Config
            <ns0:PIN>0322</ns0:PIN> - Config
            <ns0:LOGINID>D-S136</ns0:LOGINID> - Config
            <ns0:PASSWORD>@SafePay33</ns0:PASSWORD> - Config
            <ns0:EXTCODE>D-S136</ns0:EXTCODE> - Config ?
            <ns0:EXTREFNUM>100022814</ns0:EXTREFNUM> - From Sales Transaction 
            <ns0:MSISDN2>0723581504</ns0:MSISDN2> - From Sales Transaction 
            <ns0:AMOUNT>65000</ns0:AMOUNT> - From Sales Transaction 
            <ns0:LANGUAGE1>0</ns0:LANGUAGE1> - Fixed
            <ns0:LANGUAGE2>0</ns0:LANGUAGE2> - Fixed
            <ns0:SELECTOR>1</ns0:SELECTOR> - Fixed
        </ns0:COMMAND>*/
}
