using System;

namespace FSMForUnity
{
	[Flags]
    public enum FSMMachineFlags
    {
        /// <summary>
        /// When the State Machine is enabled, revert back to the default state.
        /// </summary>
        ResetOnEnable = 1 << 0,
        /// <summary>
        /// If Enable is called on the State Machine while the machine is enabled,
        /// treat that as though the machine was disabled, then re-enabled.
        /// </summary>
        TreatRedundantEnableAsReset = 1 << 1,
        /// <summary>
        /// When on, the machine will automatically output what state it is in into
        /// the console whenever any transition in the machine occur.
        /// </summary>
        DebugMode = 1 << 2,

        Default = ResetOnEnable | TreatRedundantEnableAsReset
    }

}
