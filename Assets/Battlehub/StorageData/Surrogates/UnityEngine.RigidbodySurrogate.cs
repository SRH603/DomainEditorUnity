using ProtoBuf;
using System;
using System.Threading.Tasks;

namespace Battlehub.Storage.Surrogates.UnityEngine
{
    [ProtoContract]
    [Surrogate(typeof(global::UnityEngine.Rigidbody), _PROPERTY_INDEX, _TYPE_INDEX)]
    public class RigidbodySurrogate<TID> : ISurrogate<TID> where TID : IEquatable<TID>
    {   
        const int _PROPERTY_INDEX = 25;
        const int _TYPE_INDEX = 149;

        //_PLACEHOLDER_FOR_EXTENSIONS_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        [ProtoMember(2)]
        public TID id { get; set; }

        [ProtoMember(3)]
        public TID gameObjectId { get; set; }

        [ProtoMember(4)]
        public global::UnityEngine.Vector3 velocity { get; set; }

        [ProtoMember(5)]
        public global::UnityEngine.Vector3 angularVelocity { get; set; }

        [ProtoMember(6)]
        public global::System.Single drag { get; set; }

        [ProtoMember(7)]
        public global::System.Single angularDrag { get; set; }

        [ProtoMember(8)]
        public global::System.Single mass { get; set; }

        [ProtoMember(9)]
        public global::System.Boolean useGravity { get; set; }

        [ProtoMember(10)]
        public global::System.Single maxDepenetrationVelocity { get; set; }

        [ProtoMember(11)]
        public global::System.Boolean isKinematic { get; set; }

        [ProtoMember(12)]
        public global::System.Boolean freezeRotation { get; set; }

        [ProtoMember(13)]
        public global::UnityEngine.RigidbodyConstraints constraints { get; set; }

        [ProtoMember(14)]
        public global::UnityEngine.CollisionDetectionMode collisionDetectionMode { get; set; }

        [ProtoMember(15)]
        public global::UnityEngine.Vector3 centerOfMass { get; set; }

        [ProtoMember(16)]
        public global::UnityEngine.Quaternion inertiaTensorRotation { get; set; }

        [ProtoMember(17)]
        public global::UnityEngine.Vector3 inertiaTensor { get; set; }

        [ProtoMember(18)]
        public global::System.Boolean detectCollisions { get; set; }

        [ProtoMember(19)]
        public global::UnityEngine.Vector3 position { get; set; }

        [ProtoMember(20)]
        public global::UnityEngine.Quaternion rotation { get; set; }

        [ProtoMember(21)]
        public global::UnityEngine.RigidbodyInterpolation interpolation { get; set; }

        [ProtoMember(22)]
        public global::System.Int32 solverIterations { get; set; }

        [ProtoMember(23)]
        public global::System.Single sleepThreshold { get; set; }

        [ProtoMember(24)]
        public global::System.Single maxAngularVelocity { get; set; }

        [ProtoMember(25)]
        public global::System.Int32 solverVelocityIterations { get; set; }

        //_PLACEHOLDER_FOR_NEW_PROPERTIES_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

        public ValueTask Serialize(object obj, ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = (global::UnityEngine.Rigidbody)obj;
            id = idmap.GetOrCreateID(o);
            gameObjectId = idmap.GetOrCreateID(o.gameObject);
            angularVelocity = o.angularVelocity;
#if UNITY_6000_0_OR_NEWER
            velocity = o.linearVelocity;
            drag = o.linearDamping;
            angularDrag = o.angularDamping;
#else
            velocity = o.velocity;
            drag = o.drag;
            angularDrag = o.angularDrag;
#endif
            mass = o.mass;
            useGravity = o.useGravity;
            maxDepenetrationVelocity = o.maxDepenetrationVelocity;
            isKinematic = o.isKinematic;
            freezeRotation = o.freezeRotation;
            constraints = o.constraints;
            collisionDetectionMode = o.collisionDetectionMode;
            centerOfMass = o.centerOfMass;
            inertiaTensorRotation = o.inertiaTensorRotation;
            inertiaTensor = o.inertiaTensor;
            detectCollisions = o.detectCollisions;
            position = o.position;
            rotation = o.rotation;
            interpolation = o.interpolation;
            solverIterations = o.solverIterations;
            sleepThreshold = o.sleepThreshold;
            maxAngularVelocity = o.maxAngularVelocity;
            solverVelocityIterations = o.solverVelocityIterations;
            //_PLACEHOLDER_FOR_SERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return default;
        }

        public ValueTask<object> Deserialize(ISerializationContext<TID> ctx)
        {
            var idmap = ctx.IDMap;

            var o = idmap.GetComponent<global::UnityEngine.Rigidbody, TID>(id, gameObjectId);
            o.angularVelocity = angularVelocity;
#if UNITY_6000_0_OR_NEWER
            o.linearVelocity = velocity;
            o.linearDamping = drag;
            o.angularDamping = angularDrag;
#else
            o.velocity = velocity;
            o.drag = drag;
            o.angularDrag = angularDrag;
#endif
            o.mass = mass;
            o.useGravity = useGravity;
            o.maxDepenetrationVelocity = maxDepenetrationVelocity;
            o.isKinematic = isKinematic;
            o.freezeRotation = freezeRotation;
            o.constraints = constraints;
            o.collisionDetectionMode = collisionDetectionMode;
            o.centerOfMass = centerOfMass;
            o.inertiaTensorRotation = inertiaTensorRotation;
            o.inertiaTensor = inertiaTensor;
            o.detectCollisions = detectCollisions;
            o.position = position;
            o.rotation = rotation;
            o.interpolation = interpolation;
            o.solverIterations = solverIterations;
            o.sleepThreshold = sleepThreshold;
            o.maxAngularVelocity = maxAngularVelocity;
            o.solverVelocityIterations = solverVelocityIterations;
            //_PLACEHOLDER_FOR_DESERIALIZE_METHOD_BODY_DO_NOT_DELETE_OR_CHANGE_THIS_LINE_PLEASE

            return new ValueTask<object>(o);
        }
    }
}
