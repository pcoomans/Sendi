//*************************************************************************
// Project:      Sendi
// Class Name:   Utilities
// Description:  Class with static helper functions.
//               Many helper function which are generally useful throughout the entire application
//               are gathered here.
//*************************************************************************

using System;

namespace Sendi.Util
{

    /// <summary>
    /// Class with static helper functions.
    /// Helper function which are generally useful throughout the entire application
    /// are gathered here.
    /// </summary>
    public class Utilities
    {
        private static Int32 msgSequenceNr = 0;
        public static Int32 GetNextMsgSequenceNr()
        {
            Utilities.msgSequenceNr++;
            if (Utilities.msgSequenceNr>100000)
            {
                Utilities.msgSequenceNr = 1;
            }
            return Utilities.msgSequenceNr;
        }

        private static Int32 msgTypeNr = 0;
        public static Int32 CreateNewMsgTypeNr()
        {
            Utilities.msgTypeNr++;
            if (Utilities.msgTypeNr > 100000)
            {
                Utilities.msgTypeNr = 1;
            }
            return Utilities.msgTypeNr;
        }
    }
}
