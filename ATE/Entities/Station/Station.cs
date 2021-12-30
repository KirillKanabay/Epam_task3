﻿using System;
using ATE.Args;
using ATE.Entities.ATE;
using ATE.Entities.Billings;
using ATE.Entities.Port;
using ATE.Entities.Terminal;
using ATE.Enums;

namespace ATE.Entities.Station
{
    public class Station: BaseStation
    {
        private readonly IPortController _portController;
        public Station(IPortController portController)
        {
            _portController = portController;
        }
        public override BasePort ConnectTerminal(BaseTerminal terminal)
        {
            var port = _portController.GetAvailablePort();

            if (port != null)
            {
                if (!port.IsConnectedToStation)
                {
                    port.ConnectToStation(this);
                }
                port.ConnectToTerminal(terminal);
            }
            else
            {
                throw new Exception("Все доступные порты станции заняты. Попробуйте позже!");
            }
            return port;
        }

        public override void SubscribeToBillingSystem(BaseBillingSystem billingSystem)
        {
            billingSystem.CallAllowedEvent += OnCallAllowed;
            billingSystem.CallCanceledEvent += OnCallCanceled;
        }
        public override void OnTerminalStartedCall(object sender, CallArgs e)
        {
            BasePort port = _portController.GetByPhoneNumber(e.TargetNumber);

            if (port == null)
            {
                OnCallCanceled(this, new CallCanceledArgs()
                {
                    Message = "Wrong number",
                    SourcePhoneNumber = e.SourceNumber
                });

                return;
            }

            var args = new CallArgs()
            {
                Date = DateTime.Now,
                EndDate = e.EndDate,
                SourceNumber = e.SourceNumber,
                TargetNumber = e.TargetNumber,
                StartDate = e.StartDate,
                Status = CallStatus.Await,
            };

            port.HandleIncomingCall(sender, args);

            OnCallStartedEvent(sender, args);
        }
        public override void OnTerminalAcceptedCall(object sender, CallArgs e)
        {
            if (e.Status == CallStatus.Await)
            {
                var targetPort = _portController.GetByPhoneNumber(e.SourceNumber);

                if (targetPort == null)
                {
                    OnCallCanceled(this, new CallCanceledArgs()
                    {
                        Message = "Can't accept call",
                        SourcePhoneNumber = e.TargetNumber
                    });

                    return;
                }

                targetPort.HandleAcceptedCall(this, new CallArgs()
                {
                    Date = DateTime.Now,
                    EndDate = e.EndDate,
                    SourceNumber = e.SourceNumber,
                    TargetNumber = e.TargetNumber,
                    StartDate = e.StartDate,
                    Status = CallStatus.Accepted,
                });
            }
        }
        public override void OnTerminalRejectedCall(object sender, CallArgs e)
        {
            var targetPort = _portController.GetByPhoneNumber(e.SourceNumber);

            if (targetPort == null)
            {
                OnCallCanceled(this, new CallCanceledArgs()
                {
                    Message = "Can't reject call",
                    SourcePhoneNumber = e.TargetNumber
                });

                return;
            }


            targetPort.HandleRejectedCall(this, new CallArgs()
            {
                Date = e.Date,
                EndDate = e.EndDate,
                SourceNumber = e.SourceNumber,
                TargetNumber = e.TargetNumber,
                StartDate = e.StartDate,
                Status = CallStatus.Rejected,
            });
        }
        public override void OnTerminalEndedCall(object sender, CallArgs e)
        {
            if (sender is BaseTerminal terminal && e.Status == CallStatus.Accepted)
            {
                BasePort port = null;

                if (terminal.Number == e.SourceNumber)
                {
                    port = _portController.GetByPhoneNumber(e.TargetNumber);
                }
                else
                {
                    port = _portController.GetByPhoneNumber(e.SourceNumber);
                }

                var args = new CallArgs()
                {
                    Date = e.Date,
                    EndDate = DateTime.Now,
                    SourceNumber = e.SourceNumber,
                    TargetNumber = e.TargetNumber,
                    StartDate = e.StartDate,
                    Status = CallStatus.Ended,
                };

                port.HandleEndedCall(sender, args);

                OnCallEndedEvent(sender, args);
            }
        }
        public override void OnCallAllowed(object sender, CallArgs e)
        {
            var port = _portController.GetByPhoneNumber(e.TargetNumber);
            if (port == null)
            {
                OnCallCanceled(this, new CallCanceledArgs()
                {
                    Message = "Wrong number",
                    SourcePhoneNumber = e.SourceNumber
                });

                return;
            }

            port.HandleIncomingCall(sender, e);

        }
        public override void OnCallCanceled(object sender, CallCanceledArgs e)
        {
            var port = _portController.GetByPhoneNumber(e?.SourcePhoneNumber);

            port.HandleCanceledCall(sender, e);
        }

    }
}