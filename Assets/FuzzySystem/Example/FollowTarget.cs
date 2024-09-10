using System;
using UnityEngine;

namespace FuzzySystem.Example
{
    public class FollowTarget : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Transform follower;
        [SerializeField] private TextAsset fuzzyData;

        private FuzzySystem _fuzzySystem;

        private void Awake()
        {
            _fuzzySystem = FuzzySystem.Deserialize(fuzzyData.bytes, null);
        }

        private void Update()
        {
            _fuzzySystem.evaluate = true;
            _fuzzySystem.GetFuzzificationByName("distance").value = Vector3.Distance(target.position, follower.position);

            float speed = _fuzzySystem.Output() * _fuzzySystem.defuzzification.maxValue;
            follower.position = Vector3.MoveTowards(follower.position, target.position, speed * Time.deltaTime);
        }
    }
}