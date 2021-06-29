﻿using System;
using ATE.Core.Entities.Billings;
using ATE.Core.Enums;
using ATE.Helpers;

namespace ATE
{
    public class CallPresenter
    {
        private readonly CallReporter _callReporter;
        public CallPresenter(CallReporter callReporter)
        {
            _callReporter = callReporter;
        }

        public void Present(CallSort callSort = CallSort.Date)
        {
            foreach (var call in _callReporter.Render(callSort))
            {
                string typeOfCall = call.CallType == CallType.Incoming ? "Входящий" : "Исходящий";
                ConsoleColor consoleColor = call.CallType == CallType.Incoming ? ConsoleColor.Blue : ConsoleColor.Green;
                
                ConsoleEx.WriteLineWithColor($"Дата: {call.CallDate:g}; Продолжительность: {call.Duration:F2} m.; Стоимость: {call.Price:C2}; " +
                                             $"Абонент: {call.DestinationPhoneNumber}; Тип звонка: {typeOfCall}", consoleColor);
            }
        }
    }
}