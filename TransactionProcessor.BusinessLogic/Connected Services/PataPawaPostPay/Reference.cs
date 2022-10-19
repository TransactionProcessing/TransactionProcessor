﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PataPawaPostPay
{
    using System.Runtime.Serialization;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.3")]
    [System.Runtime.Serialization.DataContractAttribute(Name="login", Namespace="http://schemas.datacontract.org/2004/07/TestHosts.SoapServices.DataTransferObject" +
        "s")]
    public partial class login : object
    {
        
        private string api_keyField;
        
        private decimal balanceField;
        
        private string messageField;
        
        private int statusField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string api_key
        {
            get
            {
                return this.api_keyField;
            }
            set
            {
                this.api_keyField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public decimal balance
        {
            get
            {
                return this.balanceField;
            }
            set
            {
                this.balanceField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.3")]
    [System.Runtime.Serialization.DataContractAttribute(Name="verify", Namespace="http://schemas.datacontract.org/2004/07/TestHosts.SoapServices.DataTransferObject" +
        "s")]
    public partial class verify : object
    {
        
        private decimal account_balanceField;
        
        private string account_nameField;
        
        private string account_noField;
        
        private System.DateTime due_dateField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public decimal account_balance
        {
            get
            {
                return this.account_balanceField;
            }
            set
            {
                this.account_balanceField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string account_name
        {
            get
            {
                return this.account_nameField;
            }
            set
            {
                this.account_nameField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string account_no
        {
            get
            {
                return this.account_noField;
            }
            set
            {
                this.account_noField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.DateTime due_date
        {
            get
            {
                return this.due_dateField;
            }
            set
            {
                this.due_dateField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.3")]
    [System.Runtime.Serialization.DataContractAttribute(Name="paybill", Namespace="http://schemas.datacontract.org/2004/07/TestHosts.SoapServices.DataTransferObject" +
        "s")]
    public partial class paybill : object
    {
        
        private string agent_idField;
        
        private string msgField;
        
        private string receipt_noField;
        
        private string rescodeField;
        
        private string sms_idField;
        
        private int statusField;
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string agent_id
        {
            get
            {
                return this.agent_idField;
            }
            set
            {
                this.agent_idField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string msg
        {
            get
            {
                return this.msgField;
            }
            set
            {
                this.msgField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string receipt_no
        {
            get
            {
                return this.receipt_noField;
            }
            set
            {
                this.receipt_noField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string rescode
        {
            get
            {
                return this.rescodeField;
            }
            set
            {
                this.rescodeField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string sms_id
        {
            get
            {
                return this.sms_idField;
            }
            set
            {
                this.sms_idField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public int status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.3")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="PataPawaPostPay.IPataPawaPostPayService")]
    public interface IPataPawaPostPayService
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPataPawaPostPayService/getLoginRequest", ReplyAction="getLoginResponse")]
        System.Threading.Tasks.Task<PataPawaPostPay.login> getLoginRequestAsync(string username, string password);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPataPawaPostPayService/getVerifyRequest", ReplyAction="getVerifyResponse")]
        System.Threading.Tasks.Task<PataPawaPostPay.verify> getVerifyRequestAsync(string username, string api_key, string account_no);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IPataPawaPostPayService/getPayBillRequest", ReplyAction="getPayBillResponse")]
        System.Threading.Tasks.Task<PataPawaPostPay.paybill> getPayBillRequestAsync(string username, string api_key, string account_no, string mobile_no, string customer_name, decimal amount);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.3")]
    public interface IPataPawaPostPayServiceChannel : PataPawaPostPay.IPataPawaPostPayService, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.3")]
    public partial class PataPawaPostPayServiceClient : System.ServiceModel.ClientBase<PataPawaPostPay.IPataPawaPostPayService>, PataPawaPostPay.IPataPawaPostPayService
    {
        
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public PataPawaPostPayServiceClient() : 
                base(PataPawaPostPayServiceClient.GetDefaultBinding(), PataPawaPostPayServiceClient.GetDefaultEndpointAddress())
        {
            this.Endpoint.Name = EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PataPawaPostPayServiceClient(EndpointConfiguration endpointConfiguration) : 
                base(PataPawaPostPayServiceClient.GetBindingForEndpoint(endpointConfiguration), PataPawaPostPayServiceClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PataPawaPostPayServiceClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(PataPawaPostPayServiceClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PataPawaPostPayServiceClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(PataPawaPostPayServiceClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public PataPawaPostPayServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<PataPawaPostPay.login> getLoginRequestAsync(string username, string password)
        {
            return base.Channel.getLoginRequestAsync(username, password);
        }
        
        public System.Threading.Tasks.Task<PataPawaPostPay.verify> getVerifyRequestAsync(string username, string api_key, string account_no)
        {
            return base.Channel.getVerifyRequestAsync(username, api_key, account_no);
        }
        
        public System.Threading.Tasks.Task<PataPawaPostPay.paybill> getPayBillRequestAsync(string username, string api_key, string account_no, string mobile_no, string customer_name, decimal amount)
        {
            return base.Channel.getPayBillRequestAsync(username, api_key, account_no, mobile_no, customer_name, amount);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService))
            {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService))
            {
                return new System.ServiceModel.EndpointAddress("http://[::]:9000/PataPawaPostPayService/basichttp");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return PataPawaPostPayServiceClient.GetBindingForEndpoint(EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return PataPawaPostPayServiceClient.GetEndpointAddress(EndpointConfiguration.BasicHttpBinding_IPataPawaPostPayService);
        }
        
        public enum EndpointConfiguration
        {
            
            BasicHttpBinding_IPataPawaPostPayService,
        }
    }
}