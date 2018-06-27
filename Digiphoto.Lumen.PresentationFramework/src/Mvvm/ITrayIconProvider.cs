using System;

namespace Digiphoto.Lumen.UI.Mvvm {

	public interface ITrayIconProvider {

		void showAbout(string title, String msg, int? sleep);

		void showAboutCloud(string title, String msg, int? sleep);

		void showError(string title, string msg, int? sleep);

		void showInfo(string title, String msg, int? sleep);

		void showWarning(string title, String msg, int? sleep);
    }
}