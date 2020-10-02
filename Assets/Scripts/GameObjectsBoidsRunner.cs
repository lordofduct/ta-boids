﻿using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

using URandom = UnityEngine.Random;

namespace ThousandAnt.Boids {

    public class GameObjectsBoidsRunner : MonoBehaviour {

        public Transform FlockMember;

        public BoidWeights Settings    = BoidWeights.Default();
        public float SeparationDistance  = 10f;
        public float Radius              = 20;
        public int   Size                = 512;
        public float MaxSpeed            = 2f;
        public float RotationSpeed       = 4f;

        [Header("Goal Setting")]
        public Transform Destination;

        [Header("Tendency")]
        public float3 Wind;

        private NativeArray<float> noiseOffsets;
        private NativeArray<float4x4> srcMatrices;
        private Transform[] transforms;
        private TransformAccessArray transformAccessArray;
        private JobHandle boidsHandle;

        private void Start() {
            transforms   = new Transform[Size];
            srcMatrices  = new NativeArray<float4x4>(transforms.Length, Allocator.Persistent);
            noiseOffsets = new NativeArray<float>(transforms.Length, Allocator.Persistent);

            for (int i = 0; i < Size; i++) {
                var pos         = transform.position + URandom.insideUnitSphere * Radius;
                var rotation    = Quaternion.Slerp(transform.rotation, URandom.rotation, 0.3f);
                transforms[i]   = GameObject.Instantiate(FlockMember, pos, rotation) as Transform;
                srcMatrices[i]  = transforms[i].localToWorldMatrix;
                noiseOffsets[i] = URandom.value * 10f;
            }

            transformAccessArray = new TransformAccessArray(transforms);
        }

        private void OnDisable() {
            boidsHandle.Complete();

            if (srcMatrices.IsCreated) {
                srcMatrices.Dispose();
            }

            if (noiseOffsets.IsCreated) {
                noiseOffsets.Dispose();
            }
        }

        private unsafe void Update() {
            boidsHandle.Complete();
            boidsHandle       = new BatchedJob {
                Settings      = Settings,
                Goal          = Destination.position,
                NoiseOffsets  = noiseOffsets,
                Time          = Time.time,
                DeltaTime     = Time.deltaTime,
                MaxDist       = SeparationDistance,
                Speed         = MaxSpeed,
                RotationSpeed = RotationSpeed,
                Size          = srcMatrices.Length,
                Src           = (float4x4*)(srcMatrices.GetUnsafePtr())
            }.Schedule(transforms.Length, 32, boidsHandle);

            boidsHandle = new CopyTransformJob {
                Src = srcMatrices
            }.Schedule(transformAccessArray, boidsHandle);
        }
    }
}
