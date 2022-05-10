#if RELEASEPUNCH
using System.Collections.Generic;
using Rocket.Unturned.Player;
using UnityEngine;

namespace RFGarage
{
    public class PlayerComponent : UnturnedPlayerComponent
    {
        internal Coroutine ResetPunchCor { get; set; }
        internal uint PunchCount { get; set; }

        private IEnumerator<WaitForSeconds> ResetPunchCountEnumerator()
        {
            yield return new WaitForSeconds(60f);
            PunchCount = 0;
            ResetPunchCor = null;
        }

        internal void ResetPunchCount()
        {
            if (ResetPunchCor != null)
                Plugin.Inst.StopCoroutine(ResetPunchCor);

            ResetPunchCor = Plugin.Inst.StartCoroutine(ResetPunchCountEnumerator());
        }
    }
}
#endif