using System;
using System.Runtime.InteropServices;
using TrueSync;
using UnityEngine;

public interface IPhysicsManager
{
    void AddBody(ICollider iCollider);
    GameObject GetGameObject(IBody rigidBody);
    IWorld GetWorld();
    IWorldClone GetWorldClone();
    void Init();
    bool IsCollisionEnabled(IBody rigidBody1, IBody rigidBody2);
    void OnRemoveBody(Action<IBody> OnRemoveBody);
    bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out IBody body, out TSVector normal, out FP fraction);
    void RemoveBody(IBody iBody);
    void UpdateStep();

    TSVector Gravity { get; set; }

    FP LockedTimeStep { get; set; }

    bool SpeculativeContacts { get; set; }
}

