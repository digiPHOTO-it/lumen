using System;
using System.ComponentModel;

namespace Digiphoto.Lumen.Model {

	/** Notare che i caratteri sono ANCHE in ordine alfabetico cosi si può fare un order by facilmente */
	public enum FaseDelGiorno : short {

		Mattino = 1,
		Pomeriggio = 2,
		Sera = 3
	};


	public static class FaseDelGiornoUtil {

		public static EnumConverter _enumConverter = new EnumConverter( typeof(FaseDelGiorno) );

		public static FaseDelGiorno getFaseDelGiorno( short faseValue ) {
			if( Enum.IsDefined(typeof(FaseDelGiorno), faseValue) ) {
				return (FaseDelGiorno)faseValue;		
			} else {
				throw new Exception("WaWaOoops!");
			}
		}

		public static FaseDelGiorno getFaseDelGiorno( DateTime timestamp ) {

			FaseDelGiorno fase = FaseDelGiorno.Sera;
			
			if( timestamp.Hour >= 14 && timestamp.Hour < 20)
				fase = FaseDelGiorno.Pomeriggio;
			else if( timestamp.Hour >= 5 )
				fase = FaseDelGiorno.Mattino;

			return fase;
		}

		public static readonly FaseDelGiorno [] fasiDelGiorno = { FaseDelGiorno.Mattino, FaseDelGiorno.Pomeriggio, FaseDelGiorno.Sera };

		public static T faseDelGiornoTypeTo<T>( object value ) {
			Type conversionType = typeof( T );

			if( !conversionType.IsGenericType && conversionType.IsEnum ) {
				return (T)Enum.ToObject( conversionType, value );
			}

			try {
				return (T)Convert.ChangeType( value, conversionType );
			} catch {
				return default( T );
			}
		}

		/// <summary>
		/// Dato il valore short, ritorna la corrispondente stringa della FaseDelGiorno
		/// </summary>
		/// <param name="valoreFase">lo short che rappresenta la fase del giorno. Può essere anche nullabile</param>
		/// <returns></returns>
		public static string valoreToString( short? valoreFase ) {
			if( valoreFase == null )
				return null;
			FaseDelGiorno f1 = FaseDelGiornoUtil.getFaseDelGiorno( (short)valoreFase );
			return FaseDelGiornoUtil.faseDelGiornoTypeTo<string>( f1 );
		}

	}


}
