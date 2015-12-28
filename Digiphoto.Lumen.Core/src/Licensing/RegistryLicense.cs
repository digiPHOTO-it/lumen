using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Digiphoto.Lumen.Licensing {

	public class RegistryLicense : License {

		private SKGL.Validate _validate;


		public RegistryLicense( string licenceKey ) {

			if( licenceKey == null )
				throw new NullReferenceException( "The licensed type reference may not be null." );

			_validate = new SKGL.Validate();
			_validate.secretPhase = "C0t3ch1n0";
			_validate.Key = licenceKey;

			if( ! _validate.IsValid )
				throw new ApplicationException( "The licence code is not valid" );
		}

		public override string LicenseKey {
			get {
				return this._validate.Key;
			}
		}

		public bool IsExpired {
			get {
				return _validate.IsExpired;
			}
		}

		public bool IsOnRightMachine {
			get {
				return _validate.IsOnRightMachine;
			}
		}

		public DateTime CreationDate {
			get {
				return _validate.CreationDate;
			}
		}

		public int DaysLeft {
			get {
				return _validate.DaysLeft;
			}
		}

		public DateTime ExpireDate {
			get {
				return _validate.ExpireDate;
			}
		}

		public override void Dispose() {
		}
	}
}
