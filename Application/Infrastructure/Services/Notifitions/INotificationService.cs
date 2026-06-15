using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Infrastructure.Services.Notifitions
{
    public interface INotificationService
    {
        Task CreateAsync(string userId, string title, string url);
    }
}
