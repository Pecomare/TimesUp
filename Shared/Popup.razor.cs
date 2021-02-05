using Microsoft.AspNetCore.Components;

namespace TimesUp.Shared
{
	public class PopupComponent : ComponentBase
	{
		public enum PopupSeverity
		{
			Success,
			Warning,
			Error
		}

		protected string? Message { get; set; }

		protected PopupSeverity Severity { get; set; }

		protected string? PopupClass { get; set; }

		/// <summary>
		/// Display the popup.
		/// </summary>
		/// <param name="message">The message to show</param>
		/// <param name="severity">The severity of the message</param>
		/// <param name="duration">How long should the popup be shown, in milliseconds</param>
		public void ShowPopup(string message, PopupSeverity severity, long duration = 3000)
		{
			Message = message;
			Severity = severity;
		}

		public void HidePopup()
		{

		}
	}
}