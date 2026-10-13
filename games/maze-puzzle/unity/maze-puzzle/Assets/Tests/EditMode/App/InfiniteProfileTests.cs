using System.Collections.Generic;
using NUnit.Framework;
using OrbitalSalvage.JsCompat;
using OrbitalSalvage.Logic;

namespace OrbitalSalvage.App.Tests
{
    public class InfiniteProfileTests
    {
        [Test]
        public void DefaultProfile_OmitsInfiniteKeys()
        {
            string text = Profile.CreateDefault().ToText();
            StringAssert.DoesNotContain("infiniteRound", text);
            StringAssert.DoesNotContain("infiniteBest", text);
        }

        [Test]
        public void InfiniteFields_RoundTrip()
        {
            Profile profile = Profile.CreateDefault();
            profile.InfiniteRound = 7;
            profile.InfiniteBest = 12;
            string text = profile.ToText();
            Profile back = Profile.FromParsed(JsonParser.Parse(text), "2099-01-01");
            Assert.AreEqual(7d, back.InfiniteRound, 0d);
            Assert.AreEqual(12d, back.InfiniteBest, 0d);
        }

        [Test]
        public void OldSave_LoadsZeroInfiniteAndLeavesExtraKeys()
        {
            string old = Profile.CreateDefault().ToText();
            Profile loaded = Profile.FromParsed(JsonParser.Parse(old), "2099-01-01");
            Assert.AreEqual(0d, loaded.InfiniteRound, 0d);
            Assert.AreEqual(0d, loaded.InfiniteBest, 0d);
            Assert.AreEqual(0, loaded.ExtraKeys.Count);
            Assert.AreEqual(old, loaded.ToText());
        }

        [Test]
        public void InfiniteRound_DoesNotLandInExtraKeys()
        {
            Profile profile = Profile.CreateDefault();
            profile.InfiniteRound = 4;
            profile.ExtraKeys.Add(new KeyValuePair<string, JsonValue>("mystery", JsonValue.FromBool(true)));
            Profile back = Profile.FromParsed(JsonParser.Parse(profile.ToText()), "2099-01-01");
            Assert.AreEqual(1, back.ExtraKeys.Count);
            Assert.AreEqual("mystery", back.ExtraKeys[0].Key);
            for (int i = 0; i < back.ExtraKeys.Count; i++)
            {
                Assert.AreNotEqual("infiniteRound", back.ExtraKeys[i].Key);
                Assert.AreNotEqual("infiniteBest", back.ExtraKeys[i].Key);
            }
        }
    }
}
