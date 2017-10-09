using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Plugin.LocalNotifications.Abstractions;
using System;
using System.Linq;

namespace Plugin.LocalNotifications
{
    /// <summary>
    /// Local Notifications implementation for UWP, WinRT and Windows Phone Silverlight
    /// </summary>
    public class LocalNotificationsImplementation : ILocalNotifications
    {
        private const string _TOAST_TEXT02_TEMPLATE = "<toast>"
                                                    + "<visual>"
                                                    + "<binding template='ToastText02'>"
                                                    + "<text id='1'>{0}</text>"
                                                    + "<text id='2'>{1}</text>"
                                                    + "</binding>"
                                                    + "</visual>"
                                                    + "</toast>";

        /// <summary>
		/// Show a local notification
		/// </summary>
		/// <param name="title">Title of the notification</param>
		/// <param name="body">Body or description of the notification</param>
		/// <param name="id">Id of the notification</param>
        public void Show(string title, string body, int id = 0, string launchArgs = null)
        {
            var xmlDoc = GetXmlDoc(title, body, launchArgs);

            // Create a toast
            var toast = new ToastNotification(xmlDoc);
            // Create a toast notifier and show the toast
            var manager = ToastNotificationManager.CreateToastNotifier();

            manager.Show(toast);
        }

        /// <summary>
		/// Show a local notification at a specified time
		/// </summary>
		/// <param name="title">Title of the notification</param>
		/// <param name="body">Body or description of the notification</param>
		/// <param name="id">Id of the notification</param>
		/// <param name="notifyTime">Time to show notification</param>
        public void Show(string title, string body, int id, DateTime notifyTime, string launchArgs = null)
        {
            var xmlDoc = GetXmlDoc(title, body, launchArgs);

            var correctedTime = notifyTime <= DateTime.Now
              ? DateTime.Now.AddMilliseconds(100)
              : notifyTime;

            var scheduledTileNotification = new ScheduledTileNotification(xmlDoc, correctedTime)
            {
                Id = id.ToString()
            };

            TileUpdateManager.CreateTileUpdaterForApplication().AddToSchedule(scheduledTileNotification);
        }

        public void SetBadge(int num)
        {
            // Get the blank badge XML payload for a badge number
            XmlDocument badgeXml =
                BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);

            // Set the value of the badge in the XML to our number
            XmlElement badgeElement = badgeXml.SelectSingleNode("/badge") as XmlElement;
            badgeElement.SetAttribute("value", num.ToString());

            // Create the badge notification
            BadgeNotification badge = new BadgeNotification(badgeXml);

            // Create the badge updater for the application
            BadgeUpdater badgeUpdater =
                BadgeUpdateManager.CreateBadgeUpdaterForApplication();

            // And update the badge
            badgeUpdater.Update(badge);
        }

        private static XmlDocument GetXmlDoc(string title, string body, string launchArgs)
        {
            var xmlData = string.Format(_TOAST_TEXT02_TEMPLATE, title, body);

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            if (string.IsNullOrEmpty(launchArgs) == false)
            {
                var n = xmlDoc.SelectSingleNode("toast");
                var launchAttribute = xmlDoc.CreateAttribute("launch");
                launchAttribute.Value = launchArgs;
                n.Attributes.SetNamedItem(launchAttribute);
            }
            return xmlDoc;
        }

        /// <summary>
        /// Cancel a local notification
        /// </summary>
        /// <param name="id">Id of the notification to cancel</param>
        public void Cancel(int id)
        {
            var scheduledNotifications = TileUpdateManager.CreateTileUpdaterForApplication().GetScheduledTileNotifications();
            var notification =
                scheduledNotifications.FirstOrDefault(n => n.Id.Equals(id.ToString(), StringComparison.OrdinalIgnoreCase));

            if (notification != null)
            {
                TileUpdateManager.CreateTileUpdaterForApplication().RemoveFromSchedule(notification);
            }
        }
    }
}
