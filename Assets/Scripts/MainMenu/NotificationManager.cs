using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_ANDROID
    using Unity.Notifications.Android;
#endif
#if UNITY_IOS
    using Unity.Notifications.iOS;
#endif

public class NotificationManager : MonoBehaviour
{
    private string[] _notificationStr = {
        "The other players are waiting for you to play!",
        "You are close to next place on the leaders chart!",
        "You have untapped energy. Don't waste your time. Let's play!",
        "Your bossy bag get bored. Let's fill it up"
        };

    private string[] _notificationTitle = {
        "Hi there!",
        "Yo!",
        "Hey!",
        "What's up?",
        };

    public void PrepareNotification()
    {
        #if UNITY_ANDROID

            AndroidNotificationChannel notificationChannel = new AndroidNotificationChannel()
            {
                Id = "example_channel_id",
                Name = "Default channel",
                Importance = Importance.High,
                Description = "Generic notification",
            };

            AndroidNotificationCenter.RegisterNotificationChannel (notificationChannel);
            AndroidNotification notification = new AndroidNotification ();
            notification.Title = _notificationTitle[UnityEngine.Random.Range(0, _notificationTitle.Length - 1)];
            notification.Text = _notificationStr[UnityEngine.Random.Range(0, _notificationStr.Length - 1)];
            notification.SmallIcon = "icon_1";
            notification.LargeIcon = "icon_2";
            notification.ShowTimestamp = true;
            notification.FireTime = System.DateTime.Now.AddMinutes(30);

            var identifier = AndroidNotificationCenter.SendNotification (notification, "example_channel_id");

            if (AndroidNotificationCenter.CheckScheduledNotificationStatus(identifier) == NotificationStatus.Scheduled)
            {
                AndroidNotificationCenter.CancelAllNotifications ();

                AndroidNotificationCenter.SendNotification (notification, "example_channel_id");
            }

        #endif

        #if UNITY_IOS
 
            var timeTrigger = new iOSNotificationTimeIntervalTrigger()
            {
                TimeInterval = new TimeSpan(0, 30, 0),
                Repeats = false
            };

            var iosNotification = new iOSNotification()
            {
                // You can specify a custom identifier which can be used to manage the notification later.
                // If you don't provide one, a unique string will be generated automatically.
                Identifier = "example_channel_id",
                Title = "Hello there!",
                Body = "Scheduled at: " + DateTime.Now.ToShortDateString() + " triggered in 5 seconds",
                Subtitle = _notificationStr[UnityEngine.Random.Range(0, _notificationStr.Length - 1)],
                ShowInForeground = true,
                ForegroundPresentationOption = (PresentationOption.Alert | PresentationOption.Sound),
                CategoryIdentifier = "category_a",
                ThreadIdentifier = "thread1",
                Trigger = timeTrigger,
            };

            iOSNotificationCenter.ScheduleNotification(iosNotification);

        #endif
    }
}
