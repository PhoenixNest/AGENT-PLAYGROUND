using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace OrbitalSalvage.Viewer.PlayTests
{
    public class ShipPlayTests
    {
        [UnityTest]
        public IEnumerator Play_BootsPainterWithoutLegacyCanvas()
        {
            yield return SceneManager.LoadSceneAsync("MainGameScene", LoadSceneMode.Single);
            yield return null;
            yield return null;
            Assert.IsNotNull(Object.FindAnyObjectByType<ParityViewer>());
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assert.AreEqual(0, canvases.Length);
        }
    }
}
