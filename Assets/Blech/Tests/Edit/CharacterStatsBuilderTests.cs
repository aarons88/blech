using NUnit.Framework;
using UnityEditor;
using Blech.Editor;
using Blech.Player;

namespace Blech.Tests
{
    public class CharacterStatsBuilderTests
    {
        [Test]
        public void Build_CreatesBeanPeaCarrot_Assets()
        {
            CharacterStatsBuilder.BuildAll();

            var bean = AssetDatabase.LoadAssetAtPath<PlayerCharacterStats>("Assets/Blech/ScriptableObjects/Characters/Bean.asset");
            var pea = AssetDatabase.LoadAssetAtPath<PlayerCharacterStats>("Assets/Blech/ScriptableObjects/Characters/Pea.asset");
            var carrot = AssetDatabase.LoadAssetAtPath<PlayerCharacterStats>("Assets/Blech/ScriptableObjects/Characters/CarrotChunk.asset");

            Assert.IsNotNull(bean, "Bean asset missing");
            Assert.IsNotNull(pea, "Pea asset missing");
            Assert.IsNotNull(carrot, "CarrotChunk asset missing");

            Assert.AreEqual("Bean", bean.displayName);
            Assert.AreEqual("Pea", pea.displayName);
            Assert.AreEqual("Carrot Chunk", carrot.displayName);

            Assert.Greater(pea.moveSpeed, bean.moveSpeed, "Pea should be faster than Bean");
            Assert.Greater(carrot.gripStrength, bean.gripStrength, "Carrot should grip harder than Bean");
        }
    }
}
