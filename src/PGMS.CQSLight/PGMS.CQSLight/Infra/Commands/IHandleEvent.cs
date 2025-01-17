﻿using Microsoft.Extensions.Logging;
using PGMS.CQSLight.Extensions;
using PGMS.Data.Services;
using System;
using System.Text;

namespace PGMS.CQSLight.Infra.Commands
{
    public interface IHandleEvent<T> where T : IEvent
    {
        void Handle(T @event, IUnitOfWork unitOfWork);
    }

    public abstract class BaseEventHandler<T> : IHandleEvent<T> where T : IEvent
    {
        protected readonly IScopedEntityRepository entityRepository;
        private readonly ILogger<IEvent> logger;

        protected BaseEventHandler(IScopedEntityRepository entityRepository, ILogger<IEvent> logger)
        {
            this.entityRepository = entityRepository;
            this.logger = logger;
        }

        public void Handle(T @event, IUnitOfWork unitOfWork)
        {
            try
            {
                HandleEvent(@event, unitOfWork);
            }
            catch (Exception ex)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"Transaction commit failed on Handling {typeof(T).Name} - Handler : {GetType().FullName} - {ex.GetErrorDetails()}");
                logger.LogWarning(sb.ToString());
                throw;
            }
        }

        protected abstract void HandleEvent(T @event, IUnitOfWork unitOfWork);
    }
}