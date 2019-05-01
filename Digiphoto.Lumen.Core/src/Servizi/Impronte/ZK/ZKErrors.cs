using libzkfpcsharp;
using System.Collections.Generic;

namespace Digiphoto.Lumen.Core.Servizi.Impronte.ZK {

	public class ZKErrors {

		private static Dictionary<int, string> descriz;

		static ZKErrors() {

			descriz = new Dictionary<int, string>();

			/*   0 */ descriz.Add( zkfp.ZKFP_ERR_OK,				"Operazione conclusa con successo" );
			/*   1 */ descriz.Add( zkfp.ZKFP_ERR_ALREADY_INIT,		"Già inizializzato" );
			/*  -1 */ descriz.Add( zkfp.ZKFP_ERR_INITLIB,			"Fallita inizializzazione dell'algoritmo" );
			/*  -2 */ descriz.Add( zkfp.ZKFP_ERR_INIT,				"Fallita inizializzazione della libreria di scansione" );
			/*  -3 */ descriz.Add( zkfp.ZKFP_ERR_NO_DEVICE,			"Nessun dispositivo collegato" );
			/*  -4 */ descriz.Add( zkfp.ZKFP_ERR_NOT_SUPPORT,		"Non supportato dalla interfaccia" );
			/*  -5 */ descriz.Add( zkfp.ZKFP_ERR_INVALID_PARAM,		"Parametro errato" );
			/*  -6 */ descriz.Add( zkfp.ZKFP_ERR_OPEN,				"Impossibile avviare il dispositvo" );
			/*  -7 */ descriz.Add( zkfp.ZKFP_ERR_INVALID_HANDLE,	"Invalid handle" );
			/*  -8 */ descriz.Add( zkfp.ZKFP_ERR_CAPTURE,			"Fallita scansione immagine" );
			/*  -9 */ descriz.Add( zkfp.ZKFP_ERR_EXTRACT_FP,		"Errore estrazione template dalla impronta" );
			/* -10 */ descriz.Add( zkfp.ZKFP_ERR_ABSORT,			"Sospesione" );
			/* -11 */ descriz.Add( zkfp.ZKFP_ERR_MEMORY_NOT_ENOUGH, "Memoria insufficiente" );
			/* -12 */ descriz.Add( zkfp.ZKFP_ERR_BUSY,				"L'impronta sta per essere scansionata" );
			/* -13 */ descriz.Add( zkfp.ZKFP_ERR_ADD_FINGER,		"Impossibile aggiungere il template dell'impronta" );
			/* -14 */ descriz.Add( zkfp.ZKFP_ERR_DEL_FINGER,		"Fallita la cancellazione del template dell'impronta" );
			/* -17 */ descriz.Add( zkfp.ZKFP_ERR_FAIL,				"Operazione fallita" );
			/* -18 */ descriz.Add( zkfp.ZKFP_ERR_CANCEL,			"Scansione annullata" );
			/* -20 */ descriz.Add( zkfp.ZKFP_ERR_VERIFY_FP,			"Confronto impronte fallita" );
			/* -22 */ descriz.Add( zkfp.ZKFP_ERR_MERGE,				"Fallita la combinazione dei template delle impronte" );
			/* -23 */ descriz.Add( zkfp.ZKFP_ERR_NOT_OPENED,		"Dispositivo non avviato" );
			/* -24 */ descriz.Add( zkfp.ZKFP_ERR_NOT_INIT,			"Non inizializzato" );
			/* -25 */ descriz.Add( zkfp.ZKFP_ERR_ALREADY_OPENED,	"Dispositivo avviato" );

		}

		public static string getDescrizione( int codiceErrore ) {
			return descriz[codiceErrore];
		}
	}
}
