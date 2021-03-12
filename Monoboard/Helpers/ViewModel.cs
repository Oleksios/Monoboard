using System.ComponentModel;

namespace Monoboard.Helpers
{
	public class ViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// Оновлює поле (властивість)
		/// </summary>
		/// <param name="propertyName">Поле (властивіть), яка змінюється</param>
		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null && !string.IsNullOrWhiteSpace(propertyName))
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Оновлює поля (властивості)
		/// </summary>
		/// <param name="propertyNames">Список полів (властивостей), що змінюються</param>
		protected void OnPropertyChanged(string[] propertyNames)
		{
			foreach (var propertyName in propertyNames)
				if (PropertyChanged != null && !string.IsNullOrWhiteSpace(propertyName))
					PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
