using PGMS.CQSLight.Extensions;
using PGMS.CQSLight.Infra.Commands.Services;
using PGMS.CQSLight.Infra.Exceptions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace PGMS.CQSLight.Infra.Commands
{
    public interface IHandleCommand<T> where T : ICommand
    {
        void Execute(T command);

        void OnFail(T command);
    }

    public abstract class ErrorMessageGenerator
    {
        protected static string GetErrorMessage(IEnumerable<ValidationResult> commandValidationResult)
        {
            var sb = new StringBuilder();
            foreach (var result in commandValidationResult)
            {
                var prefix = "";
                if (result.MemberNames != null && result.MemberNames.Any())
                {
                    prefix = result.MemberNames.Aggregate(prefix, (current, memberName) => current + memberName + " ");
                    prefix += " - ";
                }
                sb.AppendLine(prefix + result.ErrorMessage);
            }
            return sb.ToString();
        }
    }

    public abstract class BaseCommandHandler<T> : ErrorMessageGenerator, IHandleCommand<T> where T : BaseCommand
    {
        private readonly IBus bus;

        protected BaseCommandHandler(IBus bus) => this.bus = bus;

        public abstract IEnumerable<ValidationResult> ValidateCommand(T command);

        public abstract IEvent ToEvent(T command);

        public void Execute(T command)
        {
            command.Validate(out List<ValidationResult> commandValidationResult);
            if (commandValidationResult != null && commandValidationResult.Any())
            {
                throw new DomainValidationException(GetErrorMessage(commandValidationResult));
            }

            var validationResults = ValidateCommand(command);
            if (validationResults != null && validationResults.Any())
            {
                throw new DomainValidationException(GetErrorMessage(validationResults.ToList()));
            }

            var @event = ToEvent(command);
            @event.ByUser = command.ByUsername;
            @event.AggregateId = command.AggregateRootId;
            bus.Publish(@event);
        }

        public void OnFail(T command)
        {
        }
    }

    public abstract class BaseCommandHandlerMultipleEvents<T> : ErrorMessageGenerator, IHandleCommand<T> where T : BaseCommand
    {
        private readonly IBus bus;

        protected BaseCommandHandlerMultipleEvents(IBus bus) => this.bus = bus;

        public abstract IEnumerable<ValidationResult> ValidateCommand(T command);

        public abstract List<IEvent> ToEvents(T command);

        public void Execute(T command)
        {
            command.Validate(out List<ValidationResult> commandValidationResult);
            if (commandValidationResult != null && commandValidationResult.Any())
            {
                throw new DomainValidationException(GetErrorMessage(commandValidationResult));
            }

            var validationResults = ValidateCommand(command);
            if (validationResults != null && validationResults.Any())
            {
                throw new DomainValidationException(GetErrorMessage(validationResults.ToList()));
            }
            var @events = ToEvents(command);
            foreach (var @event in events)
            {
                @event.ByUser = command.ByUsername;
                @event.AggregateId = command.AggregateRootId;
            }
            bus.Publish(@events);
        }

        public void OnFail(T command)
        {
        }
    }
}