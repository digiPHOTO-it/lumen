using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using IMAPI2.Interop;
using System.IO;

namespace Digiphoto.Lumen.Servizi.Masterizza.MyBurner
{
    public interface IBurnerSrv : IServizio
	{
        void burning();

        void formatting();

        void addFileToBurner(String pathName);

        void setDiscRecorder(String volume);

        void setDiscRecorder(IDiscRecorder2 discRecorder);

        IList<MsftDiscRecorder2> listaMasterizzatori();

        void testMedia();
	}
}
