using System;
using TrueSync;
using UnityEngine;

public interface IPhysicsManager
{
	TSVector Gravity
	{
		get;
		set;
	}

	bool SpeculativeContacts
	{
		get;
		set;
	}

	FP LockedTimeStep
	{
		get;
		set;
	}

	void Init();

	void UpdateStep();

	IWorld GetWorld();

	IWorldClone GetWorldClone();

	GameObject GetGameObject(IBody rigidBody);

	bool IsCollisionEnabled(IBody rigidBody1, IBody rigidBody2);

	void AddBody(ICollider iCollider);

	void RemoveBody(IBody iBody);

	void OnRemoveBody(Action<IBody> OnRemoveBody);

	bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out IBody body, out TSVector normal, out FP fraction);
}
