using System;
using System.Collections.Generic;
using NUnit.Framework;
using Src;
using UnityEngine;

namespace Tests
{
    public class Test
    {
        [Test]
        [TestCaseSource(nameof(VectorsData))]
        public void TestAngleBetweenVectors(Vector3 a, Vector3 b, float expected)
        {
            var actual = CalculusUtils.GetAngleBetweenVectors(a, b);
        
            Assert.AreEqual(expected, actual, 0.001f);
        }
    
        public static IEnumerable<TestCaseData> VectorsData()
        {
            yield return new TestCaseData(new Vector3(1, 0, 0), new Vector3(0, 1, 0), Mathf.PI / 2);
            yield return new TestCaseData(new Vector3(1, 0, 0), new Vector3(1, 0, 0), 0);
            yield return new TestCaseData(new Vector3(1, 0, 0), new Vector3(-1, 0, 0), Mathf.PI);
            yield return new TestCaseData(new Vector3(1, 0, 0), new Vector3(1, 1, 0), Mathf.PI / 4);
            yield return new TestCaseData(new Vector3(1, 0, 0), new Vector3(0, -1, 0), Mathf.PI / 2);
            yield return new TestCaseData(new Vector3(0, -1, 0), new Vector3(1, 0, 0), Mathf.PI / 2);
            yield return new TestCaseData(new Vector3(-1, 0, 0), new Vector3(0, -1, 0), Mathf.PI / 2);
        }
    }
}
