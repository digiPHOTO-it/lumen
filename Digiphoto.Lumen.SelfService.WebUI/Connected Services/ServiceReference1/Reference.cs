﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Digiphoto.Lumen.SelfService.WebUI.ServiceReference1 {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="CarrelloDto", Namespace="http://schemas.datacontract.org/2004/07/Digiphoto.Lumen.SelfService.Carrelli")]
    [System.SerializableAttribute()]
    public partial class CarrelloDto : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid idField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private bool isVendutoField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string titoloField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid id {
            get {
                return this.idField;
            }
            set {
                if ((this.idField.Equals(value) != true)) {
                    this.idField = value;
                    this.RaisePropertyChanged("id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public bool isVenduto {
            get {
                return this.isVendutoField;
            }
            set {
                if ((this.isVendutoField.Equals(value) != true)) {
                    this.isVendutoField = value;
                    this.RaisePropertyChanged("isVenduto");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string titolo {
            get {
                return this.titoloField;
            }
            set {
                if ((object.ReferenceEquals(this.titoloField, value) != true)) {
                    this.titoloField = value;
                    this.RaisePropertyChanged("titolo");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="FotografiaDto", Namespace="http://schemas.datacontract.org/2004/07/Digiphoto.Lumen.SelfService.Carrelli")]
    [System.SerializableAttribute()]
    public partial class FotografiaDto : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string etichettaField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid idField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Nullable<bool> miPiaceField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string etichetta {
            get {
                return this.etichettaField;
            }
            set {
                if ((object.ReferenceEquals(this.etichettaField, value) != true)) {
                    this.etichettaField = value;
                    this.RaisePropertyChanged("etichetta");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid id {
            get {
                return this.idField;
            }
            set {
                if ((this.idField.Equals(value) != true)) {
                    this.idField = value;
                    this.RaisePropertyChanged("id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Nullable<bool> miPiace {
            get {
                return this.miPiaceField;
            }
            set {
                if ((this.miPiaceField.Equals(value) != true)) {
                    this.miPiaceField = value;
                    this.RaisePropertyChanged("miPiace");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="FotografoDto", Namespace="http://schemas.datacontract.org/2004/07/Digiphoto.Lumen.SelfService.Carrelli")]
    [System.SerializableAttribute()]
    public partial class FotografoDto : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string idField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private byte[] immagineField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string nomeField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string id {
            get {
                return this.idField;
            }
            set {
                if ((object.ReferenceEquals(this.idField, value) != true)) {
                    this.idField = value;
                    this.RaisePropertyChanged("id");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public byte[] immagine {
            get {
                return this.immagineField;
            }
            set {
                if ((object.ReferenceEquals(this.immagineField, value) != true)) {
                    this.immagineField = value;
                    this.RaisePropertyChanged("immagine");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string nome {
            get {
                return this.nomeField;
            }
            set {
                if ((object.ReferenceEquals(this.nomeField, value) != true)) {
                    this.nomeField = value;
                    this.RaisePropertyChanged("nome");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="ServiceReference1.ISelfService")]
    public interface ISelfService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaCarrelli", ReplyAction="http://tempuri.org/ISelfService/getListaCarrelliResponse")]
        Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto[] getListaCarrelli();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaCarrelli", ReplyAction="http://tempuri.org/ISelfService/getListaCarrelliResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto[]> getListaCarrelliAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getCarrello", ReplyAction="http://tempuri.org/ISelfService/getCarrelloResponse")]
        Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto getCarrello(System.Guid carrelloId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getCarrello", ReplyAction="http://tempuri.org/ISelfService/getCarrelloResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto> getCarrelloAsync(System.Guid carrelloId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografie", ReplyAction="http://tempuri.org/ISelfService/getListaFotografieResponse")]
        Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[] getListaFotografie(System.Guid carrelloId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografie", ReplyAction="http://tempuri.org/ISelfService/getListaFotografieResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[]> getListaFotografieAsync(System.Guid carrelloId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImage", ReplyAction="http://tempuri.org/ISelfService/getImageResponse")]
        byte[] getImage(System.Guid fotografiaId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImage", ReplyAction="http://tempuri.org/ISelfService/getImageResponse")]
        System.Threading.Tasks.Task<byte[]> getImageAsync(System.Guid fotografiaId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImageProvino", ReplyAction="http://tempuri.org/ISelfService/getImageProvinoResponse")]
        byte[] getImageProvino(System.Guid fotografiaId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImageProvino", ReplyAction="http://tempuri.org/ISelfService/getImageProvinoResponse")]
        System.Threading.Tasks.Task<byte[]> getImageProvinoAsync(System.Guid fotografiaId);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImageLogo", ReplyAction="http://tempuri.org/ISelfService/getImageLogoResponse")]
        byte[] getImageLogo();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getImageLogo", ReplyAction="http://tempuri.org/ISelfService/getImageLogoResponse")]
        System.Threading.Tasks.Task<byte[]> getImageLogoAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/setMiPiace", ReplyAction="http://tempuri.org/ISelfService/setMiPiaceResponse")]
        void setMiPiace(System.Guid fotografiaId, bool miPiace);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/setMiPiace", ReplyAction="http://tempuri.org/ISelfService/setMiPiaceResponse")]
        System.Threading.Tasks.Task setMiPiaceAsync(System.Guid fotografiaId, bool miPiace);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografi", ReplyAction="http://tempuri.org/ISelfService/getListaFotografiResponse")]
        Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografoDto[] getListaFotografi();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografi", ReplyAction="http://tempuri.org/ISelfService/getListaFotografiResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografoDto[]> getListaFotografiAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografieDelFotografo", ReplyAction="http://tempuri.org/ISelfService/getListaFotografieDelFotografoResponse")]
        Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[] getListaFotografieDelFotografo(string fotografoId, int skip, int take);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getListaFotografieDelFotografo", ReplyAction="http://tempuri.org/ISelfService/getListaFotografieDelFotografoResponse")]
        System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[]> getListaFotografieDelFotografoAsync(string fotografoId, int skip, int take);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getSettings", ReplyAction="http://tempuri.org/ISelfService/getSettingsResponse")]
        System.Collections.Generic.Dictionary<string, string> getSettings();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ISelfService/getSettings", ReplyAction="http://tempuri.org/ISelfService/getSettingsResponse")]
        System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, string>> getSettingsAsync();
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ISelfServiceChannel : Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.ISelfService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class SelfServiceClient : System.ServiceModel.ClientBase<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.ISelfService>, Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.ISelfService {
        
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
        
        public Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto[] getListaCarrelli() {
            return base.Channel.getListaCarrelli();
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto[]> getListaCarrelliAsync() {
            return base.Channel.getListaCarrelliAsync();
        }
        
        public Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto getCarrello(System.Guid carrelloId) {
            return base.Channel.getCarrello(carrelloId);
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.CarrelloDto> getCarrelloAsync(System.Guid carrelloId) {
            return base.Channel.getCarrelloAsync(carrelloId);
        }
        
        public Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[] getListaFotografie(System.Guid carrelloId) {
            return base.Channel.getListaFotografie(carrelloId);
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[]> getListaFotografieAsync(System.Guid carrelloId) {
            return base.Channel.getListaFotografieAsync(carrelloId);
        }
        
        public byte[] getImage(System.Guid fotografiaId) {
            return base.Channel.getImage(fotografiaId);
        }
        
        public System.Threading.Tasks.Task<byte[]> getImageAsync(System.Guid fotografiaId) {
            return base.Channel.getImageAsync(fotografiaId);
        }
        
        public byte[] getImageProvino(System.Guid fotografiaId) {
            return base.Channel.getImageProvino(fotografiaId);
        }
        
        public System.Threading.Tasks.Task<byte[]> getImageProvinoAsync(System.Guid fotografiaId) {
            return base.Channel.getImageProvinoAsync(fotografiaId);
        }
        
        public byte[] getImageLogo() {
            return base.Channel.getImageLogo();
        }
        
        public System.Threading.Tasks.Task<byte[]> getImageLogoAsync() {
            return base.Channel.getImageLogoAsync();
        }
        
        public void setMiPiace(System.Guid fotografiaId, bool miPiace) {
            base.Channel.setMiPiace(fotografiaId, miPiace);
        }
        
        public System.Threading.Tasks.Task setMiPiaceAsync(System.Guid fotografiaId, bool miPiace) {
            return base.Channel.setMiPiaceAsync(fotografiaId, miPiace);
        }
        
        public Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografoDto[] getListaFotografi() {
            return base.Channel.getListaFotografi();
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografoDto[]> getListaFotografiAsync() {
            return base.Channel.getListaFotografiAsync();
        }
        
        public Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[] getListaFotografieDelFotografo(string fotografoId, int skip, int take) {
            return base.Channel.getListaFotografieDelFotografo(fotografoId, skip, take);
        }
        
        public System.Threading.Tasks.Task<Digiphoto.Lumen.SelfService.WebUI.ServiceReference1.FotografiaDto[]> getListaFotografieDelFotografoAsync(string fotografoId, int skip, int take) {
            return base.Channel.getListaFotografieDelFotografoAsync(fotografoId, skip, take);
        }
        
        public System.Collections.Generic.Dictionary<string, string> getSettings() {
            return base.Channel.getSettings();
        }
        
        public System.Threading.Tasks.Task<System.Collections.Generic.Dictionary<string, string>> getSettingsAsync() {
            return base.Channel.getSettingsAsync();
        }
    }
}
