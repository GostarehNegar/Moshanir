using System;
using System.Collections.Generic;
using System.Text;

namespace GN.Library.Shared.Entities
{
    public class TaskEntity : XrmDynamicEntity
    {
        public new class Schema
        {
            public const string Subject = "subject";
            public const string LogicalName = "task";
            public const string Duration = "actualdurationminutes";
            public const string Priority = "prioritycode";
            public const string State = "statecode";
            public enum StateCodes
            {
                Open = 0,
                Completed = 1,
                Canceled = 2

            }
            public enum StatusCodes
            {
                /// <summary>
                /// NotsAtsrted=1 (State=Open)
                /// </summary>
                NotStarted = 2,
                /// <summary>
                /// InProgress=3 (Valid only for State=Open)
                /// </summary>
                InProgress = 3,
                // Completed
                /// <summary>
                /// WaitingOnSomeOneElse=4 (State=Open)
                /// </summary>
                WaitingOnSomeoneElse = 4,

                /// <summary>
                /// Completed=5 (State=Completed)
                /// </summary>
                Completed = 5,
                /// <summary>
                /// Canceled=6 (State=Cance4led)
                /// </summary>
                Canceled = 6,
                /// <summary>
                /// Defered =7 (State=Open)
                /// </summary>
                Defered = 7,


            }

        }
        public string Subject { get => this.GetAttributeValue(Schema.Subject); }
        public int Duration { get => this.GetAttributeValue<int>(Schema.Duration); }
        public int State { get => this.GetAttributeValue<int>(Schema.State); }
        public int Priority { get => this.GetAttributeValue<int>(Schema.Priority); }

    }
}
