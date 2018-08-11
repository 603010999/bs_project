using UnityEngine;
using UnityEditor;
using NUnit.Framework;

namespace Framework
{
    public class UITest
    {

        [Test(Description="UIManager 加载测试")]
        public void UIManagerLoadTest()
        {
            ResourcesConfigManager.Initialize();

            var manager = GameObjectManager.Instance.CreatePoolObject("UIManager");

            Assert.AreNotEqual(manager.GetComponent<UILayerManager>(), null);
            Assert.AreNotEqual(manager.GetComponent<UIAnimManager>(), null);
            Assert.AreNotEqual(manager.GetComponentInChildren<Camera>(), null);
        }
    }
}

