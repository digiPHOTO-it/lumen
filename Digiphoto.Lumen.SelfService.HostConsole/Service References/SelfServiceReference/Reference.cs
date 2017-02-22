﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Digiphoto.Lumen.SelfService.HostConsole.SelfServiceReference {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="SelfServiceReference.ISelfService")]
    public interface ISelfService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaCarrelli", ReplyAction="http://tempuri.org/ISelfService/getListaCarrelliResponse")]
        Digiphoto.Lumen.SelfService.Carrelli.CarrelloDto[] getListaCarrelli();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaCarrelli", ReplyAction="http://tempuri.org/ISelfService/getListaCarrelliResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.Carrelli.CarrelloDto[]> getListaCarrelliAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografie", ReplyAction="http://tempuri.org/ISelfService/getListaFotografieResponse")]
        Digiphoto.Lumen.SelfService.Carrelli.FotografiaDto[] getListaFotografie(System.Guid carrelloId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografie", ReplyAction="http://tempuri.org/ISelfService/getListaFotografieResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.Carrelli.FotografiaDto[]> getListaFotografieAsync(System.Guid carrelloId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImage", ReplyAction="http://tempuri.org/ISelfService/getImageResponse")]
        byte[] getImage(System.Guid fotografiaId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImage", ReplyAction="http://tempuri.org/ISelfService/getImageResponse")]
        System.Threading.Tasks.Task<byte[]> getImageAsync(System.Guid fotografiaId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/setMiPiace", ReplyAction="http://tempuri.org/ISelfService/setMiPiaceResponse")]
        void setMiPiace(System.Guid fotografiaId, bool miPiace);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/setMiPiace", ReplyAction="http://tempuri.org/ISelfService/setMiPiaceResponse")]
        System.Threading.Tasks.Task setMiPiaceAsync(System.Guid fotografiaId, bool miPiace);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISelfServiceChannel : Digiphoto.Lumen.SelfService.HostConsole.SelfServiceReference.ISelfService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SelfServiceClient : System.ServiceModel.ClientBase<Digiphoto.Lumen.SelfService.HostConsole.SelfServiceReference.ISelfService>, Digiphoto.Lumen.SelfService.HostConsole.SelfServiceReference.ISelfService {
        
        public SelfServiceClient() {
        }
        
        public SelfServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public SelfServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SelfServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public SelfServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Digiphoto.Lumen.SelfService.Carrelli.CarrelloDto[] getListaCarrelli() {
            return base.Channel.getListaCarrelli();
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.Carrelli.CarrelloDto[]> getListaCarrelliAsync() {
            return base.Channel.getListaCarrelliAsync();
        }
        
        public Digiphoto.Lumen.SelfService.Carrelli.FotografiaDto[] getListaFotografie(System.Guid carrelloId) {
            return base.Channel.getListaFotografie(carrelloId);
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.Carrelli.FotografiaDto[]> getListaFotografieAsync(System.Guid carrelloId) {
            return base.Channel.getListaFotografieAsync(carrelloId);
        }
        
        public byte[] getImage(System.Guid fotografiaId) {
            return base.Channel.getImage(fotografiaId);
        }
        
        public System.Threading.Tasks.Task<byte[]> getImageAsync(System.Guid fotografiaId) {
            return base.Channel.getImageAsync(fotografiaId);
        }
        
        public void setMiPiace(System.Guid fotografiaId, bool miPiace) {
            base.Channel.setMiPiace(fotografiaId, miPiace);
        }
        
        public System.Threading.Tasks.Task setMiPiaceAsync(System.Guid fotografiaId, bool miPiace) {
            return base.Channel.setMiPiaceAsync(fotografiaId, miPiace);
        }
    }
}