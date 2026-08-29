using System.Collections.Generic;
using UnityEngine;

namespace MonMulti.Notification
{
    internal class NotificationManager
    {
        private class Notification
        {
            public string Title;
            public string Message;
            public float CreatedAt;
            public float Duration;
        }

        private readonly List<Notification> _notifications = new();

        public void NewNotification(string title, string message, float duration = 4f)
        {
            _notifications.Add(new Notification
            {
                Title = title,
                Message = message,
                CreatedAt = Time.unscaledTime,
                Duration = duration
            });
        }

        public void OnGUI()
        {
            for (int i = _notifications.Count - 1; i >= 0; i--)
            {
                var notification = _notifications[i];
                float age = Time.unscaledTime - notification.CreatedAt;

                if (age >= notification.Duration + 0.35f)
                {
                    _notifications.RemoveAt(i);
                    continue;
                }

                float y = Screen.height - 100f - i * 90f;
                float x;

                if (age < 0.35f)
                {
                    float t = age / 0.35f;
                    x = Mathf.Lerp(-350f, 20f, 1f - Mathf.Pow(1f - t, 3f));
                }
                else if (age > notification.Duration)
                {
                    float t = (age - notification.Duration) / 0.35f;
                    x = Mathf.Lerp(20f, -350f, t * t * t);
                }
                else
                {
                    x = 20f;
                }

                GUI.Box(new Rect(x, y, 350f, 80f), "");
                GUI.Label(new Rect(x + 15f, y + 10f, 320f, 25f), notification.Title);
                GUI.Label(new Rect(x + 15f, y + 35f, 320f, 35f), notification.Message);
            }
        }
    }
}