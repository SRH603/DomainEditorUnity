using System;
using UnityEngine;

namespace Battlehub.RTSL
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(RTSLIgnore))]
    public class RTSLDeps : RTSLDeps<long>
    {
      
    }

    public abstract class RTSLDeps<TID> : MonoBehaviour where TID : IEquatable<TID>
    {
    }
}

